using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using repository_pattern_experiment.Data;
using repository_pattern_experiment.Data.RepositoryPattern.Cache;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Data.RepositoryPattern.Repository;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<AppUser>(options =>
            options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddMemoryCache();

            builder.Services.AddScoped<IIssueRepository, IssueRepository>();
            builder.Services.Decorate<IIssueRepository, IssueCacheRepository>();
            builder.Services.AddScoped<IBreadcrumbRepository, BreadcrumbRepository>();
            builder.Services.Decorate<IBreadcrumbRepository, BreadcrumbCacheRepository>();
            builder.Services.AddScoped<IVoteStatsRepository, VoteStatsRepository>();
            builder.Services.Decorate<IVoteStatsRepository, VoteStatsCacheRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=RepositoryTest}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
