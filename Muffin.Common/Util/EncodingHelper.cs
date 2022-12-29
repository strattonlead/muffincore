using System.Linq;
using System.Text;

namespace Muffin.Common.Util
{
    public static class EncodingHelper
    {
        public static int SerializeEncoding(Encoding enc)
        {
            return enc.CodePage;
        }

        public static Encoding DeserializeEncoding(int codePage)
        {
            var encodings = Encoding.GetEncodings();
            var encoding = encodings.FirstOrDefault(x => x.CodePage.Equals(codePage));
            if (encoding != null)
                return encoding.GetEncoding();
            return null;
        }
    }
}
