using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MockUp_CardZ.Infra.Common
{
    public class SafeCollectionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //This not works for Populate (on existingValue)
            return serializer.Deserialize<JToken>(reader).ToObjectCollectionSafe(objectType, serializer);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    public static class SafeJsonConvertExtensions
    {
        public static object ToObjectCollectionSafe(this JToken jToken, Type objectType)
        {
            return ToObjectCollectionSafe(jToken, objectType, JsonSerializer.CreateDefault());
        }

        public static object ToObjectCollectionSafe(this JToken jToken, Type objectType, JsonSerializer jsonSerializer)
        {
            var expectArray = typeof(System.Collections.IEnumerable).IsAssignableFrom(objectType);

            if (jToken is JArray jArray)
            {
                if (!expectArray)
                {
                    //to object via singel
                    if (jArray.Count == 0)
                        return JValue.CreateNull().ToObject(objectType, jsonSerializer);

                    if (jArray.Count == 1)
                        return jArray.First.ToObject(objectType, jsonSerializer);
                }
            }
            else if (expectArray)
            {
                //to object via JArray
                return new JArray(jToken).ToObject(objectType, jsonSerializer);
            }

            return jToken.ToObject(objectType, jsonSerializer);
        }

        public static T ToObjectCollectionSafe<T>(this JToken jToken)
        {
            return (T)ToObjectCollectionSafe(jToken, typeof(T));
        }

        public static T ToObjectCollectionSafe<T>(this JToken jToken, JsonSerializer jsonSerializer)
        {
            return (T)ToObjectCollectionSafe(jToken, typeof(T), jsonSerializer);
        }
    }
    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? true : false);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var expectArray = typeof(bool).IsAssignableFrom(objectType);
            return reader.Value?.ToString() == "1" || reader.Value?.ToString().ToUpper() == "TRUE";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }

    public class FormatConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int formatNumber = 2;
            try
            {
                try
                {
                    var context = HttpHelper.HttpContext;
                    formatNumber = int.Parse(context.User.Claims.First(i => i.Type == "fm").Value);
                }
                catch
                {
                    formatNumber = 2;
                }
                var n = objectType.Name;
                if (reader.Value != null)
                {
                    if (String.IsNullOrEmpty(reader.Value.ToString()))
                    {
                        if (typeof(decimal?).IsAssignableFrom(objectType) || typeof(float?).IsAssignableFrom(objectType) || typeof(double?).IsAssignableFrom(objectType))
                        {
                            return null;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        if (typeof(decimal?).IsAssignableFrom(objectType) || typeof(decimal).IsAssignableFrom(objectType))
                        {
                            return Math.Round(decimal.Parse(reader.Value.ToString()), formatNumber);
                        }
                        else if (typeof(float?).IsAssignableFrom(objectType) || typeof(float).IsAssignableFrom(objectType))
                        {
                            return (float)Math.Round(float.Parse(reader.Value.ToString()), formatNumber);
                        }
                        else if (typeof(double?).IsAssignableFrom(objectType) || typeof(double).IsAssignableFrom(objectType))
                        {
                            return Math.Round(double.Parse(reader.Value.ToString()), formatNumber);
                        }
                        return reader.Value;
                    }
                }
                else
                {
                    return reader.Value;
                }

            }
            catch
            {
                return reader.Value;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
