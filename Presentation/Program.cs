using Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Service Layer
builder.Services.InfrastructureServices(builder.Configuration);


var app = builder.Build();

// Middleware Pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
