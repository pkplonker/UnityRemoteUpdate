using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	[Preserve]
	[JSONCustomConverter(typeof(Vector3Converter))]
	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WritePropertyName("z");
			writer.WriteValue(value.z);
			writer.WriteEndObject();
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var result = new Vector3();

			if (reader.TokenType != JsonToken.Null)
			{
				var jo = JObject.Load(reader);
				result.x = jo["x"].Value<float>();
				result.y = jo["y"].Value<float>();
				result.z = jo["z"].Value<float>();
			}

			return result;
		}
	}
}