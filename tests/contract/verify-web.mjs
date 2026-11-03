// Test-only API fixture + HTTP checks. Không chứng minh backend/SQL thật đã hoạt động.
// Chạy sau dotnet build -c Release: node tests/contract/verify-web.mjs
import http from 'node:http';
import https from 'node:https';
import { spawn } from 'node:child_process';
import { join, resolve } from 'node:path';
import assert from 'node:assert/strict';

const root = resolve(import.meta.dirname, '../..');
const sessions = new Map();
let logoutFails = false;
let refreshCount = 0;
let logoutCount = 0;
const now = () => new Date();
const pair = user => ({ accessToken: `test-access-${user}`, refreshToken: `test-refresh-${user}`,
    accessTokenExpiresAt: new Date(+now() + 900000).toISOString(), refreshTokenExpiresAt: new Date(+now() + 604800000).toISOString() });
const fixture = http.createServer(async (req, res) => {
    let body = ''; for await (const part of req) body += part;
    const data = body ? JSON.parse(body) : {};
    const token = req.headers.authorization?.replace('Bearer ', '');
    const user = token ? sessions.get(token) : undefined;
    const send = (status, value) => { res.writeHead(status, {'Content-Type':'application/json'}); res.end(value === undefined ? undefined : JSON.stringify(value)); };
    if (req.url === '/api/auth/login') {
        if (data.username === 'rate') return send(429, {status:429});
        if (data.password !== '  test-password  ' || !['one','two','owner'].includes(data.username)) return send(401, {status:401});
        const result = pair(data.username); sessions.set(result.accessToken, data.username); return send(200, result);
    }
    if (req.url === '/api/auth/refresh') { refreshCount++; return send(401, {status:401}); }
    if (!user) return send(401, {status:401});
    if (req.url === '/api/auth/me') return send(200, {userId:user === 'one' ? 1 : 2, username:user, role:user === 'owner' ? 'OWNER' : 'EMPLOYEE'});
    if (req.url === '/api/auth/logout') {
        logoutCount++;
        if (logoutFails) return send(500, {detail:'must never leak internal error'});
        sessions.delete(token); return send(204);
    }
    return send(404, {status:404});
});

