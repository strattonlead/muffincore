using System.Globalization;
using System.Text.RegularExpressions;

namespace Muffin.Common.Util
{
    public static class PrimitiveHelper
    {
        public static decimal ParseDecimal(string input)
        {
            var cultureInfo = CultureInfo.InvariantCulture;
            // if the first regex matches, the number string is in us culture
            if (Regex.IsMatch(input, @"^(:?[\d,]+\.)*\d+$"))
            {
                cultureInfo = new CultureInfo("en-US");
            }
            // if the second regex matches, the number string is in de culture
            else if (Regex.IsMatch(input, @"^(:?[\d.]+,)*\d+$"))
            {
                cultureInfo = new CultureInfo("de-DE");
            }
            var styles = NumberStyles.Number;

            decimal number;
            decimal.TryParse(input, styles, cultureInfo, out number);
            return number;
        }
    }
}
