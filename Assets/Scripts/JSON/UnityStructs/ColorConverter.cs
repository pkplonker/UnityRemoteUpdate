using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	[Preserve]
	[JSONCustomConverter(typeof(ColorConverter))]
	public class ColorConverter : JsonConverter<Color>
	{
		public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("r");
			writer.WriteValue(value.r);
			writer.WritePropertyName("g");
			writer.WriteValue(value.g);
			writer.WritePropertyName("b");
			writer.WriteValue(value.b);
			writer.WritePropertyName("a");
			writer.WriteValue(value.a);
			writer.WriteEndObject();
		}

		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var result = new Color();

			if (reader.TokenType != JsonToken.Null)
			{
				var jo = JObject.Load(reader);
				result.r = jo["r"]?.Value<float>() ?? 0f;
				result.g = jo["g"]?.Value<float>() ?? 0f;
				result.b = jo["b"]?.Value<float>() ?? 0f;
				result.a = jo["a"]?.Value<float>() ?? 1f;
			}

			return result;
		}
	}
}