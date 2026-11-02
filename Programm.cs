using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Verbindung zur selben SQLite-Datei 'orders.db'
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

var app = builder.Build();

// Endpunkt 1: Gesamter Umsatz (SUM)
app.MapGet("/api/analytics/total-revenue", async (AnalyticsDbContext db) =>
{
    // AsNoTracking() optimiert reine Leseabfragen
    var totalRevenue = await db.Orders
        .AsNoTracking()
        .SumAsync(o => o.Price);

    return Results.Ok(new 
    { 
        TotalRevenue = totalRevenue 
    });
});

// Endpunkt 2: Umsatz gruppiert nach Kategorie (GROUP BY + SUM)
app.MapGet("/api/analytics/by-category", async (AnalyticsDbContext db) =>
{
    var stats = await db.Orders
        .AsNoTracking()
        .GroupBy(o => o.Category)
        .Select(group => new
        {
            Category = group.Key ?? "Unkategorisiert",
            TotalRevenue = group.Sum(o => o.Price),
            Count = group.Count()
        })
        .ToListAsync();

    return Results.Ok(stats);
});

app.Run();

// --- Modelle & Read-Only DbContext ---

public class Order
{
    public int Id { get; set; }
    public required string CustomerEmail { get; set; }
    public required string Item { get; set; }
    public string? Category { get; set; } // Feld für die Kategorisierung
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";
}

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}
