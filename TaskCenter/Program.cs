using AutoMapper;
using CommonLib.Core.Utility;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using ModelCore.DataEntity;
// Add Swagger/OpenAPI usings
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json;
using TaskCenter.Controllers.Filters;
using TaskCenter.Core.Interfaces;
using TaskCenter.Core.Services;
using TaskCenter.Properties;
using Microsoft.IdentityModel.Tokens;

namespace TaskCenter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMemoryCache();

            // 註冊 Cookie & JWT Authentication
            var jwtKey = AppSettings.Default.Jwt.SecurityKey ?? "change_this_secret_key_please";
            var jwtIssuer = AppSettings.Default.Jwt.Issuer ?? "TaskCenter";
            var jwtAudience = AppSettings.Default.Jwt.Audience ?? "TaskCenterAudience";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            builder.Services.AddAuthentication(options =>
            {
                // default to JWT for API scenarios; cookie can be selected for browser interactions
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            // 註冊 CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMyClient", policy =>
                {
                    policy.WithOrigins(AppSettings.Default.AllowCORS) // 允許的來源
                          .AllowAnyHeader()                  // 允許所有 headers
                          .AllowAnyMethod()                 // 允許 GET, POST, PUT, DELETE
                          .AllowCredentials();              // 允許 Credentials (Cookies)
                });

                // 如果要允許全部來源 (僅限測試用，不建議正式環境)
                //options.AddPolicy("AllowAll", policy =>
                //{
                //    policy.AllowAnyOrigin()
                //          .AllowAnyHeader()
                //          .AllowAnyMethod();
                //});
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddMvc(config =>
            {
                config.Filters.Add(new ExceptionFilter());
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = true;
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddRazorRuntimeCompilation();

            // Add services to the container.
            builder.Services
                .AddControllersWithViews(configure =>
                    {
                        configure.Filters.Add(new ExceptionFilter());
                    }
                )
                .AddRazorRuntimeCompilation();

            // 加上這行↓↓
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Add Swagger/OpenAPI support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TaskCenter API",
                    Version = "v1",
                    Description = "Swagger for TaskCenter project"
                });

                // Add JWT Bearer support to Swagger (Authorize button)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                // Swashbuckle 10.x removed setting Reference on OpenApiSecurityScheme.
                // Build the requirement using a scheme reference object as the key.
                var scheme = new OpenApiSecuritySchemeReference("Bearer")
                {
                    //Type = SecuritySchemeType.Http,
                    //Scheme = "bearer",
                    //BearerFormat = "JWT",
                    //In = ParameterLocation.Header,
                    //Name = "Authorization"
                };

                OpenApiSecurityRequirement keyValues = new OpenApiSecurityRequirement();
                keyValues.Add(scheme, new List<string>());

                // When adding requirement, reference the registered scheme by creating a matching scheme object.
                // Add security requirement referencing the defined scheme by reference.
                c.AddSecurityRequirement(doc => keyValues);
            });

            builder.Logging
                .AddConsole()
                .AddProvider(new FileLoggerProvider())
                .AddDebug();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                //options.UseSqlServer(ModelCore.Properties.AppSettings.Default.ConnectionString);
            });

            // Repository Pattern & Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            builder.Services.AddScoped<IElementaryService, ElementaryService>();

            // AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = AppSettings.Default.License.AutoMapperLicenseKey;
                cfg.AddMaps(typeof(Program).Assembly);
                cfg.AddMaps(typeof(ApplicationDbContext).Assembly);
                //cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            });

            // Register AutoMapper scanning all assemblies
            //builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable Swagger middleware in development
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskCenter API V1");
                    c.RoutePrefix = "swagger"; // serve at /swagger
                });
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("AllowMyClient");

            //app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();//Controller、Action才能加上 [Authorize] 屬性
            app.UseSession();

            app.Use(next =>
                context =>
                {
                    context.Request.EnableBuffering();
                    return next(context);
                });


            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}