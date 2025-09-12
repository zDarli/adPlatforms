using AdPlatforms.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/platforms/load", async (IFormFile file) =>
{
    
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest(new
        {
            status = "error",
            error = "���� ������������ ��� ����."
        });
    }

    using var reader = new StreamReader(file.OpenReadStream());
    var text = await reader.ReadToEndAsync();

    if (text.Split('\n').Any(line => line.Count(c => c == ':') != 1))
    {
        return Results.BadRequest(new
        {
            status = "error",
            error = "������ ������ ������ ��������� ����� ���� ���������."
        });
    }

    var newIndex = LocationUtils.BuiltIndexFromText(text);
    PlatformStore.Instance.ReplaceIndex(newIndex);

    return Results.Ok(new
    {
        status = "ok",
    });
})
.DisableAntiforgery()
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("LoadPlatforms");


app.MapPost("/api/platforms/find", async (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    var input = (await reader.ReadToEndAsync()).Trim();
    if (string.IsNullOrEmpty(input))
    {
        return Results.BadRequest(new
        {
            status = "error",
            error = "������ ������."
        });
    }

    var index = PlatformStore.Instance.Index;
    var result = LocationUtils.FindPlatforms(input, index);
    return Results.Ok(new { status = "ok", platforms = result });
})
.Accepts<string>("text/plain")
.Produces(StatusCodes.Status200OK)
.WithName("FindPlatforms");
app.Run();


public sealed class PlatformStore
{
    // --- �������� ---
    private static readonly Lazy<PlatformStore> _instance =
        new Lazy<PlatformStore>(() => new PlatformStore());

    public static PlatformStore Instance => _instance.Value;

    // --- ��������� ---
    private Dictionary<string, HashSet<string>> _index
        = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

    private PlatformStore() { } // ��������� �����������

    // --- ������ ������ �������� ---
    public void ReplaceIndex(Dictionary<string, HashSet<string>> newIndex)
    {
        // ��������� ������ ������
        Interlocked.Exchange(ref _index, newIndex);
    }

    // --- ������ ---
    public Dictionary<string, HashSet<string>> Index
    {
        get
        {
            // ������ ����� �������
            var copy = new Dictionary<string, HashSet<string>>(
                _index.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var kv in _index)
            {
                // ��� ������� ����� ������ ����� ���������
                copy[kv.Key] = new HashSet<string>(kv.Value, StringComparer.OrdinalIgnoreCase);
            }
            return copy;
        }
    }
}