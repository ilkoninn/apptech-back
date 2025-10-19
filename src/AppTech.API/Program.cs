using AppTech.API;
using AppTech.Business;
using AppTech.Business.Helpers;
using AppTech.DAL;
using AppTech.DAL.Persistence;
using static AppTech.Business.BusinessDependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddDataAccess(builder.Configuration)
    .AddBusiness(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<BankClient>();

builder.Services.AddRateLimiter();
builder.Services.AddSwagger();
builder.Services.AddJwt(builder.Configuration);
builder.Services.AddMemoryCache();

// Build the application
var app = builder.Build();

// Add custom middlewares here
app.AddMiddlewares();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AppTech.API v1");
        c.EnablePersistAuthorization();
    });

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

// Apply CORS policies
app.UseCors("AllowReactApp");

using var scope = app.Services.CreateScope();
await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

//app.MapHub<UserActivityHub>("/userActivityHub");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseRateLimiter();

app.Run();
