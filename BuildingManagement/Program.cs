using BuildingManagement.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add services using extensions
builder.Services
    .AddDatabaseServices(builder.Configuration)
    .AddApiManagers()
    .AddContextManagers()
    .AddSwaggerServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerServices();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
