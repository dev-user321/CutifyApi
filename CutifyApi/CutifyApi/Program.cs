using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Data;
using ServiceLayer.Services;
using ServiceLayer.Services.Interfaces;
using System.Text;

namespace CutifyApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Swagger/OpenAPI üçün
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // SQL Server konfiqurasiyası
            builder.Services.AddSqlServer<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
            builder.Services.AddHttpContextAccessor();


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options =>
     {
         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             ValidIssuer = "https://localhost:7003/",
             ValidAudience = "https://localhost:7003/",
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("I_dont_know_what_happen_in_here777")),
             ClockSkew = TimeSpan.Zero

         };
     });


            builder.Services.AddAuthorization();

            // Servislər
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddHostedService<TokenCleanupService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Session üçün cache əlavə edin
            builder.Services.AddDistributedMemoryCache();

            // Session konfiqurasiyası
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

          

            var app = builder.Build();

            // HTTP pipeline konfiqurasiyası
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseSession();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
