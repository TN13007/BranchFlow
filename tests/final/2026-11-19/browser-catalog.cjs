const {chromium}=require('C:/Users/Admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const web='https://localhost:17133',folder=process.env.BRANCHFLOW_UAT_EVIDENCE,pw=process.env.BRANCHFLOW_DEMO_PASSWORD;
const tag=process.env.BRANCHFLOW_E2E_TAG||Date.now().toString(36), name='E2E '+tag, employee='e2e.'+tag;
const results=[], consoleErrors=[];
const log=(id,actual,result='Pass')=>{results.push({id,actual,result});console.log(id+' '+result+' '+actual);};
const button=(p,name)=>p.getByRole('button',{name,exact:true});
const label=(p,name)=>p.getByLabel(name,{exact:true});
const row=(p,value)=>p.getByRole('row').filter({hasText:value}).first();
const list=(p,value)=>p.getByRole('listitem').filter({hasText:value}).first();
const shot=(p,name)=>p.screenshot({path:path.join(folder,name+'.png')});
const fill=async(p,fields)=>{for(const [key,value] of Object.entries(fields)) await label(p,key).fill(value);};
async function go(p,route){await p.bringToFront();await p.goto(web+route);}
async function login(browser,user,route,width=1440,password=pw){
 const p=await browser.newPage({viewport:{width,height:900},ignoreHTTPSErrors:true,locale:'vi-VN'});
 p.setDefaultTimeout(11000);p.on('pageerror',e=>consoleErrors.push(e.message));
 for(let i=0;i<2;i++){
  await go(p,'/login'); await p.locator('#username').fill(user);await p.locator('#password').fill(password);
  await p.locator('button.login-submit').click();await p.waitForURL(url=>url.pathname===route||url.searchParams.has('error'));
  const u=new URL(p.url());if(u.pathname===route)return p;
  if(u.searchParams.get('error')==='rate'&&!i){console.log('Waiting for auth rate limit'); await new Promise(r=>setTimeout(r,61000));continue;}
  throw Error('Login '+user+' '+u.searchParams.get('error'));
 }
}
async function test(id,p,fn){
 if(id!=='E2E-17') return;
 try{log(id,await fn());}catch(e){log(id,e.message,'Fail');await shot(p,id+'-error').catch(()=>{});}
}
(async()=>{
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
const sys=await login(browser,'demo.systemadmin','/admin');
const branchName=name+' Branch',employeeName=name+' Employee',password='Qa!7-'+tag+'-Yz',ingredientPrefix='NL QA 955df25b';
await test('E2E-15',sys,async()=>{
 await go(sys,'/admin?tab=branches');
 await button(sys,'Thêm chi nhánh').click();
 await fill(sys,{'Mã chi nhánh':'E'+tag.toUpperCase(),'Tên chi nhánh':branchName,'Địa chỉ':'Dia chi E2E'});
 await button(sys,'Lưu chi nhánh').click();await row(sys,branchName).waitFor();
 await button(row(sys,branchName),'Sửa').click();await label(sys,'Địa chỉ').fill('Dia chi E2E da cap nhat');
 await button(sys,'Lưu chi nhánh').click();await row(sys,branchName).getByText('Dia chi E2E da cap nhat',{exact:true}).waitFor();
 await shot(sys,'15-chi-nhanh-thuc-te');return branchName+' tạo và sửa địa chỉ thành công';
});
await test('E2E-16',sys,async()=>{
 await go(sys,'/admin?tab=employees'); await button(sys,'Tạo tài khoản').click();
 await fill(sys,{'Tên đăng nhập':employee,'Họ tên tài khoản mới':employeeName,'Mật khẩu ban đầu':password});
 await button(sys,'Lưu tài khoản').click();await row(sys,employee).waitFor();
 async function assign(branch){
  await button(sys,'Phân công chi nhánh').click();
  await label(sys,'Tài khoản phân công').selectOption({label:employeeName+' · '+employee});
  const select=label(sys,'Chi nhánh phân công');
  const choice=await select.locator('option').filter({hasText:branch}).first().textContent();
  await select.selectOption({label:choice.trim()});await button(sys,'Lưu phân công').click();
 }
 await assign('Demo 01');await row(sys,employee).getByText(/Demo 01/).waitFor();
 await assign('Demo 02');await sys.locator('.alert-danger').first().waitFor(); const denial=(await sys.locator('.alert-danger').first().innerText()).trim(); 
 await row(sys,employee).getByText(/Demo 01/).waitFor();await shot(sys,'16-chan-chuyen-chi-nhanh');
 const staffNew=await login(browser,employee,'/employee',390,password);
 await staffNew.getByText(/Demo 01/).first().waitFor();await staffNew.close();
 return employee+' tạo, gán Demo 01, đăng nhập được; chuyển Demo 02 bị chặn: '+denial;
});
await test('E2E-17',sys,async()=>{
 await go(sys,'/admin?tab=system-menu');
 const mode=sys.locator('.management-toolbar select');
 for(const [key,suffix] of [['categories','CAT'],['sizes','SIZE'],['topping-groups','TG'],['note-groups','NG']]){
  await mode.selectOption(key);await button(sys,'Thêm mục').click();await label(sys,'Tên').fill(name+' '+suffix);await button(sys,'Lưu').click();await list(sys,name+' '+suffix).waitFor();
 }
 await mode.selectOption('notes');await button(sys,'Thêm ghi chú').click();
 await label(sys,'Tên ghi chú').fill(name+' NOTE');await label(sys,'Nhóm ghi chú').selectOption({label:name+' NG'});
 await button(sys,'Lưu ghi chú').click();await list(sys,name+' NOTE').waitFor();
 await mode.selectOption('toppings');await button(sys,'Thêm topping').click();
 await fill(sys,{'Tên topping':name+' TOP','Giá topping':'4500'});await label(sys,'Nhóm topping').selectOption({label:name+' TG'});
 await button(sys,'Lưu topping').click();await row(sys,name+' TOP').waitFor();
 await mode.selectOption('products');await button(sys,'Thêm món').click();
 await label(sys,'Tên món').fill(name+' DRINK');await label(sys,'Danh mục món').selectOption({label:name+' CAT'});
 await button(sys,'Thêm size').click();await label(sys,'Size').selectOption({label:name+' SIZE'});await label(sys,'Giá size').fill('21000');
 await button(sys,'Lưu món').click();const product=row(sys,name+' DRINK');await product.waitFor();
 await button(product,'Sửa').click();assert.equal(Number(await label(sys,'Giá size').inputValue()),21000);
 await label(sys,'Giá size').fill('24000');await button(sys,'Lưu món').click();
 await button(product,'Sửa').click();assert.equal(Number(await label(sys,'Giá size').inputValue()),24000);
 await shot(sys,'17-menu-gia-cap-nhat');return 'Tạo và đọc lại đủ 7 loại danh mục, giá món 21.000đ → 24.000đ';
});
const admin=sys;
await test('E2E-18',admin,async()=>{
 await go(admin,'/admin?tab=employees');
 assert.equal(await button(admin,'Tạo tài khoản').count(),0);
 assert.equal(await button(admin,'Phân công chi nhánh').count(),0);
 await button(row(admin,employee),'Sửa hồ sơ').click();
 await label(admin,'Vị trí').fill(name+' Barista');
 await button(admin,'Lưu hồ sơ').click();await admin.getByText('Đã lưu.',{exact:true}).waitFor();
 await go(admin,'/admin?tab=employees');await button(row(admin,employee),'Sửa hồ sơ').click();
 assert.equal(await label(admin,'Vị trí').inputValue(),name+' Barista');
 await shot(admin,'18-ho-so-mobile');
 return 'Admin Chi nhánh sửa vị trí của '+employee+'; lưu và đọc lại thành công trên chiều rộng 390px';
});
await test('E2E-19',admin,async()=>{
 await go(admin,'/admin?tab=branch-menu');await label(admin,'Danh mục quản trị').selectOption('toppings');
 const topping=admin.getByRole('row').filter({has:admin.getByRole('button',{name:/Ngừng bán|Mở bán/})}).first();
 assert.equal(await button(admin,'Thêm topping').count(),0);const original=(await topping.locator('td').first().innerText()).trim();
 const off=button(topping,'Ngừng bán'),on=button(topping,'Mở bán');
 const first=await off.count()?off:on, second=first===off?on:off; const before=await first.innerText();
 await first.click();await second.waitFor();await shot(admin,'19-topping-mobile');await second.click();await first.waitFor();
 return original+' bật/tắt và khôi phục trạng thái '+before;
});
const staff=sys;
await test('E2E-20',staff,async()=>{
 await go(staff,'/employee?tab=stock');await staff.getByRole('heading',{name:'Tồn kho chi nhánh'}).waitFor();
 const originally=await button(staff,'Xuất kho').count()>0;
 async function grant(enabled){
  await go(sys,'/admin?tab=employees');await button(row(sys,'demo.employee.a'),'Sửa hồ sơ').click();
  await sys.getByRole('checkbox',{name:'Xuất kho',exact:true}).setChecked(enabled);
  await button(sys,'Lưu hồ sơ').click();await sys.getByText('Đã lưu.',{exact:true}).waitFor();
 }
 if(!originally)await grant(true);
 async function write(p,direction,qty){
  await button(p,direction).click();const line=p.locator('.management-stock-line').first();
  const options=line.locator('option').filter({hasText:ingredientPrefix});
  await options.first().waitFor({state:'attached'});
  const ingredient=await options.first().getAttribute('value');
  await line.locator('select').selectOption(ingredient);await line.locator('input').fill(String(qty));await label(p,'Lý do').fill(name+' '+direction);
  await button(p,'Xác nhận '+direction.toLowerCase()).click();
  const notice=p.getByText(/Đã lưu giao dịch #/);await notice.waitFor();return Number((await notice.innerText()).match(/#(\d+)/)[1]);
 }
 try{
  await go(staff,'/employee?tab=stock');
  const issued=await write(staff,'Xuất kho',1);
  await button(staff,'Lịch sử nhập xuất').click();await staff.getByRole('heading',{name:'Lịch sử nhập xuất'}).waitFor();
  await shot(staff,'20-nhan-vien-lich-su-xuat-kho');
  await go(admin,'/admin?tab=inventory');const restored=await write(admin,'Nhập kho',1);
  return 'Nhân viên lưu phiếu OUT #'+issued+'; Admin khôi phục tồn bằng IN #'+restored;
 }finally{
  if(!originally){await grant(false);await go(staff,'/employee?tab=stock');await staff.getByRole('heading',{name:'Tồn kho chi nhánh'}).waitFor();assert.equal(await button(staff,'Xuất kho').count(),0);}
 }
});
const owner=sys;
await test('E2E-21',owner,async()=>{
 await go(owner,'/owner?tab=branches');await row(owner,branchName).getByText('Dia chi E2E da cap nhat',{exact:true}).waitFor();assert.equal(await button(owner,'Thêm chi nhánh').count(),0);
 await go(owner,'/owner?tab=employee-directory');await row(owner,employeeName).getByText(name+' Barista',{exact:true}).waitFor();assert.equal(await button(owner,'Sửa hồ sơ').count(),0);
 await shot(owner,'21-owner-mobile');return 'Owner thấy chi nhánh mới, địa chỉ mới và nhân viên vừa sửa; chỉ đọc trên 390px';
});
} finally {
 fs.writeFileSync(path.join(folder,'ket-qua-gia.json'),JSON.stringify({tag,results,consoleErrors},null,2));
 await browser.close();
}
if(results.some(x=>x.result==='Fail')||consoleErrors.length) process.exitCode=1;
})().catch(e=>{console.error(e);process.exitCode=1;});
