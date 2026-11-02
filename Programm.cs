using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite-Datenbank registrieren
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

var app = builder.Build();

// Tabelle beim Start anlegen, falls sie noch nicht existiert
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// -------------------------------------------------------------
// TEIL 1: Order-Service Endpoints
// -------------------------------------------------------------

// POST /api/orders: Speichert Bestellung mit Zeitstempel & Status Pending
app.MapPost("/api/orders", async (CreateOrderRequest request, AppDbContext db) =>
{
    var order = new Order
    {
        CustomerEmail = request.CustomerEmail,
        Item = request.Item,
        Category = request.Category ?? "Sonstiges",
        Price = request.Price,
        CreatedAt = DateTime.UtcNow,
        Status = "Pending"
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/api/orders/{order.Id}", order);
});

// Hilfs-Endpunkt: Alle Bestellungen ansehen
app.MapGet("/api/orders", async (AppDbContext db) =>
    await db.Orders.ToListAsync());

// -------------------------------------------------------------
// TEIL 2: Analytics-Service Endpoints
// -------------------------------------------------------------

// GET /api/analytics/total-revenue: Berechnet Summe aller Verkäufe (SUM)
app.MapGet("/api/analytics/total-revenue", async (AppDbContext db) =>
{
    var total = await db.Orders
        .AsNoTracking()
        .SumAsync(o => o.Price);

    return Results.Ok(new { TotalRevenue = total });
});

// GET /api/analytics/by-category: Gruppiert nach Kategorie & summiert Umsatz (GROUP BY + SUM)
app.MapGet("/api/analytics/by-category", async (AppDbContext db) =>
{
    var stats = await db.Orders
        .AsNoTracking()
        .GroupBy(o => o.Category)
        .Select(g => new
        {
            Category = g.Key,
            TotalRevenue = g.Sum(o => o.Price),
            OrderCount = g.Count()
        })
        .ToListAsync();

    return Results.Ok(stats);
});

app.Run();

// -------------------------------------------------------------
// Datenmodell & DbContext
// -------------------------------------------------------------

public class Order
{
    public int Id { get; set; }
    public required string CustomerEmail { get; set; }
    public required string Item { get; set; }
    public string Category { get; set; } = "Sonstiges";
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";
}

public record CreateOrderRequest(
    string CustomerEmail, 
    string Item, 
    decimal Price, 
    string? Category
);

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
}
