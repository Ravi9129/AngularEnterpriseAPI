using AngularEnterpriseAPI.Middleware;
using AngularEnterpriseAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services, repositories and other dependencies
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddAutoMapperProfiles();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // serve the Swagger UI at app's root (http://localhost:<port>/)
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Angular Enterprise API v1");
        c.RoutePrefix = string.Empty;
    });
}
// Add these middleware registrations after error handling middleware
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<AuditMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();