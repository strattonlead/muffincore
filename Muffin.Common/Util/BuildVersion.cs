namespace Muffin.Common.Util
{
    public static class BuildVersion
    {
        private static string _version = null;
        public static string GetVersion()
        {
            if (_version == null)
                _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return _version;
        }
    }
}
