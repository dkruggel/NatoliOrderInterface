﻿using System;
using System.Runtime.InteropServices;

namespace NatoliOrderInterface
{
    internal static class NativeMethods
    {
        #region DLL Imports
        [DllImport("USER32.DLL")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        #endregion
    }
}
