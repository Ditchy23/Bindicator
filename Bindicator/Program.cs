using Microsoft.EntityFrameworkCore;
using Bindicator.Data;
using Bindicator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services for SignalR
builder.Services.AddSignalR();

// Register background services
builder.Services.AddHostedService<MqttSubscriberService>();
builder.Services.AddScoped<BinDataService>();
builder.Services.AddScoped<BinTrendService>();
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

app.MapHub<Bindicator.Hubs.BinStatusHub>("/binStatusHub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();