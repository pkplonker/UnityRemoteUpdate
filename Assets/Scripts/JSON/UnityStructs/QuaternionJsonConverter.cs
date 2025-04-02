using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteUpdate;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	[Preserve]
	[JSONCustomConverter(typeof(QuaternionConverter))]
	public class QuaternionConverter : JsonConverter<Quaternion>
	{
		public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
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

		public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var result = new Quaternion();

			if (reader.TokenType != JsonToken.Null)
			{
				var obj = JObject.Load(reader);
				result.x = obj["x"].Value<float>();
				result.y = obj["y"].Value<float>();
				result.z = obj["z"].Value<float>();
				result.w = obj["w"].Value<float>();
			}

			return result;
		}
	}
}