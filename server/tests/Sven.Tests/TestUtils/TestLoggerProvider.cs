using Microsoft.Extensions.Logging;

namespace Sven.Tests.TestUtils
{
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly List<LogEntry> _logs = new();

        public List<LogEntry> Logs => _logs;

        public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, _logs);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        public class LogEntry
        {
            public string Category { get; set; } = "";
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; } = "";
            public Exception? Exception { get; set; }
        }

        private class TestLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly List<LogEntry> _logList;

            public TestLogger(string categoryName, List<LogEntry> logList)
            {
                _categoryName = categoryName;
                _logList = logList;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                _logList.Add(new LogEntry
                {
                    Category = _categoryName,
                    LogLevel = logLevel,
                    Message = formatter(state, exception),
                    Exception = exception
                });
            }

            private class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                protected virtual void Dispose(bool disposing) { }
            }
        }
    }

}
