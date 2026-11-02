var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.MapGet("/", () => DateTime.Now.ToString("dd.MM.yyyy"));

app.Run();
