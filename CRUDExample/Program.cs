using CRUDExample;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services);
});

builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

builder.Logging.ClearProviders().AddConsole();

var app = builder.Build();

app.UseSerilogRequestLogging();

if(builder.Environment.IsEnvironment("Test") == false)
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseHttpLogging();  
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }