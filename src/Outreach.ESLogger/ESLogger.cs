using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Outreach.ESLogger
{
    public class ESLogger : ILogger
    {
        private static readonly ESLogTask logTask = new ESLogTask();

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ESClientProvider _esClient;
        private readonly string _categoryName;
        private readonly LogLevel _logLevel;

        public ESLogger(ESClientProvider esClient, IHttpContextAccessor httpContextAccessor, string categoryName, LogLevel logLevel)
        {
            _esClient = esClient;
            _httpContextAccessor = httpContextAccessor;
            _categoryName = categoryName;
            _logLevel = logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // This is the easiest way to do this, just don't support it.  Even the aspnet logging does this, probably
            // because of thread safety issues, and a lack of thread execution context aka synchronization context in net core
            // means this would be very difficult to make thread safe.
            // https://blog.stephencleary.com/2017/03/aspnetcore-synchronization-context.html
            return NoopInstance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var entry = new LogEntry
            {
                EventId = eventId,
                DateTime = DateTime.UtcNow,
                Category = _categoryName,
                Message = message,
                Level = logLevel
            };

            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {                
                entry.TraceIdentifier = context.TraceIdentifier;
                entry.UserName = context.User.Identity.Name;
                var request = context.Request;
                entry.ContentLength = request.ContentLength;
                entry.ContentType = request.ContentType;
                entry.Host = request.Host.Value;
                entry.IsHttps = request.IsHttps;
                entry.Method = request.Method;
                entry.Path = request.Path;
                entry.PathBase = request.PathBase;
                entry.Protocol = request.Protocol;
                entry.QueryString = request.QueryString.Value;
                entry.Scheme = request.Scheme;

                entry.Cookies = request.Cookies;
                entry.Headers = request.Headers;
            }

            if (exception != null)
            {
                entry.Exception = exception.ToString();
                entry.ExceptionMessage = exception.Message;
                entry.ExceptionType = exception.GetType().Name;
                entry.StackTrace = exception.StackTrace;
            }

            logTask.QueueLog(_esClient, entry);
        }

        public class Noop: IDisposable
        {
            public void Dispose() { }
        }

        static Noop NoopInstance = new Noop();
    }
}