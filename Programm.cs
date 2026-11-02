using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. SQLite DbContext registrieren (Datenbankdatei: orders.db)
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

var app = builder.Build();

// 2. Datenbank und Tabellen beim Start automatisch erzeugen, falls nicht vorhanden
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

// 3. Endpunkt: POST /api/orders
app.MapPost("/api/orders", async (CreateOrderRequest request, OrderDbContext db) =>
{
    var order = new Order
    {
        CustomerEmail = request.CustomerEmail,
        Item = request.Item,
        Price = request.Price,
        CreatedAt = DateTime.UtcNow, // Zeitstempel
        Status = "Pending"           // Status Pending
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/api/orders/{order.Id}", order);
});

// Optional: Test-Endpunkt zum Prüfen der gespeicherten Bestellungen
app.MapGet("/api/orders", async (OrderDbContext db) =>
    await db.Orders.ToListAsync());

app.Run();

// --- Modelle & DbContext ---

public class Order
{
    public int Id { get; set; }
    public required string CustomerEmail { get; set; }
    public required string Item { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";
}

public record CreateOrderRequest(string CustomerEmail, string Item, decimal Price);

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}
