using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký controller và bộ sinh tài liệu Swagger cho API.
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BranchFlow API",
        Version = "v1",
        Description = "API dự án nhóm BranchFlow. Hiện mới thiết lập cấu trúc nền."
    });

    string xmlFileName = $"{builder.Environment.ApplicationName}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

var app = builder.Build();

// Chỉ mở tài liệu Swagger trong môi trường phát triển.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BranchFlow API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
