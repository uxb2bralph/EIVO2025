using CommonLib.Core.Utility;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.SpaServices;

using ModelCore.DataEntity;
// Add Swagger/OpenAPI usings
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json;
using TaskCenter.Controllers.Filters;
using TaskCenter.Core;
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
                options.Events = new JwtBearerEvents
                {
                    // refresh token 與 access token 使用相同簽章，這裡擋掉 refresh token 被當成
                    // access token 來通過 [Authorize]。
                    OnTokenValidated = context =>
                    {
                        var tokenType = context.Principal?.FindFirst("token_type")?.Value;
                        if (string.Equals(tokenType, "refresh", StringComparison.Ordinal))
                        {
                            context.Fail("Refresh token cannot be used for authentication.");
                        }
                        return Task.CompletedTask;
                    }
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
                config.Filters.Add<ExceptionFilter>();

                // 對外收單 API（InvoiceService）以 Newtonsoft.Json 解析請求內容，維持與舊版
                // FromJsonBody<T>() 相同的寬鬆行為；僅對 InvoiceRequestViewModel 生效，
                // SPA 專用的 DTO 仍由 System.Text.Json 處理。
                config.InputFormatters.Insert(0, new LegacyJsonInputFormatter());

                // 模型繫結產生的錯誤訊息在地化（型別轉換失敗、缺值、必須為數值等）。
                var messages = config.ModelBindingMessageProvider;
                messages.SetValueIsInvalidAccessor(value => $"值 {value} 格式不正確!!");
                messages.SetValueMustNotBeNullAccessor(value => $"值 {value} 不可為空白!!");
                messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"{field} 的值 {value} 格式不正確!!");
                messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"值 {value} 格式不正確!!");
                messages.SetUnknownValueIsInvalidAccessor(field => $"{field} 的值格式不正確!!");
                messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "值格式不正確!!");
                messages.SetMissingBindRequiredValueAccessor(field => $"未提供 {field} 的資料!!");
                messages.SetMissingRequestBodyRequiredValueAccessor(() => "請求內容不可為空白!!");
                messages.SetMissingKeyOrValueAccessor(() => "資料不可為空白!!");
                messages.SetValueMustBeANumberAccessor(field => $"{field} 必須為數值!!");
                messages.SetNonPropertyValueMustBeANumberAccessor(() => "必須為數值!!");
            }).ConfigureApiBehaviorOptions(options =>
            {
                // [ApiController] 自動模型驗證失敗時，改回傳在地化且與 BaseResponseDto 一致的內容，
                // 取代預設英文的 ValidationProblemDetails。
                // 註：自動驗證只在 SPA API 生效；對外收單 API（InvoiceService）須維持 HTTP 200 + Root
                // 的回應格式，改以 [SuppressModelStateInvalidFilter] 逐一停用（見該控制器）。
                options.InvalidModelStateResponseFactory = ApiValidationResponse.Create;
            })
            .AddRazorRuntimeCompilation();

            // Add services to the container.
            builder.Services
                .AddControllersWithViews(configure =>
                    {
                        configure.Filters.Add<ExceptionFilter>();
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
            builder.Services.AddScoped<IOrganizationQueryService, OrganizationQueryService>();
            builder.Services.AddScoped<IUserAccountService, UserAccountService>();
            builder.Services.AddScoped<ITrackCodeService, TrackCodeService>();
            builder.Services.AddScoped<IWinningNumberService, WinningNumberService>();
            builder.Services.AddScoped<IPeriodicalExchangeRateService, PeriodicalExchangeRateService>();
            builder.Services.AddScoped<IBusinessRelationshipService, BusinessRelationshipService>();
            builder.Services.AddScoped<IInvoiceNumberApplyService, InvoiceNumberApplyService>();
            builder.Services.AddScoped<IInvoiceNoIntervalService, InvoiceNoIntervalService>();
            builder.Services.AddScoped<IInvoiceProcessQueryService, InvoiceProcessQueryService>();
            builder.Services.AddScoped<IInvoiceProcessActionService, InvoiceProcessActionService>();
            builder.Services.AddScoped<IInvoiceSummaryService, InvoiceSummaryService>();
            builder.Services.AddScoped<IInvoiceReportService, InvoiceReportService>();
            builder.Services.AddScoped<IMonthlyReportService, MonthlyReportService>();
            builder.Services.AddScoped<IWinningInvoiceReportService, WinningInvoiceReportService>();
            builder.Services.AddScoped<ICreateInvoiceService, CreateInvoiceService>();

            // 發票處理背景服務：收單端點（InvoiceService/ApplyInvoice）將存證作業寫入佇列，
            // 由 InvoiceProcessBackgroundService 取件執行，取代原本的 Task.Run。
            // 預設使用檔案佇列（作業以 JSON 落地，行程中斷重啟後接續處理）；
            // AppSettings.InvoiceProcessQueue.Persistent = false 時改用記憶體佇列。
            if (AppSettings.Default.InvoiceProcessQueue.Persistent)
            {
                builder.Services.AddSingleton<IInvoiceProcessQueue, InvoiceProcessFileQueue>();
            }
            else
            {
                builder.Services.AddSingleton<IInvoiceProcessQueue, InvoiceProcessQueue>();
            }
            builder.Services.AddHostedService<InvoiceProcessBackgroundService>();

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


            // Serve the SPA shell (index.html) with a <base href> injected at runtime
            // from Request.PathBase (the IIS sub-application path, e.g. "/TaskCenter").
            // This makes the build path-agnostic: the same wwwroot works under any
            // sub-path or the site root with no rebuild. PathBase is "" at the root,
            // giving <base href="/" />.
            async Task WriteSpaShellAsync(HttpContext context)
            {
                var pathBase = context.Request.PathBase.HasValue ? context.Request.PathBase.Value! : string.Empty;
                var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
                var html = await File.ReadAllTextAsync(indexPath, Encoding.UTF8);
                if (html.IndexOf("<base ", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    html = html.Replace("<head>", $"<head>\r\n  <base href=\"{pathBase}/\" />");
                }
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html, Encoding.UTF8);
            }

            // In production, intercept the shell document requests BEFORE static-file
            // serving so the raw (un-injected) index.html is never returned. Deep links
            // are handled by the SPA fallback below. (In development the Vite dev server
            // serves the shell via the proxy, so no injection is needed.)
            if (!app.Environment.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    var path = context.Request.Path.Value ?? string.Empty;
                    if (HttpMethods.IsGet(context.Request.Method) &&
                        (path == "/" || path.Equals("/index.html", StringComparison.OrdinalIgnoreCase)))
                    {
                        await WriteSpaShellAsync(context);
                        return;
                    }
                    await next();
                });
            }

            app.MapStaticAssets();

            // Execute endpoints EXPLICITLY here so that matched API/MVC endpoints run at this
            // point in the pipeline. In minimal hosting, calling MapControllers()/MapControllerRoute()
            // alone defers endpoint execution to an auto-appended terminal middleware placed AFTER
            // everything below (including UseSpa). Because the SPA dev-server proxy is itself a
            // terminal middleware, it would short-circuit the request before that auto-appended
            // endpoint execution runs, so /api/* requests matched a controller but never executed
            // it (returning an empty 500). Using an explicit UseEndpoints block fixes the ordering:
            // matched endpoints execute here, and only unmatched routes fall through to the SPA.
            app.UseEndpoints(endpoints =>
            {
                // Attribute-routed API controllers (e.g. AuthController -> /api/Auth/...).
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();
            });

            // The SPA fallback runs LAST and only handles requests that were NOT matched by the
            // API/MVC endpoints above.
            if (app.Environment.IsDevelopment())
            {
                // In development, proxy SPA (non-API) requests to the Vite dev server.
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
                });
            }
            else
            {
                // Client-side routes (e.g. /OrganizationQuery) fall through to the SPA
                // shell, also with the runtime-injected <base href>.
                app.MapFallback(WriteSpaShellAsync);
                // app.MapFallbackToFile("index.html");
            }

            app.Run();
        }
    }
}