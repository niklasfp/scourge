var builder = WebApplication.CreateBuilder(args);

// If you want swagger for the Scourge endpoints (Not needed if you know the endpoints :D)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Scourge Services
builder.Services.AddScourge();

var app = builder.Build();

// Map scourge
app.MapScourge("/");


// If you want swagger (Not needed if you know the endpoints :D)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
