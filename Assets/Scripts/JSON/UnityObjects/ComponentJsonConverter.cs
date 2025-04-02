// using Newtonsoft.Json;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using RemoteUpdate.SerializationDataObjects;
// using UnityEngine;
// using UnityEngine.Scripting;
//
// namespace RemoteUpdate
// {
// 	[Preserve]
// 	[JSONCustomConverter(typeof(ComponentJsonConverter))]
// 	public class ComponentJsonConverter : JsonConverter
// 	{
// 		public override bool CanRead => true;
// 		public override bool CanWrite => true;
// 		public override bool CanConvert(Type objectType)
// 		{
// 			return typeof(Component).IsAssignableFrom(objectType);
// 		}
//
// 		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
// 		{
// 			var dto = new ComponentDTO
// 			{
// 				Type = value.GetType().AssemblyQualifiedName,
// 				Properties = new Dictionary<string, object>()
// 			};
//
// 			var type = value.GetType();
//
// 			foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
// 			{
// 				if (!prop.CanRead || prop.GetIndexParameters().Length != 0)
// 					continue;
//
// 				if (prop.Name == "gameObject" || prop.Name == "transform")
// 					continue;
//
// 				try
// 				{
// 					var pValue = prop.GetValue(value);
// 					dto.Properties[prop.Name] = SanitizeUnityReference(pValue);
// 				}
// 				catch { }
// 			}
//
//
// 			foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
// 			{
// 				try
// 				{
// 					var fValue = field.GetValue(value);
// 					dto.Properties[field.Name] = SanitizeUnityReference(fValue);
// 				}
// 				catch { }
// 			}
//
// 			serializer.Serialize(writer, dto);
// 		}
//
// 		private object SanitizeUnityReference(object value)
// 		{
// 			if (value == null) return null;
//
// 			if (value is UnityEngine.Object unityObj)
// 			{
// 				return new Dictionary<string, object>
// 				{
// 					{"instanceId", unityObj.GetInstanceID()},
// 					{"type", unityObj.GetType().Name}
// 				};
// 			}
//
// 			if (value is IEnumerable<object> enumerable)
// 			{
// 				return enumerable.Select(SanitizeUnityReference).ToList();
// 			}
//
// 			return value;
// 		}
//
// 		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
// 		{
// 			var dto = serializer.Deserialize<ComponentDTO>(reader);
// 			if (dto == null || string.IsNullOrEmpty(dto.Type))
// 				return null;
//
// 			var type = Type.GetType(dto.Type);
// 			if (type == null || !typeof(Component).IsAssignableFrom(type))
// 				return null;
//
// 			var placeholder = new GameObject("ComponentPlaceholder");
// 			var component = placeholder.AddComponent(type);
// 			ApplyDtoToComponent(component, dto);
// 			UnityEngine.Object.DestroyImmediate(placeholder);
//
// 			return component;
// 		}
//
// 		public void ApplyDtoToComponent(Component component, ComponentDTO dto)
// 		{
// 			var type = component.GetType();
// 			foreach (var pair in dto.Properties)
// 			{
// 				var prop = type.GetProperty(pair.Key, BindingFlags.Public | BindingFlags.Instance);
// 				if (prop != null && prop.CanWrite && prop.GetIndexParameters().Length == 0)
// 				{
// 					try
// 					{
// 						prop.SetValue(component, Convert.ChangeType(pair.Value, prop.PropertyType));
// 					}
// 					catch { }
//
// 					continue;
// 				}
//
// 				var field = type.GetField(pair.Key, BindingFlags.Public | BindingFlags.Instance);
// 				if (field != null)
// 				{
// 					try
// 					{
// 						field.SetValue(component, Convert.ChangeType(pair.Value, field.FieldType));
// 					}
// 					catch { }
// 				}
// 			}
// 		}
// 	}
// }