using System.IO;

namespace Muffin.Common.Util
{
    public static class PathHelper
    {
        public static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string ReplaceInvalidChars(string filename)
        {
            return ReplaceInvalidChars(filename, "_");
        }

        public static string ReplaceInvalidChars(string filename, string replacement)
        {
            return string.Join(replacement, filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
