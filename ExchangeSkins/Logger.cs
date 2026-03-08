using System.Text;

namespace ExchangeSkins
{
    public enum LogLevel
    {
        TRACE,
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        CRITICAL
    }

    public class Logger
    {
        private static readonly object _lock = new object();
        private static Logger _instance;
        private readonly string _logFilePath;
        private readonly LogLevel _minLogLevel;

        private Logger(string logDirectory = null, LogLevel minLogLevel = LogLevel.TRACE)
        {
            if (string.IsNullOrEmpty(logDirectory))
            {
                logDirectory = Path.Combine(Tools.GetExecutableDir(), "Logs");
            }

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(logDirectory, $"log_{timestamp}.txt");
            _minLogLevel = minLogLevel;

            WriteToFile($"========================================");
            WriteToFile($"LOG STARTED: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            WriteToFile($"========================================\n");
        }

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void Initialize(string logDirectory = null, bool consoleOutput = true, LogLevel minLogLevel = LogLevel.TRACE)
        {
            lock (_lock)
            {
                _instance = new Logger(logDirectory, minLogLevel);
            }
        }

        private void WriteToFile(string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGGER ERROR] Failed to write to log file: {ex.Message}");
            }
        }

        private void Log(LogLevel level, string message, Exception ex = null)
        {
            if (level < _minLogLevel)
                return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string threadId = Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3);
            string levelStr = level.ToString().PadRight(8);
            
            string logMessage = $"[{timestamp}] [{threadId}] [{levelStr}] {message}";

            if (ex != null)
            {
                logMessage += $"\n  Exception: {ex.GetType().Name}: {ex.Message}";
                logMessage += $"\n  StackTrace: {ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    logMessage += $"\n  InnerException: {ex.InnerException.Message}";
                }
            }

            WriteToFile(logMessage);
       
        }

        public void Trace(string message) => Log(LogLevel.TRACE, message);
        public void Debug(string message) => Log(LogLevel.DEBUG, message);
        public void Info(string message) => Log(LogLevel.INFO, message);
        public void Warning(string message) => Log(LogLevel.WARNING, message);
        public void Error(string message, Exception ex = null) => Log(LogLevel.ERROR, message, ex);
        public void Critical(string message, Exception ex = null) => Log(LogLevel.CRITICAL, message, ex);

        public void Section(string sectionName)
        {
            string separator = new string('=', 80);
            WriteToFile($"\n{separator}");
            WriteToFile($"  {sectionName}");
            WriteToFile($"{separator}");
        }

        public void SubSection(string subSectionName)
        {
            string separator = new string('-', 80);
            WriteToFile($"\n{separator}");
            WriteToFile($"  {subSectionName}");
            WriteToFile($"{separator}");
        }
    }
}