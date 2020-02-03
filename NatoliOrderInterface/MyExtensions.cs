using System;
using System.Collections.Generic;
using System.Text;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static string GetFileNameFromPath(this string path)
        {
            return path.Substring(path.LastIndexOf("\\") + 1, path.IndexOf(".pdf") - path.LastIndexOf("\\") - 1);
        }
    }
}
