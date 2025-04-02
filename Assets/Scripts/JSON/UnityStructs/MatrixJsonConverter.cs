using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteUpdate;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	//modified from https://github.com/ianmacgillivray/Json-NET-for-Unity/blob/master/Source/Newtonsoft.Json/Converters/Matrix4x4Converter.cs
	[Preserve]
	[JSONCustomConverter(typeof(MatrixJsonConverter))]
	public class MatrixJsonConverter : JsonConverter<Matrix4x4>
	{
		public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
		{
			if (value == default)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("m00");
			writer.WriteValue(value.m00);

			writer.WritePropertyName("m01");
			writer.WriteValue(value.m01);

			writer.WritePropertyName("m02");
			writer.WriteValue(value.m02);

			writer.WritePropertyName("m03");
			writer.WriteValue(value.m03);

			writer.WritePropertyName("m10");
			writer.WriteValue(value.m10);

			writer.WritePropertyName("m11");
			writer.WriteValue(value.m11);

			writer.WritePropertyName("m12");
			writer.WriteValue(value.m12);

			writer.WritePropertyName("m13");
			writer.WriteValue(value.m13);

			writer.WritePropertyName("m20");
			writer.WriteValue(value.m20);

			writer.WritePropertyName("m21");
			writer.WriteValue(value.m21);

			writer.WritePropertyName("m22");
			writer.WriteValue(value.m22);

			writer.WritePropertyName("m23");
			writer.WriteValue(value.m23);

			writer.WritePropertyName("m30");
			writer.WriteValue(value.m30);

			writer.WritePropertyName("m31");
			writer.WriteValue(value.m31);

			writer.WritePropertyName("m32");
			writer.WriteValue(value.m32);

			writer.WritePropertyName("m33");
			writer.WriteValue(value.m33);
			writer.WriteEndObject();
		}

		public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return default;
			}

			if (reader.TokenType != JsonToken.StartObject)
			{
				throw new JsonSerializationException(
					$"Unexpected token {reader.TokenType} when deserializing matrix4x4.");
			}

			var obj = JObject.Load(reader);

			return new Matrix4x4
			{
				m00 = (float) obj["m00"],
				m01 = (float) obj["m01"],
				m02 = (float) obj["m02"],
				m03 = (float) obj["m03"],
				m20 = (float) obj["m20"],
				m21 = (float) obj["m21"],
				m22 = (float) obj["m22"],
				m23 = (float) obj["m23"],
				m30 = (float) obj["m30"],
				m31 = (float) obj["m31"],
				m32 = (float) obj["m32"],
				m33 = (float) obj["m33"]
			};
		}
	}
}