using System.Text;

namespace AirBreather.Text
{
    public static class EncodingEx
    {
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding();

        public static readonly Encoding Windows1252 = Encoding.GetEncoding(1252);
    }
}
