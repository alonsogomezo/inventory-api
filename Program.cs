using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/productos", (AppDbContext db) =>
{
    return db.Productos.ToList();
});

app.MapPost("/productos", (AppDbContext db, Producto nuevoProducto) =>
{
    if(!EsValido(nuevoProducto))
    {
        return Results.BadRequest("Datos inválidos: el nombre es obligatorio, el precio debe ser mayor a cero y el stock no puede ser negativo.");
    }
    db.Add(nuevoProducto);
    db.SaveChanges();
    return Results.Ok(nuevoProducto);
});

app.MapDelete("/productos/{id}", (AppDbContext db, int id) =>
{
    Producto producto = db.Productos.Find(id);
    if(producto == null)
    {
        return Results.NotFound();
    }
    db.Productos.Remove(producto);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/productos/{id}", (int id, AppDbContext db, Producto actualProducto) =>
{
    Producto producto = db.Productos.Find(id);
    if(producto == null)
    {
        return Results.NotFound();
    }
    if(!EsValido(actualProducto))
    {
        return Results.BadRequest("Datos inválidos: el nombre es obligatorio, el precio debe ser mayor a cero y el stock no puede ser negativo.");
    }
    producto.Nombre = actualProducto.Nombre;
    producto.Precio = actualProducto.Precio;
    producto.Stock = actualProducto.Stock;
    db.SaveChanges();
    return Results.NoContent();
});

bool EsValido(Producto producto)
{
    if(producto.Precio > 0 && producto.Stock >= 0 && !string.IsNullOrWhiteSpace(producto.Nombre))
    {
        return true;
    }
    return false; 
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}