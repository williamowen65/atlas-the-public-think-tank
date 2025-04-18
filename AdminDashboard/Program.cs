using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Identity;
using Shared.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container - using shared components
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add shared identity database
builder.Services.AddSharedIdentityDatabase(builder.Configuration);

// Add shared identity UI
builder.Services.AddSharedIdentityUI(builder.Configuration, options => {
    options.SignIn.RequireConfirmedAccount = true;
});

// Add controllers and views
builder.Services.AddControllersWithViews();

// Add shared UI components
builder.Services.AddSharedUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Use shared identity UI (authentication/authorization)
app.UseSharedIdentityUI();

// Use shared UI components
app.UseSharedUI();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
