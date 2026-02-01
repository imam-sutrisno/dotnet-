# .NET Cheatsheet

## Table of Contents
- [.NET CLI Commands](#net-cli-commands)
- [C# Basics](#c-basics)
- [Data Types](#data-types)
- [Control Structures](#control-structures)
- [Object-Oriented Programming](#object-oriented-programming)
- [LINQ (Language Integrated Query)](#linq-language-integrated-query)
- [ASP.NET Core](#aspnet-core)
- [Entity Framework Core](#entity-framework-core)
- [Dependency Injection](#dependency-injection)
- [Asynchronous Programming](#asynchronous-programming)

---

## .NET CLI Commands

### Project Management
```bash
# Create a new solution
dotnet new sln -n MySolution

# Create a new console application
dotnet new console -n MyApp

# Create a new web API
dotnet new webapi -n MyApi

# Create a new MVC application
dotnet new mvc -n MyMvcApp

# Create a new Razor Pages application
dotnet new webapp -n MyWebApp

# Create a new class library
dotnet new classlib -n MyLibrary

# Create a new xUnit test project
dotnet new xunit -n MyTests

# Create a new NUnit test project
dotnet new nunit -n MyTests

# Create a new MSTest project
dotnet new mstest -n MyTests
```

### Build and Run
```bash
# Build the project
dotnet build

# Run the project
dotnet run

# Run with specific configuration
dotnet run --configuration Release

# Clean build outputs
dotnet clean

# Publish the application
dotnet publish -c Release -o ./publish

# Watch and run (auto-reload on changes)
dotnet watch run
```

### Package Management
```bash
# Add a NuGet package
dotnet add package PackageName

# Add a specific version
dotnet add package PackageName --version 1.0.0

# Remove a package
dotnet remove package PackageName

# Restore packages
dotnet restore

# List packages
dotnet list package
```

### Solution Management
```bash
# Add project to solution
dotnet sln add MyProject/MyProject.csproj

# Remove project from solution
dotnet sln remove MyProject/MyProject.csproj

# List projects in solution
dotnet sln list
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity detailed

# Run tests with code coverage
dotnet test /p:CollectCoverage=true
```

---

## C# Basics

### Variables and Constants
```csharp
// Variable declaration
int age = 25;
string name = "John";
double price = 19.99;
bool isActive = true;

// Constants
const int MAX_VALUE = 100;
const string APP_NAME = "MyApp";

// Implicit typing
var message = "Hello World"; // string
var count = 10; // int

// Multiple declarations
int x = 1, y = 2, z = 3;
```

### String Operations
```csharp
// String concatenation
string fullName = firstName + " " + lastName;

// String interpolation
string greeting = $"Hello, {name}!";

// String formatting
string formatted = string.Format("Hello, {0}!", name);

// String methods
string upper = text.ToUpper();
string lower = text.ToLower();
string trimmed = text.Trim();
bool contains = text.Contains("substring");
string replaced = text.Replace("old", "new");
string[] parts = text.Split(',');
string joined = string.Join(", ", parts);
```

### Arrays and Collections
```csharp
// Array declaration
int[] numbers = new int[5];
int[] scores = { 95, 87, 92, 88, 94 };

// Array operations
int length = numbers.Length;
int first = numbers[0];
numbers[1] = 100;

// List<T>
List<string> names = new List<string>();
names.Add("Alice");
names.Remove("Alice");
int count = names.Count;
names.Clear();

// Dictionary<TKey, TValue>
Dictionary<string, int> ages = new Dictionary<string, int>();
ages.Add("Alice", 25);
ages["Bob"] = 30;
bool hasKey = ages.ContainsKey("Alice");
int aliceAge = ages["Alice"];

// HashSet<T>
HashSet<int> uniqueNumbers = new HashSet<int>();
uniqueNumbers.Add(1);
uniqueNumbers.Add(1); // Won't add duplicate
```

---

## Data Types

### Value Types
```csharp
// Integral types
byte b = 255;           // 0 to 255
sbyte sb = -128;        // -128 to 127
short s = -32768;       // -32,768 to 32,767
ushort us = 65535;      // 0 to 65,535
int i = -2147483648;    // -2,147,483,648 to 2,147,483,647
uint ui = 4294967295;   // 0 to 4,294,967,295
long l = 9223372036854775807;
ulong ul = 18446744073709551615;

// Floating-point types
float f = 3.14f;        // ~6-9 digits precision
double d = 3.14159265359; // ~15-17 digits precision
decimal m = 3.14159265359m; // 28-29 digits precision

// Other value types
bool flag = true;
char letter = 'A';
DateTime now = DateTime.Now;
```

### Reference Types
```csharp
// String
string text = "Hello World";

// Object
object obj = new object();

// Arrays
int[] numbers = new int[10];

// Classes
Person person = new Person();

// Delegates
Action action = () => Console.WriteLine("Action");
Func<int, int> square = x => x * x;
```

### Nullable Types
```csharp
// Nullable value types
int? nullableInt = null;
bool? nullableBool = null;

// Checking for null
if (nullableInt.HasValue)
{
    int value = nullableInt.Value;
}

// Null-coalescing operator
int result = nullableInt ?? 0; // Returns 0 if null

// Null-conditional operator
string name = person?.Name; // Returns null if person is null
int? length = name?.Length;
```

---

## Control Structures

### Conditional Statements
```csharp
// if-else
if (condition)
{
    // code
}
else if (anotherCondition)
{
    // code
}
else
{
    // code
}

// Ternary operator
int result = (x > y) ? x : y;

// switch statement
switch (value)
{
    case 1:
        Console.WriteLine("One");
        break;
    case 2:
        Console.WriteLine("Two");
        break;
    default:
        Console.WriteLine("Other");
        break;
}

// Switch expression (C# 8.0+)
string result = value switch
{
    1 => "One",
    2 => "Two",
    _ => "Other"
};
```

### Loops
```csharp
// for loop
for (int i = 0; i < 10; i++)
{
    Console.WriteLine(i);
}

// while loop
while (condition)
{
    // code
}

// do-while loop
do
{
    // code
} while (condition);

// foreach loop
foreach (var item in collection)
{
    Console.WriteLine(item);
}

// Loop control
break;    // Exit loop
continue; // Skip to next iteration
```

---

## Object-Oriented Programming

### Classes and Objects
```csharp
// Class definition
public class Person
{
    // Fields
    private string name;
    
    // Properties
    public string Name { get; set; }
    public int Age { get; set; }
    
    // Auto-implemented property
    public string Email { get; set; }
    
    // Read-only property
    public string FullName { get; }
    
    // Constructor
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
    
    // Method
    public void Introduce()
    {
        Console.WriteLine($"Hi, I'm {Name}");
    }
    
    // Static method
    public static Person Create(string name, int age)
    {
        return new Person(name, age);
    }
}

// Creating objects
Person person = new Person("John", 25);
var person2 = new Person("Jane", 30);
```

### Inheritance
```csharp
// Base class
public class Animal
{
    public string Name { get; set; }
    
    public virtual void MakeSound()
    {
        Console.WriteLine("Some sound");
    }
}

// Derived class
public class Dog : Animal
{
    public override void MakeSound()
    {
        Console.WriteLine("Woof!");
    }
}

// Using inheritance
Animal animal = new Dog();
animal.MakeSound(); // Outputs: Woof!
```

### Interfaces
```csharp
// Interface definition
public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

// Implementation
public class UserRepository : IRepository<User>
{
    public User GetById(int id) { /* implementation */ }
    public IEnumerable<User> GetAll() { /* implementation */ }
    public void Add(User entity) { /* implementation */ }
    public void Update(User entity) { /* implementation */ }
    public void Delete(int id) { /* implementation */ }
}
```

### Abstract Classes
```csharp
// Abstract class
public abstract class Shape
{
    public abstract double GetArea();
    
    public void Display()
    {
        Console.WriteLine($"Area: {GetArea()}");
    }
}

// Concrete implementation
public class Circle : Shape
{
    public double Radius { get; set; }
    
    public override double GetArea()
    {
        return Math.PI * Radius * Radius;
    }
}
```

### Access Modifiers
```csharp
public class Example
{
    public int PublicField;        // Accessible from anywhere
    private int PrivateField;      // Accessible only within class
    protected int ProtectedField;  // Accessible within class and derived classes
    internal int InternalField;    // Accessible within same assembly
    protected internal int ProtectedInternalField; // protected OR internal
    private protected int PrivateProtectedField;   // protected AND internal
}
```

---

## LINQ (Language Integrated Query)

### Basic LINQ Operations
```csharp
// Where - Filtering
var evenNumbers = numbers.Where(n => n % 2 == 0);

// Select - Projection
var names = people.Select(p => p.Name);
var nameAndAge = people.Select(p => new { p.Name, p.Age });

// OrderBy / OrderByDescending
var sorted = people.OrderBy(p => p.Age);
var sortedDesc = people.OrderByDescending(p => p.Age);

// ThenBy - Secondary sorting
var sorted = people.OrderBy(p => p.LastName)
                   .ThenBy(p => p.FirstName);

// First / FirstOrDefault
var first = people.First();
var firstOrDefault = people.FirstOrDefault();
var firstAdult = people.First(p => p.Age >= 18);

// Single / SingleOrDefault
var single = people.Single(p => p.Id == 1);

// Any / All
bool hasAdults = people.Any(p => p.Age >= 18);
bool allAdults = people.All(p => p.Age >= 18);

// Count
int count = people.Count();
int adultCount = people.Count(p => p.Age >= 18);

// Sum / Average / Min / Max
int total = numbers.Sum();
double average = numbers.Average();
int min = numbers.Min();
int max = numbers.Max();

// Distinct
var uniqueNames = names.Distinct();

// Skip / Take (Pagination)
var page = people.Skip(10).Take(10);

// GroupBy
var grouped = people.GroupBy(p => p.City);

// Join
var result = customers.Join(orders,
    c => c.Id,
    o => o.CustomerId,
    (c, o) => new { c.Name, o.OrderDate });
```

### Query Syntax
```csharp
// Query syntax
var query = from p in people
            where p.Age >= 18
            orderby p.Name
            select p;

// Equivalent method syntax
var query = people.Where(p => p.Age >= 18)
                  .OrderBy(p => p.Name)
                  .Select(p => p);
```

---

## ASP.NET Core

### Program.cs (Minimal API)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        var created = await _userService.CreateAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id)
            return BadRequest();
        
        await _userService.UpdateAsync(user);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
```

### Middleware
```csharp
// Custom middleware
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    
    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Before
        await _next(context);
        // After
    }
}

// Extension method
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomMiddleware>();
    }
}

// Usage
app.UseCustomMiddleware();
```

### Routing
```csharp
// Attribute routing
[Route("api/[controller]")]
[Route("api/users")]

// Route parameters
[HttpGet("{id}")]
[HttpGet("{id:int}")] // Constraint

// Multiple routes
[HttpGet("active")]
[HttpGet("list/active")]

// Route constraints
[HttpGet("{id:int:min(1)}")]
[HttpGet("{date:datetime}")]
[HttpGet("{name:alpha}")]
```

---

## Entity Framework Core

### DbContext
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        
        modelBuilder.Entity<User>()
            .Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
```

### Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;"
  }
}
```

### Registration
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### CRUD Operations
```csharp
// Create
var user = new User { Name = "John", Email = "john@example.com" };
_context.Users.Add(user);
await _context.SaveChangesAsync();

// Read
var user = await _context.Users.FindAsync(id);
var users = await _context.Users.ToListAsync();
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == email);

// Update
user.Name = "Updated Name";
_context.Users.Update(user);
await _context.SaveChangesAsync();

// Delete
_context.Users.Remove(user);
await _context.SaveChangesAsync();

// Include related entities
var users = await _context.Users
    .Include(u => u.Orders)
    .ToListAsync();
```

### Migrations
```bash
# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# List migrations
dotnet ef migrations list

# Generate SQL script
dotnet ef migrations script
```

---

## Dependency Injection

### Service Registration
```csharp
// Transient - Created each time they are requested
builder.Services.AddTransient<IMyService, MyService>();

// Scoped - Created once per request
builder.Services.AddScoped<IUserService, UserService>();

// Singleton - Created once and reused
builder.Services.AddSingleton<IConfiguration, Configuration>();

// Multiple implementations
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
```

### Constructor Injection
```csharp
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    
    public UserController(
        IUserService userService,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
}
```

### Service Lifetimes
- **Transient**: New instance every time
- **Scoped**: One instance per request
- **Singleton**: One instance for application lifetime

---

## Asynchronous Programming

### Async/Await
```csharp
// Async method
public async Task<User> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    return user;
}

// Async void (only for event handlers)
private async void Button_Click(object sender, EventArgs e)
{
    await DoSomethingAsync();
}

// Multiple async operations
public async Task<Result> ProcessAsync()
{
    var task1 = GetDataAsync();
    var task2 = GetMoreDataAsync();
    
    await Task.WhenAll(task1, task2);
    
    var result1 = await task1;
    var result2 = await task2;
    
    return new Result { Data1 = result1, Data2 = result2 };
}

// Parallel execution
var results = await Task.WhenAll(
    GetUser1Async(),
    GetUser2Async(),
    GetUser3Async()
);

// First completed task
var result = await Task.WhenAny(task1, task2, task3);
```

### Task Methods
```csharp
// Create completed task
Task<int> task = Task.FromResult(42);

// Delay
await Task.Delay(1000); // Wait 1 second

// Run synchronous code asynchronously
await Task.Run(() => {
    // CPU-intensive work
});

// ConfigureAwait
await SomeMethodAsync().ConfigureAwait(false);
```

### Best Practices
```csharp
// ✓ Good - Async all the way
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetUserAsync(id);
}

// ✗ Bad - Blocking on async code
public User GetUser(int id)
{
    return _repository.GetUserAsync(id).Result; // Don't do this!
}

// ✓ Good - Use Task.FromResult for already computed values
public Task<int> GetValueAsync()
{
    return Task.FromResult(42);
}
```

---

## Additional Tips

### Exception Handling
```csharp
try
{
    // Code that might throw exception
}
catch (SpecificException ex)
{
    // Handle specific exception
    _logger.LogError(ex, "Error message");
}
catch (Exception ex)
{
    // Handle general exception
    throw; // Re-throw
}
finally
{
    // Always executed
}
```

### Using Statement
```csharp
// Traditional using
using (var stream = new FileStream("file.txt", FileMode.Open))
{
    // Use stream
}

// Using declaration (C# 8.0+)
using var stream = new FileStream("file.txt", FileMode.Open);
// Stream disposed at end of scope
```

### Pattern Matching
```csharp
// Type pattern
if (obj is string s)
{
    Console.WriteLine(s.ToUpper());
}

// Property pattern
if (person is { Age: >= 18 })
{
    Console.WriteLine("Adult");
}

// Switch expression with patterns
var result = shape switch
{
    Circle { Radius: > 0 } c => c.Radius * c.Radius * Math.PI,
    Rectangle { Width: > 0, Height: > 0 } r => r.Width * r.Height,
    _ => 0
};
```

### Records (C# 9.0+)
```csharp
// Record declaration
public record Person(string Name, int Age);

// Usage
var person = new Person("John", 25);
var person2 = person with { Age = 26 }; // Non-destructive mutation
```

---

## Resources

- [Official .NET Documentation](https://docs.microsoft.com/dotnet/)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)

---

*This cheatsheet covers the essential .NET and C# concepts for quick reference.*
