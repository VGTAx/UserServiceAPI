using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using UserServiceAPI.Data;
using UserServiceAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<UserServiceContext>(options =>
  options.UseSqlite(builder.Configuration.GetConnectionString("ConnectionString")));

builder.Services.AddAutoMapper(typeof(UserMappingProfile));

builder.Services.AddLogging(loggingBuilder =>
{
  loggingBuilder.ClearProviders();
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerBuilder =>
{
  swaggerBuilder.SwaggerDoc("v1", new OpenApiInfo
  {
    Version = "v1",
    Title = "UserServiceAPI",
    Description = "User Service API for managing users",
  });
  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  swaggerBuilder.EnableAnnotations();
  swaggerBuilder.IncludeXmlComments(xmlPath);
});

try
{
  var app = builder.Build();

  Log.Information("Application built and starting...");
  // Configure the HTTP request pipeline.
  if (app.Environment.IsDevelopment())
  {
    app.UseSwagger();
    app.UseSwaggerUI();
  }
  app.UseCors();

  app.UseHttpsRedirection();

  app.UseMiddleware<HttpLoggingMiddleware>();

  app.UseAuthorization();

  app.MapControllers();

  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
  Log.Information("Application has stopped");
  Log.CloseAndFlush();
}
