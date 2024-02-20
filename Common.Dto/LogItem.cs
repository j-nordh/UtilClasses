using System;
using System.Threading;

namespace Common.Dto
{
    public class LogItem
    {
        public DateTime Occurred { get; set; }
        public int ThreadId { get; set; }
        public string Message { get; set; }
        public string Section { get; set; }
        public LogTypes LogType { get; set; }
        public int RouteKey { get; set; }
        public override string ToString() => $"{Occurred.ToString("s").Replace("T", " ")}\t{ThreadId}\t{Section}\t{Message}";
        //                                     Should have been Occurred.ToSaneString() but that was a circular dependency...
        public LogItem():this("", LogTypes.NoLog)
        { }

        public LogItem(string message, LogTypes type): this (message, DateTime.Now, Thread.CurrentThread.ManagedThreadId, type)
        {}

        public LogItem(string message, DateTime occurred, int threadId, LogTypes type)
        {
            Message = message;
            Occurred = occurred;
            ThreadId = threadId;
            LogType = type;
        }
    }

    public static class LogItemExtensions
    {
        public static LogItem WithSection(this LogItem logitem, string section)
        {
            logitem.Section = section;
            return logitem;
        }

        public static LogItem WithKey(this LogItem logitem, int key)
        {
            logitem.RouteKey = key;
            return logitem;
        }
    }
}