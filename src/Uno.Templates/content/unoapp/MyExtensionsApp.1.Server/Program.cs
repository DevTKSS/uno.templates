//+:cnd:noEmit
#if (useHttp && useServer)
using System.Text.Json.Serialization.Metadata;
#endif
#if (useSerilog)
using Serilog;
#endif
#if (useWasm)
using Uno.Wasm.Bootstrap.Server;
#endif
#if (useHttp && useServer)
using MyExtensionsApp._1.DataContracts.Serialization;
#endif

try
{
#if (useSerilog)
    Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine("App_Data", "Logs", "log.txt"))
            .CreateLogger();
#endif
    var builder = WebApplication.CreateBuilder(args);
#if (useSerilog)
    SerilogHostBuilderExtensions.UseSerilog(builder.Host);
#endif

#if (useHttp && useServer)
    // Configure the JsonOptions to use the generated WeatherForecastContext
    builder.Services.Configure<JsonOptions>(options =>
        options.JsonSerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
            WeatherForecastContext.Default
        ));
#endif
    // Configure the RouteOptions to use lowercase URLs
    builder.Services.Configure<RouteOptions>(options =>
        options.LowercaseUrls = true);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        // Include XML comments for all included assemblies
        Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml")
            .Where(x => x.Contains("MyExtensionsApp._1")
                && File.Exists(Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Path.GetFileNameWithoutExtension(x)}.dll")))
            .ToList()
            .ForEach(path => c.IncludeXmlComments(path));
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
#if useWasm

    app.UseUnoFrameworkFiles();
    app.MapFallbackToFile("index.html");
#endif

#if useServer
    app.MapWeatherApi();
#endif
    app.UseStaticFiles();

    await app.RunAsync();
}
catch (Exception ex)
{
#if (useSerilog)
    Log.Fatal(ex, "Application terminated unexpectedly");
#else
    Console.Error.WriteLine("Application terminated unexpectedly");
    Console.Error.WriteLine(ex);
#endif
//-:cnd:noEmit
#if DEBUG
    if (System.Diagnostics.Debugger.IsAttached)
    {
        System.Diagnostics.Debugger.Break();
    }
#endif
//+:cnd:noEmit
}
#if (useSerilog)
finally
{
    Log.CloseAndFlush();
}
#endif
//-:cnd:noEmit
