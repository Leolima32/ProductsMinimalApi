using ProductsMinimalApi.Data;
using ProductsMinimalApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<AppDbContext>("Data Source=app.db;Cache=Shared");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/application-logs-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/products", (AppDbContext context) =>
{
    return context.Products.ToList();
});

app.MapGet("/api/products/{Id}", (AppDbContext context, Guid id) =>
{
    return context.Products.Where(x => x.Id == id).FirstOrDefault();
});

app.MapPost("/api/products", (AppDbContext context, Product product) =>
{
    context.Add(product);
    context.SaveChanges();

    Log.Information("New product insert: {@product}", product);

    return Results.Created($"api/products/{product.Id}", product);
});

app.MapPut("/api/products", (AppDbContext context, Product product) =>
{
    var currentProduct = context.Products.Where(x => x.Id == product.Id).FirstOrDefault();

    if(currentProduct == null)
        return Results.NotFound();

    currentProduct.Name = product.Name;
    currentProduct.Category = product.Category;
    currentProduct.Price = product.Price;

    context.SaveChanges();

    Log.Information("Product {@Id} updated: {@Product}", product.Id, product);

    return Results.Created($"api/products/{product.Id}", product);
});

app.MapDelete("/api/products/{Id}", (AppDbContext context, Guid id) =>
{
    var currentProduct = context.Products.Where(x => x.Id == id).FirstOrDefault();

    if(currentProduct == null)
        return Results.NotFound();
        
    context.Products.Remove(currentProduct);
    context.SaveChanges();

    Log.Information("Product {@Id} removed", id);

    return Results.Ok();
});

app.Run();
