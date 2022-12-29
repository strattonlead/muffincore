using System.Text.RegularExpressions;

namespace Muffin.Common.Util
{
    public static class MailHelper
    {
        const string pattern =
   @"^([0-9a-zA-Z]" +
   @"([\+\-_\.][0-9a-zA-Z]+)*" +
   @")+" +
   @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

        public static bool IsValidMail(this string mail)
        {
            if (mail == null)
                return false;

            try
            {
                var regex = new Regex(pattern);
                return regex.IsMatch(mail);
            }
            catch
            {
                return false;
            }
        }
    }
}
