const { chromium }=require('C:/Users/Admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert=require('node:assert/strict');
const os=require('node:os'), path=require('node:path');
const base='https://localhost:17133';
const fs=require('node:fs'); const out=process.env.BRANCHFLOW_UAT_EVIDENCE; const shot=(page,name)=>page.screenshot({path:path.join(out,'staff-'+name+'.png')});
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage({viewport:{width:1440,height:900},ignoreHTTPSErrors:true,locale:'vi-VN'});
  const errors=[]; page.on('pageerror',e=>errors.push(e.message));
  page.setDefaultTimeout(16000);
  await page.goto(base+'/login');
  await page.locator('#username').fill('demo.employee.a');
  await page.locator('#password').fill(process.env.BRANCHFLOW_DEMO_PASSWORD);
  await page.locator('button.login-submit').click();
  await page.waitForURL('**/employee');
  const routes=[
    ['/employee',page.getByRole('heading',{name:'Ca làm của bạn'})],
    ['/orders/create',page.locator('.order-menu-product-card').first()],
    ['/orders/mine',page.locator('.staff-history-filters')],
    ['/employee?tab=stock',page.getByRole('heading',{name:'Tồn kho chi nhánh'})],
    ['/account',page.getByRole('heading',{name:'Thông tin cá nhân'})]
  ];
  async function noOverflow(name){
    const diff=await page.evaluate(()=>document.documentElement.scrollWidth-innerWidth);
    assert(diff<=1,name+' overflow '+diff);
  }
  for(const width of [1440,390,360]){
   await page.setViewportSize({width,height:width===1440?900:820});
   for(const [url,ready] of routes){
     await page.goto(base+url);
     await ready.waitFor();
     assert.equal(await page.locator('.staff-shell').count(),1);
     await noOverflow(url+' '+width);
     if(url==='/employee'){
       assert.equal(await page.locator('main').getByRole('link',{name:'Gọi món',exact:true}).count(),1);
       assert.equal(await page.locator('main').getByRole('link',{name:'Xuất kho',exact:true}).count(),0);
       if(width===1440) { await page.getByRole('button',{name:'Tải lại',exact:true}).waitFor(); await shot(page,'trang-chu'); }
     }
     if(url==='/orders/create' && width===390) {
       const cart=page.getByRole('button',{name:/Mở giỏ hàng/});
       assert.equal(await cart.evaluate(el=>getComputedStyle(el).position),'fixed');
       const initial=await cart.boundingBox();
       await page.evaluate(()=>window.scrollTo(0,200));
       const scrolled=await cart.boundingBox();
       assert.equal(Math.round(initial.y),Math.round(scrolled.y));
     }
     if(url.includes('tab=stock')){
       await page.getByRole('button',{name:'Lịch sử nhập xuất',exact:true}).click();
       await page.getByRole('heading',{name:'Lịch sử nhập xuất'}).waitFor();
     }
   }
   console.log('PASS 5 Employee screens at '+width+'px');
  }
  await page.setViewportSize({width:390,height:820});
  await page.goto(base+'/orders/create');
  const menuSearch=page.getByRole('searchbox',{name:'Tìm món'});
  await menuSearch.fill('item-zz-not-existing');
  await page.getByText('Không tìm thấy món phù hợp').waitFor();
  await page.getByRole('button',{name:'Bỏ lọc'}).click();
  await page.locator('.order-menu-product-card').first().getByRole('button',{name:/Chọn món/}).click();
  const product=page.locator('#product-options[open]');
  await product.waitFor();
  await product.locator('input[name=size]').first().check();
  await product.getByRole('button',{name:'Tăng số ly'}).click();
  if(await product.locator('summary').filter({hasText:'Topping'}).count()){
    await product.locator('summary').filter({hasText:'Topping'}).click();
    const add=product.locator('.topping-row').first().getByRole('button',{name:/Thêm/});
    const row=product.locator('.topping-row').first(); const max=Number((await row.innerText()).match(/tối đa (\d+)/)[1]); for(let i=1;i<=max;i++){await add.click();await row.getByText(String(i),{exact:true}).waitFor();}
    assert.equal(await add.isDisabled(),true);
  }
  await product.getByRole('button',{name:/Thêm vào đơn/}).click();
  await product.waitFor({state:'detached'});
  await page.evaluate(()=>window.scrollTo(0,0));
  await shot(page,'goi-mon-mobile');
  await page.getByRole('button',{name:'Mở giỏ hàng, 2 ly'}).click();
  const cart=page.locator('#order-cart[open]');
  await cart.waitFor();
  await cart.getByRole('button',{name:'Chỉnh sửa tùy chọn'}).click();
  await product.waitFor();
  assert.equal(await product.locator('#cup-quantity').inputValue(),'2');
  await product.getByRole('button',{name:'Tăng số ly'}).click();
  await product.getByRole('button',{name:/Lưu thay đổi/}).click();
  await cart.getByText('Tổng cộng · 3 ly').waitFor();
  await noOverflow('mobile cart');
  await shot(page,'gio-hang-mobile');
  await cart.getByRole('button',{name:'Xác nhận tạo đơn'}).click();
  await page.waitForURL(/\/orders\/\d+/,{timeout:22000});
  const details=page.url();
  const summary=page.getByRole('region',{name:'Tóm tắt hóa đơn'});
  await summary.waitFor();
  const code=await summary.getByRole('heading',{level:2}).first().innerText();
  await page.getByText('Đã tạo đơn '+code,{exact:true}).waitFor();
  await page.locator('.staff-page-actions').getByRole('link',{name:/Đơn của tôi/}).click();
  await page.getByRole('searchbox',{name:'Mã đơn hàng'}).fill(code);
  await page.getByRole('button',{name:'Tìm đơn',exact:true}).click();
  await page.getByRole('link',{name:new RegExp(code)}).click();
  await page.waitForURL(details);
  await summary.waitFor();
  fs.writeFileSync(path.join(out,'order-code.txt'),code); assert.deepEqual(errors,[]);
  console.log('PASS mobile menu search, topping limit, cart edit, order confirmation, lookup and receipt '+code);
 }finally{await browser.close()}
})().catch(e=>{console.error(e.stack);process.exitCode=1});



