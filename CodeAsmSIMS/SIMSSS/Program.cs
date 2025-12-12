using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SIMSS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Configure to connect db
            builder.Services.AddDbContext<SimsDbContext.SimsDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });

            builder.Services.AddScoped<Interfaces.IUserRepository, Repository.UserRepository>();
            builder.Services.AddScoped<Interfaces.IPasswordHasher, Services.DefaultPasswordHasher>();
            builder.Services.AddScoped<Services.UserService, Services.UserService>();
            builder.Services.AddScoped<Services.IDashboardService, Services.DashboardService>();
            // Add services to the container.
            builder.Services.AddControllersWithViews();


            //Configure the Indentity Authencation
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
            {
                option.LoginPath = "/Login/Index";
                option.AccessDeniedPath = "/Authentication/AccessDenied";
                option.LogoutPath = "/Login/Logout";
            });
            // Configure the Roles for users
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("Student", policy => policy.RequireRole("Student"));
                options.AddPolicy("Falculty", policy => policy.RequireRole("Falculty"));
            });

            var app = builder.Build();

            //Configure the Indentity Authencation
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

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
