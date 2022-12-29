using System.Drawing;

namespace Muffin.Common.Util
{
    public static class ColorHelper
    {
        public const string BLACK = "#000000";
        public const string WHITE = "#FFFFFF";

        public static string ToHex(int d)
        {
            var invertHex = d.ToString("X6");
            return string.Format("#{0}{1}{2}{3}{4}{5}", invertHex[4], invertHex[5], invertHex[2], invertHex[3], invertHex[0], invertHex[1]);
        }

        public static int ToInt32(string hexValue)
        {
            var _hexValue = string.Copy(hexValue).Replace("#", "");
            return int.Parse(_hexValue, System.Globalization.NumberStyles.HexNumber);
        }

        public static string ToInverseHex(string hex)
        {
            var color = ColorFromHex(hex);
            var inverse = Color.FromArgb(color.ToArgb() ^ 0xffffff);
            return "#" + inverse.R.ToString("X2") + inverse.G.ToString("X2") + inverse.B.ToString("X2");
        }

        public static string ToInverseHex(int d)
        {
            var hex = d.ToString("X6");
            return ToInverseHex(hex);
        }

        public static bool UseBlack(int d, double margin = 0.7)
        {
            return UseBlack(ToHex(d), margin);
        }

        public static bool UseBlack(string hex, double margin = 0.7)
        {
            if (hex == null)
                return true;
            if (hex.Length <= 6)
                return true;
            var s_r = hex.Substring(1, 2);
            var s_g = hex.Substring(3, 2);
            var s_b = hex.Substring(5, 2);

            var r = int.Parse(s_r, System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(s_g, System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(s_b, System.Globalization.NumberStyles.HexNumber);

            //const double gamma = 2.2f;
            var L = 0.2126 * r
                    + 0.7152 * g
                    + 0.0722 * b;

            return L > margin;
        }

        public static bool UseWhite(int d)
        {
            return UseWhite(ToHex(d));
        }

        public static bool UseWhite(string hex)
        {
            return !UseBlack(hex);
        }

        public static Color ColorFromHex(string hex)
        {
            if (hex == null)
                return Color.Black;
            if (hex.Length <= 6)
                return Color.Black;
            var s_r = hex.Substring(1, 2);
            var s_g = hex.Substring(3, 2);
            var s_b = hex.Substring(5, 2);

            var r = int.Parse(s_r, System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(s_g, System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(s_b, System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(r, b, g);
        }

        public static bool IsBrighterThan(string hex1, string hex2)
        {
            return ColorFromHex(hex1).GetBrightness() > ColorFromHex(hex2).GetBrightness();
        }

        public static string GetReadableColor(string hexColor)
        {
            return ColorHelper.IsBrighterThan(hexColor, "#0000FF") ? ColorHelper.BLACK : ColorHelper.WHITE;
        }
    }
}
