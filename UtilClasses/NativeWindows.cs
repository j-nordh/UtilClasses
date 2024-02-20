using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UtilClasses
{
    public static class NativeWindows
    {
        [Flags]
        public enum ExecutionState : uint
        {
            Continuous = 0x80000000,
            SystemRequired = 0x00000001,
            DisplayRequired = 0x00000002,
            AwayModeRequired = 0x00000040,
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);
    }
}
