using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string resolution
var connection = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")
    : Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");

// Validate connection string
if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

// Register DbContext
builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseSqlServer(connection));

var app = builder.Build();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapGet("/", () => "Hello world!");

app.MapGet("/Person", (PersonDbContext context) =>
    context.Person.ToList());

app.MapPost("/Person", (Person person, PersonDbContext context) =>
{
    context.Person.Add(person);
    context.SaveChanges();
    return Results.Created($"/Person/{person.Id}", person);
});

app.Run();

// Models
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) { }
    public DbSet<Person> Person { get; set; }
}
