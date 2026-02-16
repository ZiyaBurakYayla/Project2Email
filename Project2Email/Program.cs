using Microsoft.AspNetCore.Identity;
using Project2Email.Context;
using Project2Email.Entities;
using Project2Email.Models;
using Project2Email.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secret.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddDbContext<EmailContext>();

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<EmailContext>
    ().AddErrorDescriber<CustomIdentityValidator>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<GeminiService>();

builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=UserLogin}/{id?}");

app.Run();
