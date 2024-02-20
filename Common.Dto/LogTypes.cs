using System;

namespace Common.Dto
{
    [Flags]
    public enum LogTypes
    {
        NoLog = 0,
        StartStopResult = 1,
        Warnings = 2,
        Message = 4,
        Response = 8,
        Debug = 16,
        Error = 32,
        Flagged = 64,
        Sync = 128,
        Everything = int.MaxValue
    }
}