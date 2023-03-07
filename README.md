# **Scourge** - *The punisher of your .net core web app or app*

## What can Scourge do and what the h... is this insanity.

I needed to inflict :exclamation::boom: on .net stuff running in containers, and this helped me channel all my :rage4:

> **Warning**
> This is still work in progress, and no nuget packages published...yet, that said it's as simple as cloning and run this sample [src/Contallocator](src/Contallocator)

## General stuff

* Inflict pain :punch: in both web and standard .net core applications, with special support for aspnet core.
* Allocate chunks of both managed and unmanaged memory.
* Throws hard to handle exceptions like [stackoverflow exception](https://learn.microsoft.com/en-us/dotnet/api/system.stackoverflowexception) and exceptions thrown in async.methods returning `void`
* Throw whatever exception you demand. (as long as it supports a `.ctor(string message)`
* Get Garbage Collector information and force collects.
* Get Machine simple information, total memory as seen by the app, number of processors, all configured settings and environment variables.
* **ThreadWhipper ™ :wink:** start X threads to consume 100% cpu on **all** or **some** of the processors. available to the application (*useful in the winter when you need electric heating*)
* Process information - information about the current process, environment variables, configuration, memory etc.
* Process list - list runing processes including various details.

### Coming soon

* Starve the thread pool.

> See the [Considering](#considering) section for more possible fun.

## Aspnet core specific stuff

* All the nastiness from [General Stuff](#general-stuff) exposed as rest endpoints.
* Open api. (swagger) for all the endpoints.
* Easy injection into your aspnet core app.
* Send log messages with different log levels.

### Coming soon to Scourge for aspnet core

* Inject scourge into your aspnet core application using startup hooks (enable scourge without recompinig or adding nuget packages. see [Specify the hosting startup assembly](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-7.0#specify-the-hosting-startup-assembly)

## Examples

 Enable Scourge in your aspnet core app.

1. Add a reference to the `Scourge.AspNetCore` nuget package // TODO: add nuget url
2. Use Scourge in your app, see the simple example below. or the "full" [sample/test application](src/Contallocator)

> **Warning**
> In the example below and in the "full" example there is **NO AUTHENTICATION** enabled, so don't do this in production.

```csharp
var builder = WebApplication.CreateBuilder(args);

// If you want swagger for the Scourge endpoints (Not needed if you know the endpoints :D)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Scourge Services
builder.Services.AddScourge();

var app = builder.Build();

// Map scourge
app.MapScourge("/scourge");


// If you want swagger (Not needed if you know the endpoints :D)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

```

### Considering

* CLI for Scourge
* UI for Scourge
* Socket leaking
* Random tasker *background service that will consume memory and cpu cycles at random.*
* Random failures from the middleware (must make sure this middleware is one of the first, investigate if it's also possible via starup hooks)
* Feel free to [add ideas](https://github.com/niklasfp/scourge/issues/new) (play nice, start the title with [IDEA])
* Contribute your own hurtful ideas [Contributing To Scourge](#contributing-to-scurge)

## Contributing to Scurge

You're more than welcome to contibute to Scurge, the process is simple.

1. **Come to terms with the fact that Scourge is .net 7 and above only.**
2. Decide if you want to contribute :punch:hurt, information, ui or just nice helpers
3. Fork Scurge
4. Add your stuff
5. Submit a pull request.

**Scource** meaning according to [www.merriam-webster.com](https://www.merriam-webster.com/dictionary/scourge)

>*WHIP especially : one used to inflict pain or punishment.*  
>*An instrument of punishment or criticism.*  
>*Cause of wide or great affliction.*  
