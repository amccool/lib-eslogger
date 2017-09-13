# lib-eslogger
An implementation of ILogger that logs to Elastic Search.  A large chunk of code was borrowed from [here](https://github.com/DaniJG/ELKLogging).

See example for usage.

## Install

Install the nuget package `Outreach.ESLogger` or add a reference in your csproj and run `dotnet restore`.

```
<PackageReference Include="Outreach.ESLogger" Version="1.0.0"/>
```

## Usage

In `Startup.cs` put the using at the top.

```
using Outreach.ESLogger;
```

Then configure it like so.

```
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureESLogger(Configuration.GetSection("ElasticSearch"));
            
            // The rest...
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"))
                         .AddDebug()
                         .AddESLogger(app.ApplicationServices, "log-");
            
            // The rest...
        }
```

And be sure to have a section in your `appsettings.json` for `ElasticSearch`

```
{
    "ElasticSearch": {
        "Uri": "http://localhost:9200/",
        "DefaultIndex": "default-",
        "UserName": null,
        "Password": null
    }
}
```
