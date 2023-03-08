var builder = WebApplication.CreateBuilder(args);

// If you want swagger for the Scourge endpoints (Not needed if you know the endpoints :D)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Scourge Services
builder.Services.AddScourge();

var app = builder.Build();

// Map scourge
app.UseScourgeThrottling();
app.MapScourge("/");


// If you want swagger (Not needed if you know the endpoints :D)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// AppDomains does not exist in .net core...but some compatibility was left behind :D
AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    // The throw in async void at least gets here :D
    Console.WriteLine($"[Unhandled (IsTerminating: {eventArgs.IsTerminating})]: {eventArgs.ExceptionObject}");
};


try
{
    app.Run();
}
catch (Exception e)
{
    // No, stack overflow will never end here, it's not possible to catch a stack overflow exception like this.
    Console.WriteLine($"[FATAL]: {e}");
}
