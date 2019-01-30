using System;
using System.IO;

namespace Sharper.Loggers
{
    public class FileLogger : AbstractLogger
    {
        private readonly StreamWriter _writer;

        public FileLogger(string name) : base(name)
        {
            _writer = new StreamWriter(new FileStream($"{name}.log", FileMode.Append, FileAccess.Write, FileShare.Read));
        }

        #region Overrides of AbstractLogger

        protected override void DebugOverride(string message)
            => Write(LogLevel.Debug, message);

        protected override void InfoOverride(string message)
            => Write(LogLevel.Info, message);

        protected override void WarnOverride(string message)
            => Write(LogLevel.Warn, message);

        protected override void ErrorOverride(string message)
            => Write(LogLevel.Error, message);

        #endregion

        private void Write(LogLevel level, string message)
        {
            _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}");
            _writer.Flush();
        }
    }
}