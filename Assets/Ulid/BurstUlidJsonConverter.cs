#if HAS_SYSTEM_TEXT_JSON
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibSugar.Unity
{

    public class BurstUlidJsonConverter : JsonConverter<BurstUlid>
    {
        public override BurstUlid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String) throw new JsonException("Expected string");

            try
            {
                if (reader.HasValueSequence)
                {
                    var seq = reader.ValueSequence;
                    if (seq.Length != 26) throw new JsonException("Ulid invalid: length must be 26");
                    Span<byte> buf = stackalloc byte[26];
                    seq.CopyTo(buf);
                    return BurstUlid.ParseUtf8(buf);
                }
                else
                {
                    var buf = reader.ValueSpan;
                    if (buf.Length != 26) throw new JsonException("Ulid invalid: length must be 26");
                    return BurstUlid.ParseUtf8(buf);
                }
            }
            catch (Exception e)
            {
                throw new JsonException("Ulid invalid format", e);
            }
        }

        public override unsafe void Write(Utf8JsonWriter writer, BurstUlid value, JsonSerializerOptions options)
        {
            var str = value.ToFixedString();
            var span = new ReadOnlySpan<byte>(str.GetUnsafePtr(), str.Length);
            writer.WriteStringValue(span);
        }
    }

}
#endif
