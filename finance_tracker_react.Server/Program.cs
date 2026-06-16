using System.Data.Common;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;
using System.IO;
using Microsoft.EntityFrameworkCore;
using finance_tracker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<transactionsContext>(opt=> opt.UseSqlServer("Server=localhost;Database=finances;Trusted_Connection=True;TrustServerCertificate=True;"));
builder.Services.AddAntiforgery(options =>
{
    // Header name expected from the client
    options.HeaderName = "RequestVerificationToken";
    // Configure cookie so it can be sent from the dev server via proxy
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapControllers();


// Removed convenience GET at /transactions to avoid auto-loading pre-seeded data on startup.
// The frontend will load transactions only after an explicit upload.

// Endpoint to fetch and store antiforgery tokens for the SPA
app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext http) =>
{
    var tokens = antiforgery.GetAndStoreTokens(http);
    return Results.Json(new { token = tokens.RequestToken });
});

// Expose an upload endpoint at /transactions/upload that the frontend expects.
// This mirrors the controller route at /api/transactions/upload but allows the dev proxy
// to send requests to the shorter path without rewriting.
app.MapPost("/transactions/upload", async (transactionsContext db, IFormFile file, IAntiforgery antiforgery, HttpContext http) =>
{
    // Validate antiforgery token for this minimal API endpoint
    try
    {
        await antiforgery.ValidateRequestAsync(http);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.Unauthorized();
    }

    if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded.");

    using var reader = new StreamReader(file.OpenReadStream());

    // Replace existing data atomically
    using var tx = await db.Database.BeginTransactionAsync();
    try
    {
        
        await db.SaveChangesAsync();

        // Skip header if present
        var header = await reader.ReadLineAsync();
        var inserted = 0;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var cols = line.Split(',');
            if (cols.Length < 7) continue;

            string type = cols[0];
            string details = cols[1];
            string particulars = cols[2];
            string code = cols[3];
            string reference = cols[4];
            decimal amount = decimal.Parse(cols[5]);
            var date = cols[6];

            var newTx = new finance_tracker.Models.transactions
            {
                type = type,
                details = details,
                particulars = particulars,
                code = code,
                reference = reference,
                amount = amount,
                date = date
            };
            db.transactions.Add(newTx);
            inserted++;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Results.Ok(new { inserted });
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync();
        return Results.Problem(detail: ex.Message);
    }
});
app.Run();