function request(path, jar = new Map(), fields, port = 7134, secure = true) {
    return new Promise((resolveResponse, reject) => {
        const body = fields ? new URLSearchParams(fields).toString() : undefined;
        const client = secure ? https : http;
        const req = client.request({hostname:'localhost', port, path, method:body === undefined ? 'GET' : 'POST',
            // Test-only HTTPS client for the existing untrusted development certificate.
            rejectUnauthorized:false,
            headers:{Cookie:[...jar].map(([k,v])=>`${k}=${v}`).join('; '), ...(body === undefined ? {} : {'Content-Type':'application/x-www-form-urlencoded','Content-Length':Buffer.byteLength(body)})}
        }, res => {
            const cookies = res.headers['set-cookie'] ?? [];
            for (const cookie of cookies) { const [name,...value] = cookie.split(';')[0].split('='); jar.set(name,value.join('=')); }
            let text = ''; res.setEncoding('utf8'); res.on('data',d=>text+=d); res.on('end',()=>resolveResponse({status:res.statusCode,headers:res.headers,text,cookies}));
        });
        req.setTimeout(5000,()=>req.destroy(new Error('HTTP check timed out'))); req.on('error',reject); req.end(body);
    });
}
const csrf = page => {
    const tokens = [...page.text.matchAll(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/g)].map(x=>x[1]);
    assert.ok(tokens.length > 0, 'Antiforgery token rendered');
    assert.equal(new Set(tokens).size,1,'No conflicting antiforgery tokens');
    return tokens[0];
};
let checks = 0;
const pass = label => { checks++; console.log(`PASS ${label}`); };
const dotnet = process.platform === 'win32' ? join(process.env.LOCALAPPDATA,'Microsoft/dotnet/dotnet.exe') : 'dotnet';
let child;
try {
    await new Promise(r=>fixture.listen(19107,'127.0.0.1',r));
    child = spawn(dotnet,[join(root,'src/BranchFlow.Web/bin/Release/net10.0/BranchFlow.Web.dll'),'--urls','https://localhost:7134','--contentRoot',join(root,'src/BranchFlow.Web')],
        {cwd:root,windowsHide:true,env:{...process.env,ASPNETCORE_ENVIRONMENT:'Development',Api__BaseUrl:'http://127.0.0.1:19107/'},stdio:['ignore','pipe','pipe']});
    let logs = ''; child.stdout.on('data',d=>logs+=d); child.stderr.on('data',d=>logs+=d);
    let ready = false;
    for(let i=0;i<100;i++) { try { await request('/login'); ready=true; break; } catch { await new Promise(r=>setTimeout(r,100)); } }
    assert.ok(ready, `Web test host starts: ${logs.slice(-2000)}`);
    let response = await request('/auth/login',new Map(),{username:'one',password:'  test-password  '});
    assert.equal(response.status,400); pass('Missing antiforgery rejected');
    const invalidJar = new Map(); const invalidPage = await request('/login',invalidJar);
    response = await request('/auth/login',invalidJar,{username:'one',password:'incorrect',__RequestVerificationToken:csrf(invalidPage)});
    assert.equal(response.headers.location,'/login?error=credentials'); pass('Invalid credentials produce generic login error');
    response = await request('/auth/login',invalidJar,{username:'rate',password:'incorrect',__RequestVerificationToken:csrf(invalidPage)});
    assert.equal(response.headers.location,'/login?error=rate'); pass('Rate limit has explicit retry message');
    const one = new Map(), two = new Map();
    for (const [jar,user] of [[one,'one'],[two,'two']]) {
        const page = await request('/login',jar);
        response = await request('/auth/login',jar,{username:`  ${user}  `,password:'  test-password  ',__RequestVerificationToken:csrf(page)});
        assert.equal(response.headers.location,'/orders/create');
        const cookie = response.cookies.find(c=>c.startsWith('__Host-BranchFlow.Auth='));
        assert.ok(cookie?.toLowerCase().includes('secure')); assert.ok(cookie.toLowerCase().includes('httponly')); assert.ok(cookie.toLowerCase().includes('samesite=lax'));
        assert.ok(!cookie.includes('test-access') && !cookie.includes('test-refresh'));
    }
    pass('Login trims username, preserves password spaces, issues protected Secure/HttpOnly/Lax cookie');
    assert.notEqual(one.get('__Host-BranchFlow.Auth'),two.get('__Host-BranchFlow.Auth'));
    for(const [jar,user] of [[one,'one'],[two,'two']]) {
        const account = await request('/account',jar); assert.equal(account.status,200); assert.ok(account.text.includes(`<strong>${user}</strong>`));
        const refreshed = await request('/account',jar); assert.equal(refreshed.status,200);
    }
    pass('Two browser-equivalent cookie jars keep distinct users across repeated page requests');
    const owner = new Map(); const ownerPage = await request('/login',owner);
    response = await request('/auth/login',owner,{username:'owner',password:'  test-password  ',__RequestVerificationToken:csrf(ownerPage)});
    assert.equal(response.headers.location,'/account');
    response = await request('/orders/create',owner); assert.equal(response.status,302); assert.ok(response.headers.location.includes('/account'));
    pass('Non-employee user cannot access order creation');
    const account = await request('/account',one);
    response = await request('/auth/logout',one,{__RequestVerificationToken:csrf(account)});
    assert.equal(response.headers.location,'/login?notice=logout'); assert.equal(logoutCount,1);
    response = await request('/account',one); assert.equal(response.status,302);
    assert.equal((await request('/account',two)).status,200); pass('Logout invalidates current Web/API session and preserves other user');
    logoutFails = true;
    response = await request('/auth/logout',two,{__RequestVerificationToken:csrf(await request('/account',two))});
    assert.equal(response.headers.location,'/login?notice=local-logout'); assert.ok(!response.text.includes('must never leak'));
    assert.equal((await request('/account',two)).status,302); pass('API logout failure still clears local session and reports limitation');
    assert.equal(refreshCount,0); pass('Fresh sessions do not unnecessarily refresh');
    console.log(`${checks} HTTP contract-fixture checks passed. Backend/SQL acceptance remains Not Run.`);
} finally {
    child?.kill(); fixture.closeAllConnections(); await new Promise(r=>fixture.close(r));
}
