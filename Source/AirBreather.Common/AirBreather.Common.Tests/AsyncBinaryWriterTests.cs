using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using AirBreather.IO;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class AsyncBinaryWriterTests
    {
        [Fact]
        public async Task TestAsyncBinaryWriter()
        {
            bool customBool;
            unsafe
            {
                byte customByte = 39;
                customBool = *(bool*)&customByte;
            }

            object[] expectedResults =
            {
                true,
                false,
                customBool,
                (byte)42,
                (sbyte)-28,
                (short)-279,
                (ushort)64221,
                (int)-288888,
                (uint)3310229011,
                (long)-19195205991011,
                (ulong)11223372036854775807,
                (decimal)295222.2811m,
                (float)3811.55f,
                (double)Math.PI
            };

            using (var ms = new MemoryStream())
            {
                using (var wr = new AsyncBinaryWriter(ms, Encoding.Default, leaveOpen: true))
                {
                    foreach (dynamic obj in expectedResults)
                    {
                        await wr.WriteAsync(obj).ConfigureAwait(false);
                    }
                }

                ms.Position = 0;

                using (var rd = new BinaryReader(ms, Encoding.Default, leaveOpen: true))
                {
                    foreach (var obj in expectedResults)
                    {
                        switch (obj)
                        {
                            case bool b8:
                                ////Assert.Equal(b8, rd.ReadBoolean());
                                if (b8)
                                {
                                    Assert.True(rd.ReadBoolean());
                                }
                                else
                                {
                                    Assert.False(rd.ReadBoolean());
                                }

                                break;

                            case byte u8:
                                Assert.Equal(u8, rd.ReadByte());
                                break;

                            case sbyte s8:
                                Assert.Equal(s8, rd.ReadSByte());
                                break;

                            case short s16:
                                Assert.Equal(s16, rd.ReadInt16());
                                break;

                            case ushort u16:
                                Assert.Equal(u16, rd.ReadUInt16());
                                break;

                            case int s32:
                                Assert.Equal(s32, rd.ReadInt32());
                                break;

                            case uint u32:
                                Assert.Equal(u32, rd.ReadUInt32());
                                break;

                            case long s64:
                                Assert.Equal(s64, rd.ReadInt64());
                                break;

                            case ulong u64:
                                Assert.Equal(u64, rd.ReadUInt64());
                                break;

                            case float f32:
                                Assert.Equal(f32, rd.ReadSingle());
                                break;

                            case double f64:
                                Assert.Equal(f64, rd.ReadDouble());
                                break;

                            case decimal d128:
                                Assert.Equal(d128, rd.ReadDecimal());
                                break;
                        }
                    }
                }
            }
        }
    }
}
