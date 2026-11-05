using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// DDL fixture dùng tên GUID tự sinh và ID INT từ DB, không nhận nội dung từ client.
#pragma warning disable EF1002

// Fixture riêng, chỉ test local; tự dọn toàn bộ dữ liệu thuộc fixture sau khi chạy.
string root=Path.GetFullPath(args.Single());
const string connection="Server=localhost\\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True";
await using var db=new BranchFlowDbContext(new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options);
var suffix=Guid.NewGuid().ToString("N")[..10]; var now=DateTime.UtcNow; var created=new List<object>();
T Add<T>(T entity) where T:class { db.Add(entity); created.Add(entity); return entity; }
Process? host=null; var logs=new StringBuilder(); int checks=0; int branchId=0; bool fixtureSaved=false; string trigger="OrderSmoke_"+suffix;
void Require(bool condition,string label){ if(!condition)throw new Exception(label); Console.WriteLine($"PASS {Interlocked.Increment(ref checks)}: {label}"); }
try
{
 var role=await db.Roles.SingleAsync(r=>r.Code=="EMPLOYEE"); var ownerRole=await db.Roles.SingleAsync(r=>r.Code=="OWNER");
 var user=Add(new AppUser{RoleId=role.Id,Username="order_"+suffix,FullName="Order smoke",PasswordHash="Not a login fixture",IsActive=true});
 var other=Add(new AppUser{RoleId=role.Id,Username="other_"+suffix,FullName="Other",PasswordHash="Not a login fixture",IsActive=true});
 var owner=Add(new AppUser{RoleId=ownerRole.Id,Username="owner_"+suffix,FullName="Owner",PasswordHash="Not a login fixture",IsActive=true});
 var branch=Add(new Branch{Code="OS"+suffix,Name="Order smoke",Address="Local fixture",IsActive=true});
 var category=Add(new Category{Name="Order "+suffix,IsActive=true});
 var product=Add(new Product{Category=category,Name="Order drink "+suffix,IsActive=true});
 var size=Add(new Size{Name="O"+suffix,IsActive=true});
 var ps=Add(new ProductSize{Product=product,Size=size,Price=30000m,IsActive=true});
 Add(new BranchProduct{Branch=branch,Product=product,IsAvailable=true});
 var group=Add(new ToppingGroup{Name="Order "+suffix,IsActive=true});
 var t1=Add(new Topping{ToppingGroup=group,Name="Pearl "+suffix,Price=5000m,IsActive=true});
 var t2=Add(new Topping{ToppingGroup=group,Name="Jelly "+suffix,Price=4000m,IsActive=true});
 Add(new BranchTopping{Branch=branch,Topping=t1,IsAvailable=true}); Add(new BranchTopping{Branch=branch,Topping=t2,IsAvailable=true});
 var ng=Add(new NoteGroup{Name="Sugar "+suffix,IsActive=true});
 var n1=Add(new NoteOption{NoteGroup=ng,Name="Less "+suffix,IsActive=true});
 var n2=Add(new NoteOption{NoteGroup=ng,Name="None "+suffix,IsActive=true});
 var profile=Add(new EmployeeProfile{User=user,EmployeeCode="O"+suffix,HireDate=DateOnly.FromDateTime(now)});
 Add(new EmployeeBranchAssignment{EmployeeProfile=profile,Branch=branch,EffectiveFrom=now.AddDays(-1),AssignedByUser=owner});
 var session=Add(new AuthSession{Id=Guid.NewGuid(),User=user,CreatedAt=now,ExpiresAt=now.AddDays(2)});
 var otherSession=Add(new AuthSession{Id=Guid.NewGuid(),User=other,CreatedAt=now,ExpiresAt=now.AddHours(1)});
 var ownerSession=Add(new AuthSession{Id=Guid.NewGuid(),User=owner,CreatedAt=now,ExpiresAt=now.AddHours(1)});
 await db.SaveChangesAsync(); branchId=branch.Id;fixtureSaved=true;
 string key=Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
 using var http=new HttpClient{BaseAddress=new Uri("http://localhost:19119"),Timeout=TimeSpan.FromSeconds(20)};
 async Task StartHost(string? qaUtc = null)
 {
  if(host is not null){if(!host.HasExited){host.Kill(true);await host.WaitForExitAsync();}host.Dispose();}
  var start=new ProcessStartInfo("dotnet"){WorkingDirectory=root,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
  foreach(var a in new[]{Path.Combine(root,"src/BranchFlow.Api/bin/Release/net10.0/BranchFlow.Api.dll"),"--urls","http://localhost:19119","--contentRoot",Path.Combine(root,"src/BranchFlow.Api")}) start.ArgumentList.Add(a);
  start.Environment["ASPNETCORE_ENVIRONMENT"]=qaUtc is null?"Production":"Testing";
  if(qaUtc is not null)start.Environment["Qa__UtcNow"]=qaUtc;
  start.Environment["ConnectionStrings__BranchFlowDb"]=connection;
  start.Environment["Jwt__Key"]=key;start.Environment["Jwt__Issuer"]="OrderSmoke";start.Environment["Jwt__Audience"]="OrderSmoke";
  host=Process.Start(start)!;host.OutputDataReceived+=(_,e)=>{lock(logs)logs.AppendLine(e.Data);};host.ErrorDataReceived+=(_,e)=>{lock(logs)logs.AppendLine(e.Data);};host.BeginOutputReadLine();host.BeginErrorReadLine();
  bool ready=false;for(int i=0;i<80;i++){try{using var probe=await http.GetAsync("/api/orders/mine");ready=true;break;}catch(HttpRequestException){await Task.Delay(150);}}
  if(!ready)throw new Exception("Host not ready");
 }
 await StartHost();
 string Token(AppUser u,AuthSession s,string r)=>new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("OrderSmoke","OrderSmoke",[new Claim("sub",u.Id.ToString()),new Claim("sid",s.Id.ToString()),new Claim("role",r)],now.AddMinutes(-1),now.AddMinutes(30),new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),SecurityAlgorithms.HmacSha256)));
 string token=Token(user,session,"EMPLOYEE");
 object Item(int cups=2,object[]? toppings=null,int[]? notes=null,int? productSize=null)=>new {productSizeId=productSize??ps.Id,quantity=cups,toppings=toppings??[new{toppingId=t1.Id,quantity=3},new{toppingId=t2.Id,quantity=1}],noteOptionIds=notes??[n1.Id]};
 object Body(object? item=null,int? b=null)=>new{branchId=b??branch.Id,items=new[]{item??Item()}};
 async Task<JsonElement> Send(string method,string path,int status,object? body=null,string? bearer=null)
 {
  using var req=new HttpRequestMessage(new HttpMethod(method),path); if(bearer!="none") req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",bearer??token);
  if(body!=null)req.Content=JsonContent.Create(body);
  using var res=await http.SendAsync(req); string text=await res.Content.ReadAsStringAsync();
  Require((int)res.StatusCode==status,$"{method} {path} expected={status} actual={(int)res.StatusCode}");
  using var json=JsonDocument.Parse(text);
  if(status>=400)Require(res.Content.Headers.ContentType?.MediaType=="application/problem+json" && json.RootElement.TryGetProperty("traceId",out _),"Safe ProblemDetails + traceId");
  return json.RootElement.Clone();
 }
 async Task<int> Counter()=>await db.OrderDailyCounters.AsNoTracking().Where(c=>c.BranchId==branch.Id).Select(c=>c.LastNumber).SingleOrDefaultAsync();
 await Send("POST","/api/orders",401,Body(),"none");
 await Send("POST","/api/orders",403,Body(),Token(owner,ownerSession,"OWNER"));
 await Send("POST","/api/orders",403,Body(),Token(other,otherSession,"EMPLOYEE"));
 await Send("POST","/api/orders",400,Body(Item(0)));
 await Send("POST","/api/orders",400,new{branchId=branch.Id,items=Array.Empty<object>()});
 await Send("POST","/api/orders",400,Body(Item(toppings:[new{toppingId=t1.Id,quantity=4}])));
 await Send("POST","/api/orders",400,Body(Item(toppings:[new{toppingId=t1.Id,quantity=1},new{toppingId=t1.Id,quantity=2}])));
 await Send("POST","/api/orders",400,Body(Item(notes:[n1.Id,n2.Id])));
 await Send("POST","/api/orders",400,Body(Item(productSize:-1)));
 foreach(var field in new[]{"price","totalAmount","createdByUserId","status"})
 {
  var payload=new Dictionary<string,object>{{"branchId",branch.Id},{"items",new[]{Item()}},{field,1}};
  await Send("POST","/api/orders",400,payload);
 }
 foreach(var field in new[]{"unitPrice","subtotalAmount"})
  await Send("POST","/api/orders",400,Body(new Dictionary<string,object>{{"productSizeId",ps.Id},{"quantity",1},{"toppings",Array.Empty<object>()},{"noteOptionIds",Array.Empty<int>()},{field,1}}));
 await Send("POST","/api/orders",400,Body(Item(toppings:[new{toppingId=t1.Id,quantity=1,price=1}])));
 await Send("POST","/api/orders",400,new{branchId=branch.Id,items=(object?)null});
 await Send("POST","/api/orders",400,new{branchId=branch.Id,items=new object?[]{null}});
 await Send("POST","/api/orders",400,Body(new{productSizeId=ps.Id,quantity=1,toppings=(object?)null,noteOptionIds=Array.Empty<int>()}));
 await Send("POST","/api/orders",400,Body(new{productSizeId=ps.Id,quantity=1,toppings=Array.Empty<object>(),noteOptionIds=(object?)null}));
 await Send("POST","/api/orders",400,Body(Item(-1)));
 await Send("POST","/api/orders",400,Body(new{productSizeId=ps.Id,quantity=1.5,toppings=Array.Empty<object>(),noteOptionIds=Array.Empty<int>()}));
 await Send("POST","/api/orders",400,Body(new{productSizeId=ps.Id,quantity="abc",toppings=Array.Empty<object>(),noteOptionIds=Array.Empty<int>()}));
 await Send("POST","/api/orders",400,Body(new{productId=product.Id,sizeId=size.Id,quantity=1,toppings=Array.Empty<object>(),noteOptionIds=Array.Empty<int>()}));
 Require(await Counter()==0,"Invalid requests leave no counter/order");
 var result=await Send("POST","/api/orders",201,Body()); int id=result.GetProperty("id").GetInt32(); string code=result.GetProperty("code").GetString()!;
 Require(result.GetProperty("totalAmount").GetDecimal()==98000m,"SQL price: (30000 + 3*5000 + 4000)*2 = 98000");
 var saved=await db.SalesOrders.AsNoTracking().Include(o=>o.OrderItems).ThenInclude(i=>i.OrderItemToppings).Include(o=>o.OrderItems).ThenInclude(i=>i.OrderItemNotes).SingleAsync(o=>o.Id==id);
 Require(saved.TotalAmount==98000m && saved.OrderItems.Single().OrderItemToppings.Count==2 && saved.OrderItems.Single().OrderItemNotes.Count==1,"Header/details/topping/note persisted in SQL");
 Require(code.EndsWith("-0001") && saved.BusinessDate==DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)),"Vietnam business date + first daily sequence");
 await Send("GET","/api/orders/"+id,200);
 await Send("GET","/api/orders/"+id,404,bearer:Token(other,otherSession,"EMPLOYEE"));
 var list=await Send("GET","/api/orders/mine?keyword="+code+"&businessDate="+saved.BusinessDate.ToString("yyyy-MM-dd"),200);
 Require(list.GetArrayLength()==1 && list[0].GetProperty("id").GetInt32()==id,"Own history filters match SQL order");
 // Đổi menu sau bán: chi tiết cũ phải giữ snapshot, đơn mới phải lấy giá mới từ DB.
 product.Name="Updated "+suffix;ps.Price=31000;await db.SaveChangesAsync();
 var detail=await Send("GET","/api/orders/"+id,200);
 Require(detail.GetProperty("items")[0].GetProperty("productName").GetString()==saved.OrderItems.Single().ProductNameSnapshot,"Historical product snapshot unchanged");
 var updated=await Send("POST","/api/orders",201,Body());Require(updated.GetProperty("totalAmount").GetDecimal()==100000m,"New order uses changed SQL price");
 var bp=await db.BranchProducts.SingleAsync(x=>x.BranchId==branch.Id && x.ProductId==product.Id);bp.IsAvailable=false;await db.SaveChangesAsync();
 int before=await Counter(); await Send("POST","/api/orders",400,Body());Require(await Counter()==before,"Unavailable product rolls back allocated counter");bp.IsAvailable=true;await db.SaveChangesAsync();
 // Inject lỗi chi tiết sau khi header đã insert. Trigger chỉ áp dụng branch fixture.
 await db.Database.ExecuteSqlRawAsync($"CREATE TRIGGER dbo.[{trigger}] ON dbo.OrderItem AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.SalesOrder o ON o.Id=i.SalesOrderId WHERE o.BranchId={branch.Id}) THROW 51050,'OrderSmoke injected detail failure',1; END");
 int count=await db.SalesOrders.CountAsync(o=>o.BranchId==branch.Id);before=await Counter();
 await Send("POST","/api/orders",500,Body());
 Require(await db.SalesOrders.CountAsync(o=>o.BranchId==branch.Id)==count && await Counter()==before,"SQL detail failure rolls back header/details/counter");
 await db.Database.ExecuteSqlRawAsync($"DROP TRIGGER dbo.[{trigger}]");
 var simultaneous=await Task.WhenAll(Enumerable.Range(0,12).Select(_=>Send("POST","/api/orders",201,Body())));
 Require(simultaneous.Select(x=>x.GetProperty("code").GetString()).Distinct().Count()==12,"Twelve concurrent orders have distinct codes");
 var multiline=await Send("POST","/api/orders",201,new{branchId=branch.Id,items=new[]{Item(),Item(1,toppings:[],notes:[])}});
 Require(multiline.GetProperty("totalAmount").GetDecimal()==131000m,"Multiple rows total from SQL 100000 + 31000 = 131000");
 foreach(int quantity in new[]{1,2,3}) { var r=await Send("POST","/api/orders",201,Body(Item(1,toppings:[new{toppingId=t1.Id,quantity}])));Require(r.GetProperty("totalAmount").GetDecimal()==31000+5000*quantity,"Topping quantity price "+quantity); }
 await Send("POST","/api/orders",400,Body(Item(toppings:[new{toppingId=t1.Id,quantity=0}])));
 await Send("POST","/api/orders",400,Body(Item(notes:[n1.Id,n1.Id])));
 // Counter API đi qua 9999: không được cắt mất chữ số thứ năm.
 await db.OrderDailyCounters.Where(c=>c.BranchId==branch.Id).ExecuteUpdateAsync(s=>s.SetProperty(c=>c.LastNumber,9999));
 var tenThousand=await Send("POST","/api/orders",201,Body());
 Require(tenThousand.GetProperty("code").GetString()!.EndsWith("-10000"),"API code retains sequence 10000");
 t2.Price=4500;t2.Name="Changed topping "+suffix;await db.SaveChangesAsync();
 var changed=await Send("POST","/api/orders",201,Body());Require(changed.GetProperty("totalAmount").GetDecimal()==101000,"Current topping SQL price is applied");
 var oldDetail=await Send("GET","/api/orders/"+id,200);
 Require(oldDetail.GetProperty("totalAmount").GetDecimal()==98000 && oldDetail.GetProperty("items")[0].GetProperty("toppings").EnumerateArray().Any(t=>t.GetProperty("name").GetString()=="Jelly "+suffix && t.GetProperty("unitPrice").GetDecimal()==4000),"Old topping name/price snapshot preserved");
 // QA-S4-041: two real HTTP inserts against a controlled API clock, then SQL assertions.
 var boundaryDate=DateTime.UtcNow.Date;
 if(DateTime.UtcNow>=boundaryDate.AddHours(16).AddMinutes(59).AddSeconds(50))boundaryDate=boundaryDate.AddDays(1);
 var beforeMidnight=new DateTimeOffset(boundaryDate.AddHours(16).AddMinutes(59).AddSeconds(59),TimeSpan.Zero);
 var afterMidnight=beforeMidnight.AddSeconds(1);
 await StartHost(beforeMidnight.ToString("O"));
 var firstBoundary=await Send("POST","/api/orders",201,Body());
 await StartHost(afterMidnight.ToString("O"));
 var secondBoundary=await Send("POST","/api/orders",201,Body());
 var firstId=firstBoundary.GetProperty("id").GetInt32();var secondId=secondBoundary.GetProperty("id").GetInt32();
 var boundaryRows=await db.SalesOrders.AsNoTracking().Where(o=>o.Id==firstId||o.Id==secondId).ToDictionaryAsync(o=>o.Id);
 Require(boundaryRows[firstId].BusinessDate==DateOnly.FromDateTime(boundaryDate) && boundaryRows[secondId].BusinessDate==DateOnly.FromDateTime(boundaryDate.AddDays(1)),"QA-S4-041 Vietnam business date changes at 17:00:00Z");
 Require(boundaryRows[firstId].CreatedAt==beforeMidnight.UtcDateTime && boundaryRows[secondId].CreatedAt==afterMidnight.UtcDateTime,"QA-S4-041 SQL CreatedAt remains exact UTC");
 Require(firstBoundary.GetProperty("businessDate").GetDateTime().Date==boundaryDate.Date && secondBoundary.GetProperty("businessDate").GetDateTime().Date==boundaryDate.AddDays(1).Date,"QA-S4-041 HTTP business dates match SQL");
 await StartHost();
 branch.IsActive=false;await db.SaveChangesAsync();await Send("POST","/api/orders",404,Body());
 Console.WriteLine($"RESULT: {checks} checks PASS. API/SQL transaction, scope, price, snapshots and concurrency verified.");
}
finally
{
 if(host is not null && !host.HasExited){host.Kill(true);await host.WaitForExitAsync();}host?.Dispose();
 await db.Database.ExecuteSqlRawAsync($"IF OBJECT_ID(N'dbo.{trigger}',N'TR') IS NOT NULL DROP TRIGGER dbo.[{trigger}]");
 db.ChangeTracker.Clear();
 if(branchId>0)
 {
  var ids=await db.SalesOrders.Where(o=>o.BranchId==branchId).Select(o=>o.Id).ToListAsync();
  var itemIds=await db.OrderItems.Where(i=>ids.Contains(i.SalesOrderId)).Select(i=>i.Id).ToListAsync();
  await db.OrderItemNotes.Where(n=>itemIds.Contains(n.OrderItemId)).ExecuteDeleteAsync();await db.OrderItemToppings.Where(t=>itemIds.Contains(t.OrderItemId)).ExecuteDeleteAsync();
  await db.OrderItems.Where(i=>ids.Contains(i.SalesOrderId)).ExecuteDeleteAsync();await db.SalesOrders.Where(o=>ids.Contains(o.Id)).ExecuteDeleteAsync();await db.OrderDailyCounters.Where(c=>c.BranchId==branchId).ExecuteDeleteAsync();
 }
 if(fixtureSaved) foreach(var entity in created.AsEnumerable().Reverse()){db.Entry(entity).State=EntityState.Deleted;await db.SaveChangesAsync();db.Entry(entity).State=EntityState.Detached;}
 Console.WriteLine("Fixture cleanup completed; no Dev/cloud changes.");
}
