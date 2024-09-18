using IncidentFinder.Data.Context;
using IncidentFinder.Services.EventProcessor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    ConfigureSwagger(services);
    ConfigureDatabase(services, configuration);
    services.AddHostedService<EventProcessorService>();
    services.AddScoped<EventProcessorService>();
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen();
}

void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContextFactory<Context>(options => options.UseNpgsql(connectionString));
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}
