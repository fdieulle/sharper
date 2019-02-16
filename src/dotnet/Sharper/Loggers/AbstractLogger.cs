using System;
using System.Text;

namespace Sharper.Loggers
{
    public abstract class AbstractLogger : ILogger
    {
        private readonly StringBuilder _buffer = new StringBuilder();
        private LogLevel _level;

        protected AbstractLogger(string name)
        {
            Name = name;
            UpdateLevel(LogLevel.Info);
        }

        #region Implementation of ILogger

        public string Name { get; }

        public virtual LogLevel Level
        {
            get => _level;
            set
            {
                if (_level == value) return;
                UpdateLevel(value);
            }
        }

        public bool IsDebugEnabled { get; private set; }

        public void Debug(string message)
        {
            if (!IsDebugEnabled) return;

            DebugOverride(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (!IsDebugEnabled) return;

            _buffer.Clear();
            _buffer.AppendFormat(format, args);
            Debug(_buffer.ToString());
        }

        public void Debug(string message, Exception e)
        {
            if (!IsDebugEnabled) return;

            _buffer.Clear();
            _buffer.AppendLine(message);
            _buffer.AppendFormat(e);
            Debug(_buffer.ToString());
        }

        public bool IsInfoEnabled { get; private set; }

        public void Info(string message)
        {
            if (!IsInfoEnabled) return;

            InfoOverride(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (!IsInfoEnabled) return;

            _buffer.Clear();
            _buffer.AppendFormat(format, args);
            Info(_buffer.ToString());
        }

        public void Info(string message, Exception e)
        {
            if (!IsInfoEnabled) return;

            _buffer.Clear();
            _buffer.AppendLine(message);
            _buffer.AppendFormat(e);
            Info(_buffer.ToString());
        }

        public bool IsWarnEnabled { get; private set; }

        public void Warn(string message)
        {
            if (!IsWarnEnabled) return;

            WarnOverride(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (!IsWarnEnabled) return;

            _buffer.Clear();
            _buffer.AppendFormat(format, args);
            Warn(_buffer.ToString());
        }

        public void Warn(string message, Exception e)
        {
            if (!IsWarnEnabled) return;

            _buffer.Clear();
            _buffer.AppendLine(message);
            _buffer.AppendFormat(e);
            Warn(_buffer.ToString());
        }

        public bool IsErrorEnabled { get; private set; }

        public void Error(string message)
        {
            if (!IsErrorEnabled) return;

            ErrorOverride(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (!IsErrorEnabled) return;

            _buffer.Clear();
            _buffer.AppendFormat(format, args);
            Error(_buffer.ToString());
        }

        public void Error(string message, Exception e)
        {
            if (!IsErrorEnabled) return;

            _buffer.Clear();
            _buffer.AppendLine(message);
            _buffer.AppendFormat(e);
            Error(_buffer.ToString());
        }

        #endregion

        protected abstract void DebugOverride(string message);
        protected abstract void InfoOverride(string message);
        protected abstract void WarnOverride(string message);
        protected abstract void ErrorOverride(string message);

        private void UpdateLevel(LogLevel value)
        {
            _level = value;
            IsDebugEnabled = value <= LogLevel.Debug;
            IsInfoEnabled = value <= LogLevel.Info;
            IsWarnEnabled = value <= LogLevel.Warn;
            IsErrorEnabled = value <= LogLevel.Error;
        }
    }
}