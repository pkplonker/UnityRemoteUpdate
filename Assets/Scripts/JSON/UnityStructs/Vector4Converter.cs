using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	[Preserve]
	[JSONCustomConverter(typeof(Vector4Converter))]
	public class Vector4Converter : JsonConverter<Vector4>
	{
		public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WritePropertyName("z");
			writer.WriteValue(value.z);
			writer.WritePropertyName("w");
			writer.WriteValue(value.w);
			writer.WriteEndObject();
		}

		public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var result = new Vector4();

			if (reader.TokenType != JsonToken.Null)
			{
				var jo = JObject.Load(reader);
				result.x = jo["x"].Value<float>();
				result.y = jo["y"].Value<float>();
				result.z = jo["z"].Value<float>();
				result.w = jo["w"].Value<float>();
			}

			return result;
		}
	}
}