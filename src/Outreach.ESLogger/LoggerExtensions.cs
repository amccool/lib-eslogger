using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Outreach.ESLogger
{
    public static class LoggerExtensions
    {
        public static ILoggerFactory AddESLogger(this ILoggerFactory factory, IServiceProvider serviceProvider, string indexName = null, FilterLoggerSettings filter = null)
        {
            factory.AddProvider(new ESLoggerProvider(serviceProvider, indexName, filter));
            return factory;
        }

        public static IServiceCollection ConfigureESLogger(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions();
            services.Configure<ESOptions>(config);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ESClientProvider>();

            return services;
        }
    }
}