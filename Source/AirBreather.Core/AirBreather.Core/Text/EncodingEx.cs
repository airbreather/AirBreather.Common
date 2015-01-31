using System.Text;

namespace AirBreather.Core.Text
{
    public static class EncodingEx
    {
        // srsly, why use a BOM with UTF-8 by default (Encoding.UTF8)?
        // are we trying to make it easier for consumers
        // to guess the encoding of our text?  ... I promise
        // that if I give myself a stream of bytes that should be
        // interpreted as text, I'll also provide the encoding.
        // is this really such a complicated thing?
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding();
    }
}
