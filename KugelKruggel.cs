using System;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        
        // Liefert das Datum beim Aufruf der Startseite (/)
        app.MapGet("/", () => DateTime.Now.ToString("dd.MM.yyyy"));
        
        app.Run();
    }
}
