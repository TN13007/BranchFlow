async page => {
 const api='https://127.0.0.1:17108', web='https://localhost:17133', http=page.context().request, checks=[];
 const check=(name,value)=>{if(!value)throw new Error(name);checks.push(name);};
 const login=async username=>{const r=await http.post(api+'/api/auth/login',{data:{username,password:'Demo@12345'}});check(username+' login',r.status()===200);return await r.json();};
 const get=(url,token)=>http.get(api+url,{headers:{Authorization:'Bearer '+token}});
 const reset=(id,token)=>http.post(api+'/api/management/users/'+id+'/reset-password',{headers:{Authorization:'Bearer '+token},data:{identityVerified:true,reason:'Người dùng quên mật khẩu'}});
 page.setDefaultTimeout(15000); await page.context().clearCookies();
 try {
 const admin=await login('demo.systemadmin'), owner=await login('demo.owner'), branch=await login('demo.branchadmin.01');
 const ownerMe=await (await get('/api/auth/me',owner.accessToken)).json();
 const adminMe=await (await get('/api/auth/me',admin.accessToken)).json();
 check('owner cannot read recovery requests',(await get('/api/management/password-requests',owner.accessToken)).status()===403);
 check('branch admin cannot read recovery requests',(await get('/api/management/password-requests',branch.accessToken)).status()===403);
 check('system admin cannot reset owner',(await reset(ownerMe.userId,admin.accessToken)).status()===403);
 check('system admin cannot reset system admin',(await reset(adminMe.userId,admin.accessToken)).status()===403);
 check('web forgot rejects missing CSRF token',(await http.post(web+'/auth/forgot-password',{form:{username:'unknown-security-test'}})).status()===400);
 check('web change requires login',(await http.post(web+'/auth/change-password',{form:{currentPassword:'none',newPassword:'irrelevant-test-password',confirmPassword:'irrelevant-test-password'},maxRedirects:0})).status()===302);
 let limited=false;
 for(let i=0;i<6;i++){const r=await http.post(api+'/api/auth/forgot-password',{data:{username:'unknown-security-test'}});if(r.status()===429){check('recovery rate limit publishes retry delay',r.headers()['retry-after']==='60');limited=true;break;}check('recovery accepts bounded attempts',r.status()===200);}
 check('recovery rate limit enforced',limited);
 await page.goto(web+'/login');await page.locator('#username').fill('demo.systemadmin');await page.locator('#password').fill('Demo@12345');await page.locator('.login-submit').click();await page.waitForURL('**/admin');
 check('authenticated password form rejects missing CSRF token',(await http.post(web+'/auth/change-password',{form:{currentPassword:'none',newPassword:'irrelevant-test-password',confirmPassword:'irrelevant-test-password'}})).status()===400);
 await page.setViewportSize({width:390,height:844});await page.goto(web+'/admin?tab=employees');await page.getByRole('heading',{name:'Hỗ trợ mật khẩu'}).waitFor();
 await page.getByRole('searchbox',{name:'Tìm tài khoản'}).fill('demo.employee.a');
 const row=page.getByRole('row').filter({has:page.getByRole('cell',{name:/demo.employee.a$/})});
 await row.getByRole('button',{name:'Đặt lại mật khẩu',exact:true}).click();
 check('row reset action selects intended user',await page.locator('#reset-target option:checked').textContent().then(s=>s.includes('demo.employee.a')));
 await page.locator('.password-support').scrollIntoViewIfNeeded();
 check('mobile admin support has no horizontal overflow',await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1));
 await page.screenshot({path:'admin-support-mobile.png'});
 return {result:'Pass',count:checks.length,checks};
 }catch(e){return {result:'Fail',completed:checks,error:e.message,url:page.url()};}
}
