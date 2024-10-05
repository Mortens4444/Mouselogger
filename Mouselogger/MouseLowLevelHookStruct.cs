using System;
using System.Runtime.InteropServices;

namespace Mouselogger
{
    [StructLayout(LayoutKind.Sequential)]

    public class MouseLowLevelHookStruct
    {
        public PointStruct pt;
        public int mouseData;
        public int flags;
        public int time;
        public UIntPtr dwExtraInfo;
    }
}
