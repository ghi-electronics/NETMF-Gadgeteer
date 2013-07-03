using System.Collections;

namespace NETMF.OpenSource.XBee.Util
{
    /// <summary>
    ///   TODO: Update Comments
    ///     
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        Off,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        Fatal,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        Error,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        Warn,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        Info,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        Debug,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        LowDebug,

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        All
    }

    /// <summary>
    /// TODO: Update Comments
    /// </summary>
    public static class Logger
    {
        public static LogLevel LoggingLevel { get; set; }

        private static readonly Hashtable LevelNames;
        private static readonly Hashtable LogWritters;

        static Logger()
        {
            LoggingLevel = LogLevel.Info;

            LevelNames = new Hashtable
            {
                {LogLevel.Fatal, "Fatal"},
                {LogLevel.Error, "Error"},
                {LogLevel.Warn, "Warn"},
                {LogLevel.Info, "Info"},
                {LogLevel.Debug, "Debug"},
                {LogLevel.LowDebug, "LowDebug"},
            };

            LogWritters = new Hashtable
            {
                {LogLevel.Fatal, null},
                {LogLevel.Error, null},
                {LogLevel.Warn, null},
                {LogLevel.Info, null},
                {LogLevel.Debug, null},
                {LogLevel.LowDebug, null},
            };
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="logWritter"></param>
        /// <param name="logLevel"></param>
        public static void Initialize(LogWriteDelegate logWritter, params LogLevel[] logLevel)
        {
            if (logLevel.Length == 0 || logLevel[0] == LogLevel.All)
            {
                foreach (var level in LogWritters.Keys)
                    LogWritters[level] = logWritter;
            }
            else
            {
                foreach (var level in logLevel)
                    LogWritters[level] = logWritter;
            }
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static bool IsActive(LogLevel level)
        {
            return LessOrEqual(level, LoggingLevel);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="level"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool LessOrEqual(LogLevel level, LogLevel from)
        {
            return level <= from;
        }


        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        public static void Fatal(string message)
        {
            Log(message, LogLevel.Fatal);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Log(message, LogLevel.Error);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            Log(message, LogLevel.Warn);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        public static void LowDebug(string message)
        {
            Log(message, LogLevel.LowDebug);
        }

        /// <summary>
        /// TODO: Update Comments
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageLevel"></param>
        private static void Log(string message, LogLevel messageLevel)
        {
            var logWritter = LogWritters[messageLevel] as LogWriteDelegate;

            if (IsActive(messageLevel) && logWritter != null)
                logWritter.Invoke(LevelNames[messageLevel] + "\t" + message);
        }
    }

    /// <summary>
    /// TODO: Update Comments
    /// </summary>
    /// <param name="message"></param>
    public delegate void LogWriteDelegate(string message);
}