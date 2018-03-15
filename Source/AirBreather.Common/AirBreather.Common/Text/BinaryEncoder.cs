using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace AirBreather.Text
{
    public static class BinaryEncoder
    {
        private static readonly Lazy<char[]> Lexicon = new Lazy<char[]>(BuildLexicon, LazyThreadSafetyMode.PublicationOnly);

        public static string Encode(ReadOnlySpan<byte> data)
        {
            char[] lexicon = Lexicon.Value;
            StringBuilder sb = new StringBuilder();
            int bitOffset = 0;
            while (data.Length > 2 || (data.Length == 2 && bitOffset < 2))
            {
                int indexBase = data[0] | (data[1] << 8);
                if (bitOffset > 1)
                {
                    indexBase |= data[2] << 16;
                    data = data.Slice(3);
                }
                else
                {
                    data = data.Slice(2);
                }

                sb.Append(lexicon[indexBase >> bitOffset]);
                if (--bitOffset == -1)
                {
                    bitOffset = 7;
                }
            }

            if (data.Length > 0)
            {
                int index = data[0];
                if (data.Length == 2)
                {
                    index |= data[1] << 8;
                }

                sb.Append(lexicon[index >> bitOffset]);
            }

            return sb.ToString();
        }

        private static char[] BuildLexicon()
        {
            char[] result = new char[32768];

            char nxtChar = '$';
            for (int i = 0; i < result.Length; i++)
            {
                while (!IsValid(nxtChar))
                {
                    ++nxtChar;
                }

                result[i] = nxtChar++;
            }

            return result;
        }

        private static bool IsValid(char c)
        {
            switch (Char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.OtherNumber:
                case UnicodeCategory.CurrencySymbol:
                case UnicodeCategory.MathSymbol:
                case UnicodeCategory.ModifierSymbol:
                case UnicodeCategory.OtherSymbol:
                    return true;

                default:
                    return false;
            }
        }
    }
}
