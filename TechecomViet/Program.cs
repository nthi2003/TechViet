using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true; // Cookie chỉ dùng cho HTTP
    options.Cookie.IsEssential = true; // Cookie là cần thiết
});

// Cấu hình DbContext và kết nối cơ sở dữ liệu
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechViet"));
});

// Cấu hình Identity
builder.Services.AddIdentity<UserModel, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

// Cấu hình các dịch vụ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình môi trường và các Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Bật HSTS khi không phải trong môi trường phát triển
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Cấu hình Middleware cho routing
app.UseRouting();

// Cấu hình Authentication và Authorization (chỉ cần một lần)
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình các route
app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Doashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
