using AppTech.Business;
using AppTech.Business.Helpers;
using AppTech.DAL;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions()
    {
        Args = Environment.GetCommandLineArgs(),
        WebRootPath = @"C:\inetpub\Apptech-Backend\wwwroot",
    }
);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddRazorRuntimeCompilation();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100000000;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600;
});
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.MaxDepth = 64;
});

// Add application services (Business and DataAccess layers)
builder.Services
    .AddDataAccess(builder.Configuration)
    .AddBusiness(builder.Configuration);

// Register your custom services
builder.Services.AddHttpClient<BankClient>();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.AccessDeniedPath = "/Home/AccessDeniedPage";
    opt.LoginPath = "/Account/Login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"C:\inetpub\Apptech-Backend\wwwroot"),
    RequestPath = ""
});

app.UseRouting();

app.UseAuthorization();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseStatusCodePagesWithRedirects("/Error/{0}"); //point to error page

app.Run(); // Run the application
