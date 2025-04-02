using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	// modified from https://github.com/ianmacgillivray/Json-NET-for-Unity/blob/master/Source/Newtonsoft.Json/Converters/VectorConverter.cs
	[Preserve]
	[JSONCustomConverter(typeof(Vector2Converter))]
	public class Vector2Converter : JsonConverter<Vector2>
	{
		public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WriteEndObject();
		}

		public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var result = new Vector2();

			if (reader.TokenType != JsonToken.Null)
			{
				var jo = JObject.Load(reader);
				result.x = jo["x"].Value<float>();
				result.y = jo["y"].Value<float>();
			}

			return result;
		}
	}
}