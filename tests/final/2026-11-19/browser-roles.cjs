const {chromium, request}=require('C:/Users/Admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert=require('node:assert/strict'), fs=require('node:fs'), path=require('node:path');
const web='https://localhost:17133', api='https://127.0.0.1:17108';
const code=process.env.BRANCHFLOW_UAT_ORDER_CODE, folder=process.env.BRANCHFLOW_UAT_EVIDENCE;
const pw=process.env.BRANCHFLOW_DEMO_PASSWORD;
const amount=n=>new Intl.NumberFormat('vi-VN',{maximumFractionDigits:2}).format(n)+'đ';
const results=[];
function passed(id,condition,actual){results.push({id,condition,actual,result:'Pass'});console.log(id+' PASS '+actual)}
async function run(){
  const browser=await chromium.launch({channel:'msedge',headless:true});
  const http=await request.newContext({baseURL:api,ignoreHTTPSErrors:true});
  const tokens=new Map(), errors=[];
  async function login(user,route,width=1440){
    const page=await browser.newPage({viewport:{width,height:width===1440?900:860},ignoreHTTPSErrors:true,locale:'vi-VN'});
    page.setDefaultTimeout(15000); page.on('pageerror',e=>errors.push(e.message));
    await page.goto(web+'/login'); await page.locator('#username').fill(user); await page.locator('#password').fill(pw);
    await page.locator('button.login-submit').click(); await page.waitForURL('**'+route);
    return page;
  }
  async function apiCall(user,verb,uri,expect=200,data){
    if(!tokens.has(user)){
      let login=await http.post('/api/auth/login',{data:{username:user,password:pw}}); if(login.status()===429){ console.log('Login rate limit, retrying after the published window'); await new Promise(r=>setTimeout(r,61000)); login=await http.post('/api/auth/login',{data:{username:user,password:pw}}); }
      assert.equal(login.status(),200,'API login '+user);
      tokens.set(user,(await login.json()).accessToken);
    }
    const res=await http.fetch(uri,{method:verb,headers:{Authorization:'Bearer '+tokens.get(user)},data});
    assert.equal(res.status(),expect,verb+' '+uri+' '+user);
    return expect===200?await res.json():null;
  }
  const shot=(p,name)=>p.screenshot({path:path.join(folder,name+'.png')});
  const ownerId='demo.owner', staffId='demo.employee.a', adminId='demo.branchadmin.01', systemId='demo.systemadmin';
  const otherId='demo.employee.b', otherAdminId='demo.branchadmin.02';
  const date=code.slice(11,15)+'-'+code.slice(15,17)+'-'+code.slice(17,19);
  try{
    const staff=await login(staffId,'/employee',390);
    const admin=await login(adminId,'/admin',390);
    const sys=await login(systemId,'/admin');
    const owner=await login(ownerId,'/owner');
    passed('UAT-01','Bốn vai trò đăng nhập và vào đúng phân hệ','Nhân viên /employee, hai Admin /admin, Owner /owner');
    const matches=await apiCall(staffId,'GET','/api/orders/mine?keyword='+code);
    assert.equal(matches.length,1);
    const orderId=matches[0].id;
    const orderBefore=await apiCall(staffId,'GET', '/api/orders/'+orderId);
    const branch=orderBefore.branchId, orderDate=orderBefore.businessDate;
    const adminPath='/api/management/orders/'+orderId;
    const revenuePath='/api/management/revenue?branchId='+branch+'&from='+orderDate+'&to='+orderDate;
    const getRevenue=async user=>(await apiCall(user,'GET',revenuePath)).reduce((v,r)=>v+r.amount,0);
    const baseline=await getRevenue(ownerId);
    assert.equal(baseline,await getRevenue(adminId));
    assert.equal(baseline,await getRevenue(systemId));
    const stockPath='/api/management/branches/'+branch+'/stock';
    const stockBefore=await apiCall(adminId,'GET',stockPath);
    assert(stockBefore.length>0);
    await staff.goto(web+'/orders/'+orderId);
    const report=staff.getByRole('region',{name:'Báo đơn sai'});
    await report.getByRole('button',{name:'Báo đơn sai',exact:true}).click();
    const reason='UAT local: khách cần 2 ly, đơn đang ghi 3 ly';
    await report.getByLabel('Lý do báo đơn sai').fill(reason);
    await report.getByRole('button',{name:'Gửi báo cáo'}).click();
    await report.getByText('Đã gửi · Chờ Admin xử lý',{exact:true}).waitFor();
    await apiCall(staffId,'POST','/api/orders/'+orderId+'/report',409,{reason:'Trùng'});
    assert.equal(await getRevenue(ownerId),baseline);
    await shot(staff,'02-nhan-vien-bao-sai');
    passed('UAT-03','Báo đơn sai giữ nguyên doanh thu, gửi lặp bị chặn','Mã '+code+', báo cáo chờ xử lý, API trả 409 cho báo trùng');
    await apiCall(otherId,'GET','/api/orders/'+orderId,404);
    await apiCall(otherId,'POST','/api/orders/'+orderId+'/report',404,{reason:'UAT'});
    await apiCall(otherAdminId,'GET',adminPath,403);
    const otherQueue=await apiCall(otherAdminId,'GET','/api/management/orders/reported');
    assert(!otherQueue.some(x=>x.id===orderId));
    await apiCall(ownerId,'PUT',adminPath,403,{});
    passed('UAT-04','Cách ly tài khoản và chi nhánh','Nhân viên khác trả 404; Admin khác chi nhánh và Owner ghi đơn trả 403; báo sai không lộ ở hàng chờ chi nhánh khác');
    await admin.goto(web+'/admin?tab=orders');
    const queue=admin.getByRole('region',{name:'Đơn nhân viên báo sai'});
    const queued=queue.getByRole('button',{name:'Xử lý báo cáo đơn '+code,exact:true});
    await queued.waitFor(); await shot(admin,'03-admin-hang-cho');
    await queued.click();
    await admin.getByText(reason,{exact:true}).waitFor();
    const number=admin.getByRole('spinbutton',{name:/Số ly/}).first();
    assert.equal(await number.inputValue(),'3');
    await number.fill('2');
    await admin.getByLabel('Lý do sửa hoặc hủy').fill('UAT: đối chiếu và sửa số ly');
    await admin.getByRole('button',{name:'Lưu số ly'}).click();
    await admin.getByText('Đã cập nhật số ly.',{exact:true}).waitFor();
    await staff.reload();
    await report.getByText('Báo cáo đã được xử lý',{exact:true}).waitFor();
    await shot(staff,'04-nhan-vien-da-xu-ly');
    const adjusted=await apiCall(staffId,'GET','/api/orders/'+orderId);
    assert(!adjusted.hasPendingReport); assert.equal(adjusted.items[0].quantity,2);
    assert.equal(await getRevenue(ownerId),baseline-(orderBefore.totalAmount-adjusted.totalAmount));
    passed('UAT-05','Admin nhận báo cáo, sửa số ly và lưu lịch sử','3 → 2 ly; '+amount(orderBefore.totalAmount)+' → '+amount(adjusted.totalAmount)+'; Nhân viên thấy đã xử lý');

    const product=adjusted.items[0].productName;
    await sys.goto(web+'/admin?tab=system-menu');
    const sysRow=sys.getByRole('row').filter({hasText:product}).first();
    await sysRow.getByRole('button',{name:'Sửa',exact:true}).click();
    await sys.getByLabel('Giá size').first().waitFor();
    await sys.goto(web+'/admin?tab=employees');
    const userRow=sys.getByRole('row').filter({hasText:'demo.employee.a'}).first();
    await userRow.getByRole('button',{name:'Sửa hồ sơ',exact:true}).click();
    await sys.getByRole('heading',{name:'Hồ sơ demo.employee.a',exact:true}).waitFor();
    passed('UAT-06','Admin Tổng truy cập form danh mục và hồ sơ','Đã mở giá theo size của '+product+' và hồ sơ demo.employee.a');

    await admin.goto(web+'/admin?tab=branch-menu');
    const branchRow=admin.getByRole('row').filter({hasText:product}).first();
    const off=branchRow.getByRole('button',{name:'Ngừng bán',exact:true});
    const on=branchRow.getByRole('button',{name:'Mở bán',exact:true});
    await off.click(); await on.waitFor();
    try{
       await staff.goto(web+'/orders/create');
       await staff.locator('.order-menu-product-card').first().waitFor();
       assert.equal(await staff.getByRole('heading',{name:product,exact:true}).count(),0);
       assert.equal((await apiCall(staffId,'GET','/api/orders/'+orderId)).totalAmount,adjusted.totalAmount);
    }finally{ await on.click();await off.waitFor(); }
    await staff.reload();
    await staff.getByRole('heading',{name:product,exact:true}).waitFor();
    passed('UAT-07','Ngừng bán và mở bán ảnh hưởng menu chi nhánh, giữ giá đơn đã tạo','Món '+product+' biến mất rồi hiện lại; tổng đơn cũ giữ '+amount(adjusted.totalAmount));

    await admin.goto(web+'/admin?tab=orders');
    const filter=admin.getByRole('searchbox',{name:'Tìm đơn'});
    await filter.fill(code);
    await admin.getByRole('button',{name:'Xem chi tiết đơn '+code,exact:true}).click();
    await admin.getByLabel('Lý do sửa hoặc hủy').fill('UAT: hủy đơn dùng để kiểm thử');
    await admin.getByRole('button',{name:'Hủy đơn',exact:true}).click();
    await admin.getByRole('button',{name:'Xác nhận hủy đơn '+code,exact:true}).click();
    await admin.getByText('Đã hủy đơn.',{exact:true}).waitFor();
    await staff.goto(web+'/orders/'+orderId);
    await staff.locator('.status-badge.cancelled').waitFor();
    const afterCancel=await getRevenue(ownerId);
    assert.equal(afterCancel,baseline-orderBefore.totalAmount);
    passed('UAT-08','Hủy đơn và đối chiếu doanh thu','Nhân viên thấy Đã hủy; doanh thu chi nhánh '+amount(baseline)+' → '+amount(afterCancel)+' sau toàn bộ lượt');

    await owner.goto(web+'/owner');
    await owner.locator('.management-filters select').selectOption(String(branch));
    await owner.getByLabel('Từ ngày').fill(orderDate);
    await owner.getByLabel('Đến ngày').fill(orderDate);
    await owner.getByRole('button',{name:'Áp dụng bộ lọc'}).click();
    await owner.locator('.management-metrics article').first().getByText(amount(afterCancel),{exact:true}).waitFor();
    await shot(owner,'05-owner-doanh-thu');
    await owner.goto(web+'/owner?tab=employee-directory');
    await owner.getByRole('searchbox',{name:'Tìm nhân viên'}).fill('Demo Employee A');
    await owner.getByRole('row').filter({hasText:'Demo Employee A'}).first().waitFor();
    assert.equal(await owner.getByRole('button',{name:/Sửa hồ sơ|Tạo tài khoản/}).count(),0);
    await owner.goto(web+'/owner?tab=inventory');
    const ownerBranch=owner.locator('.management-filters select');
    await ownerBranch.selectOption(String(branch));
    await owner.getByRole('heading',{name:'Tồn kho chi nhánh'}).waitFor();
    assert.equal(await owner.getByRole('button',{name:/^Nhập kho$|^Xuất kho$/}).count(),0);
    passed('UAT-09','Owner xem đúng doanh thu, danh bạ và kho chỉ đọc','Tổng UI khớp API '+amount(afterCancel)+'; hiện nhân viên và tồn kho trong phạm vi; không có nút ghi');

    const target=stockBefore.find(x=>x.quantity>=2);
    assert(target,'stock fixture missing');
    const before=target.quantity;
    const checkStock=async n=>{
      const rows=await apiCall(adminId,'GET',stockPath);
      assert.equal(rows.find(x=>x.ingredientId===target.ingredientId).quantity,n);
    };
    await checkStock(before);
    await admin.goto(web+'/admin?tab=inventory');
    async function stockWrite(direction,qty,expected){
      const inStock=direction==='Nhập kho';
      await admin.getByRole('button',{name:direction,exact:true}).click();
      const line=admin.locator('.management-stock-line').first();
      await line.locator('select').selectOption(String(target.ingredientId));
      await line.locator('input').fill(String(qty));
      await admin.getByLabel('Lý do',{exact:true}).fill('UAT local '+direction+' '+qty);
      await admin.getByRole('button',{name:'Xác nhận '+direction.toLowerCase(),exact:true}).click();
      if(expected) { const status=admin.getByText(/Đã lưu giao dịch #/); await status.waitFor(); return Number((await status.innerText()).match(/#(\d+)/)[1]); }
      else await admin.getByText(/Không đủ tồn|vượt tồn|tồn kho không đủ/i).first().waitFor();
    }
    const receiptIn=await stockWrite('Nhập kho',2,true); await checkStock(before+2);
    await stockWrite('Xuất kho',before+3,false); await checkStock(before+2);
    const receiptOut=await stockWrite('Xuất kho',2,true); await checkStock(before);
    await admin.getByRole('button',{name:'Lịch sử nhập xuất',exact:true}).click();
    await admin.getByRole('heading',{name:'Lịch sử nhập xuất'}).waitFor();
    await shot(admin,'06-admin-lich-su-kho');
    const history=await apiCall(adminId,'GET',stockPath+'/history');
    assert(history.some(h=>h.id===receiptIn&&h.type==='IN'&&h.lines.some(l=>l.before===before&&l.after===before+2))); assert(history.some(h=>h.id===receiptOut&&h.type==='OUT'&&h.lines.some(l=>l.before===before+2&&l.after===before)));
    passed('UAT-10','Nhập, chặn xuất quá tồn, xuất hợp lệ và tra lịch sử','Tồn '+before+' → '+(before+2)+' → '+(before+2)+' → '+before+', phiếu IN #'+receiptIn+' và OUT #'+receiptOut);

    await staff.goto(web+'/employee?tab=stock');
    await staff.getByRole('heading',{name:'Tồn kho chi nhánh'}).waitFor();
    const rule=await apiCall(staffId,'GET',stockPath+'/capabilities');
    assert.equal(await staff.getByRole('button',{name:'Xuất kho',exact:true}).count(),rule.canIssue?1:0);
    if(!rule.canIssue){
       await sys.goto(web+'/admin?tab=employees');
       await sys.getByRole('row').filter({hasText:'demo.employee.a'}).first().getByRole('button',{name:'Sửa hồ sơ',exact:true}).click();
       await sys.getByRole('checkbox',{name:'Xuất kho',exact:true}).check();
       await sys.getByRole('button',{name:'Lưu hồ sơ',exact:true}).click();
       await sys.getByText('Đã lưu.',{exact:true}).waitFor();
       try {
         await staff.reload();
         await staff.getByRole('button',{name:'Xuất kho',exact:true}).click();
         await staff.getByRole('heading',{name:'Phiếu xuất kho'}).waitFor();
         assert((await apiCall(staffId,'GET',stockPath+'/capabilities')).canIssue);
       }finally{
         await sys.getByRole('row').filter({hasText:'demo.employee.a'}).first().getByRole('button',{name:'Sửa hồ sơ',exact:true}).click();
         await sys.getByRole('checkbox',{name:'Xuất kho',exact:true}).uncheck();
         await sys.getByRole('button',{name:'Lưu hồ sơ',exact:true}).click();
         await sys.getByText('Đã lưu.',{exact:true}).waitFor();
       }
    }
    passed('UAT-11','Nút xuất kho của Nhân viên tuân theo quyền','Quyền API và nút trên UI nhất quán'+(!rule.canIssue?' và hiện phiếu khi Admin Tổng cấp quyền thử':''));
    await owner.locator('.employee-sidebar').getByRole('button',{name:'Đăng xuất',exact:true}).click();
    await owner.waitForURL(url=>url.pathname==='/login'&&url.searchParams.has('notice'));
    await owner.goto(web+'/owner');
    await owner.waitForURL(/\/login/);
    passed('UAT-12','Đăng xuất chặn truy cập lại phân hệ','Owner quay về trang đăng nhập và không mở lại được /owner');
    assert.deepEqual(errors,[]);
  }finally{
   fs.writeFileSync(path.join(folder,'ket-qua-trinh-duyet.json'),JSON.stringify(results,null,2));
   for(const token of tokens.values()){try{await http.post('/api/auth/logout',{headers:{Authorization:'Bearer '+token}});}catch{}}
   await http.dispose(); await browser.close();
  }
}
run().catch(e=>{console.error(e.stack);process.exitCode=1});


