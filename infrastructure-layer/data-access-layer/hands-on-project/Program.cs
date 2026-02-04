using ProductAPI.Domain.Repositories;
using ProductAPI.Infrastructure.DataAccess.Context;
using ProductAPI.Infrastructure.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Product API - Dapper Example", 
        Version = "v1",
        Description = "Web API menggunakan Dapper untuk Data Access Layer"
    });
});

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register repositories with Dependency Injection
builder.Services.AddScoped<IProductRepository>(sp => 
    new ProductRepository(connectionString));

builder.Services.AddScoped<ICustomerRepository>(sp => 
    new CustomerRepository(connectionString));

builder.Services.AddScoped<IOrderRepository>(sp => 
    new OrderRepository(connectionString));

// Register Database Initializer
builder.Services.AddScoped(sp => new DatabaseInitializer(connectionString));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    try
    {
        await dbInitializer.InitializeAsync();
        Console.WriteLine("Database initialized successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
        Console.WriteLine("Note: Make sure SQL Server is running and connection string is correct.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
