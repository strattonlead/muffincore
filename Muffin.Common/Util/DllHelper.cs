using System;
using System.Runtime.InteropServices;

namespace Muffin.Common.Util
{
    public class DllHelper
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static bool IsDllPresent(string path)
        {
            return GetModuleHandle(path) != IntPtr.Zero;
        }
    }
}
