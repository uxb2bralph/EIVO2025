using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SpaServices;

var builder = WebApplication.CreateBuilder(args);

// Minimal static file host for the built frontend
builder.Services.AddRouting();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // In development, proxy SPA requests to Vite dev server
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
else
{
    // Serve static files from wwwroot in production
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Fallback to index.html for SPA routes
    app.MapFallbackToFile("index.html");
}

app.Run();
