using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Paychex.Api.Api {
    /// <summary>
    /// https://stackoverflow.com/questions/22752075/how-can-i-ignore-unknown-enum-values-during-json-deserialization
    /// </summary>
    public class TolerantEnumConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            var type = IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
            return (type?.IsEnum).GetValueOrDefault();
        }

        object TryParseEnum(Type enumType, string enumText)
        {
            var names = Enum.GetNames(enumType);
            var match = names.FirstOrDefault(n => string.Equals(n, enumText, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return Enum.Parse(enumType, match);
            }
            return null;
        }
        /// <inheritdoc />
        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        object existingValue,
                                        JsonSerializer serializer)
        {

            var isNullable = IsNullableType(objectType);
            var enumType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    var enumText = reader.Value.ToString();

                    if (!string.IsNullOrEmpty(enumText))
                    {
                        var txt = TryParseEnum(enumType, enumText);
                        if (txt != null)
                            return txt;

                        txt = TryParseEnum(enumType, enumText.Replace("-", "_"));
                        if (txt != null)
                            return txt;

                        Trace.TraceWarning(
                            $"Received value {enumText} while attempting to deserialize an enum of type {enumType.Name}"
                        );
                    }

                    break;
                case JsonToken.Integer:
                    var enumVal = Convert.ToInt32(reader.Value);
                    var values = (int[])Enum.GetValues(enumType);
                    if (values.Contains(enumVal))
                    {
                        return Enum.Parse(enumType, enumVal.ToString());
                    }

                    Trace.TraceWarning(
                        $"Received value {enumVal} while attempting to deserialize an enum of type {enumType.Name}"
                    );
                    break;
            }

            if (isNullable)
                return null;

            var names = Enum.GetNames(enumType);
            var defaultName = names.FirstOrDefault(n => string.Equals(n, "Unknown", StringComparison.OrdinalIgnoreCase))
                              ?? names.First();

            return Enum.Parse(enumType, defaultName);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue(value.ToString());

        private static bool IsNullableType(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}