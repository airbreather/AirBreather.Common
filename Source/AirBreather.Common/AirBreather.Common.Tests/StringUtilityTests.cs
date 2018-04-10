using Xunit;

namespace AirBreather.Tests
{
    public sealed class StringUtilityTests
    {
        public static object[][] ToHexStringTestData =
        {
            new object[] { "" },
            new object[] { "1bce06897182AB16A277b827" },
            new object[] { "766EA0CDB76DA3B2BA76DC1057ED14EBA6A0D07456C4A1762B0E38384D67490A00E1E790AA2153D151F2202061F9A96E0C3E397CFB7BA6B6729EEB894B68FFCD7939BFFF9AA1A1BC781A2A1985585546E279A0160E44A706C6BAF475924E96A65CAAB32DDEB7496AFD79ABBB8EA917B111CBEDA4BB80CAEE56EDF538158704D3BA6D21B5A6F10E8318000BFA470A9D95C9E79942974A88CF8C7E5C59690DD7AE" },
        };

        [Theory]
        [MemberData(nameof(ToHexStringTestData))]
        public void ToHexStringTest(string s)
        {
            var b = s.HexStringToByteArrayChecked();
            var s2 = StringUtility.ToHexString(b);

            Assert.Equal(s, s2, ignoreCase: true);
        }
    }
}
