using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	[Preserve]
	[JSONCustomConverter(typeof(Vector3ArrayConverter))]
	public class Vector3ArrayConverter : JsonConverter<UnityEngine.Vector3[]>
	{
		public override void WriteJson(JsonWriter writer, UnityEngine.Vector3[] value, JsonSerializer serializer)
		{
			writer.WriteStartArray();
			foreach (var vector in value)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("x");
				writer.WriteValue(vector.x);
				writer.WritePropertyName("y");
				writer.WriteValue(vector.y);
				writer.WritePropertyName("z");
				writer.WriteValue(vector.z);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
		}

		public override UnityEngine.Vector3[] ReadJson(JsonReader reader, Type objectType,
			UnityEngine.Vector3[] existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var array = JArray.Load(reader);
			return array.Select(item => new UnityEngine.Vector3(
				(float) item["x"],
				(float) item["y"],
				(float) item["z"]
			)).ToArray();
		}
	}
}