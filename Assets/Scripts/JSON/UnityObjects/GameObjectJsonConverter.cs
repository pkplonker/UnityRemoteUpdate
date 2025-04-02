using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RemoteUpdate;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
[JSONCustomConverter(typeof(GameObjectJsonConverter))]
public class GameObjectJsonConverter : JsonConverter
{
	private HashSet<int> visitedInstanceIDs = new HashSet<int>();

	public override bool CanConvert(Type objectType)
	{
		return typeof(GameObject).IsAssignableFrom(objectType)
		       || typeof(Component).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}

		if (value is GameObject go)
		{
			bool isRootCall = visitedInstanceIDs.Count == 0;
			WriteGameObject(writer, go, serializer);
			if (isRootCall) visitedInstanceIDs.Clear();
		}
		else if (value is Component comp)
		{
			JObject compData = SerializeComponent(comp, serializer);
			compData.WriteTo(writer);
		}
		else
		{
			JToken t = JToken.FromObject(value, serializer);
			t.WriteTo(writer);
		}
	}

	private void WriteGameObject(JsonWriter writer, GameObject go, JsonSerializer serializer)
	{
		int id = go.GetInstanceID();
		if (visitedInstanceIDs.Contains(id))
		{
			JObject referenceObj = new JObject
			{
				["instanceId"] = id,
				["isReference"] = true
			};
			referenceObj.WriteTo(writer);
			return;
		}

		visitedInstanceIDs.Add(id);

		JObject obj = new JObject();
		obj["instanceId"] = id;
		obj["name"] = go.name;
		obj["tag"] = go.tag;
		obj["layer"] = go.layer;
		obj["activeSelf"] = go.activeSelf;

		JArray componentsArray = new JArray();
		foreach (Component component in go.GetComponents<Component>())
		{
			JObject compData = SerializeComponent(component, serializer);
			componentsArray.Add(compData);
		}

		obj["components"] = componentsArray;

		// children?

		obj.WriteTo(writer);
	}

	private JObject SerializeComponent(Component comp, JsonSerializer serializer)
	{
		Type type = comp.GetType();
		JObject jObj = new JObject();

		jObj["type"] = type.Name;
		jObj["instanceId"] = comp.GetInstanceID();

		foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
		{
			if (field.IsDefined(typeof(NonSerializedAttribute), true)
			    || field.IsDefined(typeof(JsonIgnoreAttribute), true))
			{
				continue;
			}

			string fieldName = field.Name;
			object fieldValue = field.GetValue(comp);
			jObj[fieldName] = JToken.FromObject(SanitizeUnityReference(fieldValue, serializer));
		}

		foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x=>x.CanRead && x.CanWrite))
		{
			if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
				continue;

			string propName = prop.Name;
			if (propName == "gameObject" || propName == "transform" || propName == "attachedRigidbody")
				continue;
			if (propName == "name" || propName == "tag" || propName == "hideFlags")
				continue;

			object propValue;
			try
			{
				propValue = prop.GetValue(comp, null);
			}
			catch (Exception)
			{
				continue;
			}

			var val = SanitizeUnityReference(propValue, serializer);
			jObj[propName] = val != null
				? JToken.FromObject(val, serializer)
				: JValue.CreateNull();
		}

		return jObj;
	}

	private object SanitizeUnityReference(object obj, JsonSerializer serializer)
	{
		if (obj == null)
			return null;

		if (obj is UnityEngine.Object unityObj)
		{
			JObject refJson = new JObject();
			refJson["instanceId"] = unityObj.GetInstanceID();
			refJson["type"] = unityObj.GetType().Name;
			if (unityObj is GameObject && visitedInstanceIDs.Contains(unityObj.GetInstanceID()))
			{
				refJson["isReference"] = true;
			}

			return refJson;
		}

		if (obj is IEnumerable enumerable && !(obj is string))
		{
			JArray array = new JArray();
			foreach (var item in enumerable)
			{
				array.Add(JToken.FromObject(SanitizeUnityReference(item, serializer), serializer));
			}

			return array;
		}

		return obj;
	}

	public override bool CanRead => false;

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		=> throw new NotImplementedException("ReadJson is not implemented for GameObjectJsonConverter.");
}