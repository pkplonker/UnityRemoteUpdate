using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using RemoteUpdate;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
[JSONCustomConverter(typeof(GameObjectJsonConverter))]
public class GameObjectJsonConverter : JsonConverter
{
	private readonly Dictionary<int, UnityEngine.Object> deserializedObjects = new();
	private readonly List<(Transform child, int parentId)> pendingParents = new();
	private readonly List<DeferredReference> deferredReferences = new();
	private HashSet<int> visitedInstanceIDs = new HashSet<int>();

	private struct DeferredReference
	{
		public UnityEngine.Object TargetObject;
		public MemberInfo Member;
		public JToken ValueToken;
	}

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
			if (compData != null)
			{
				compData.WriteTo(writer);
			}
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
		var t = go.transform;
		JObject transformData = new JObject
		{
			["position"] = JObject.FromObject(t.localPosition, serializer),
			["rotation"] = JObject.FromObject(t.localEulerAngles, serializer),
			["scale"] = JObject.FromObject(t.localScale, serializer)
		};

		obj["transform"] = transformData;
		JArray componentsArray = new JArray();
		foreach (Component component in go.GetComponents<Component>())
		{
			JObject compData = SerializeComponent(component, serializer);
			if (compData != null)
			{
				componentsArray.Add(compData);
			}
		}

		obj["components"] = componentsArray;

		if (t.parent != null)
		{
			obj["parentId"] = t.parent.gameObject.GetInstanceID();
		}

		obj.WriteTo(writer);
	}

	[CanBeNull]
	private JObject SerializeComponent(Component comp, JsonSerializer serializer)
	{
		Type type = comp.GetType();
		JObject jObj = new JObject();
		if (type == typeof(Transform)) return null;
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

		foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			         .Where(x => x.CanRead && x.CanWrite))
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
	{
		var jo = JObject.Load(reader);

		if (jo.TryGetValue("isReference", out var isRef) && isRef.Value<bool>())
		{
			int refId = jo["instanceId"]?.Value<int>() ?? -1;
			if (refId != -1 && deserializedObjects.TryGetValue(refId, out var existing))
				return existing;
			return null;
		}

		var go = new GameObject(jo["name"]?.ToString() ?? "Unnamed");
		if (jo.TryGetValue("tag", out var tagToken)) go.tag = tagToken.ToString();
		if (jo.TryGetValue("layer", out var layerToken)) go.layer = layerToken.Value<int>();
		if (jo.TryGetValue("activeSelf", out var activeToken)) go.SetActive(activeToken.Value<bool>());

		int id = jo["instanceId"]?.Value<int>() ?? go.GetInstanceID();
		deserializedObjects[id] = go;

		if (jo.TryGetValue("parentId", out var parentToken))
		{
			pendingParents.Add((go.transform, parentToken.Value<int>()));
		}

		if (jo.TryGetValue("transform", out var transformToken) && transformToken is JObject transformObj)
		{
			var t = go.transform;

			if (transformObj.TryGetValue("position", out var pos))
				t.localPosition = pos.ToObject<Vector3>(serializer);

			if (transformObj.TryGetValue("rotation", out var rot))
				t.localEulerAngles = rot.ToObject<Vector3>(serializer);

			if (transformObj.TryGetValue("scale", out var scale))
				t.localScale = scale.ToObject<Vector3>(serializer);
		}

		if (jo.TryGetValue("components", out var compsToken) && compsToken is JArray components)
		{
			foreach (var compToken in components)
			{
				var comp = DeserializeComponent((JObject) compToken, go);
				if (comp != null)
					deserializedObjects[comp.GetInstanceID()] = comp;
			}
		}

		return go;
	}

	private object ConvertToken(Type targetType, JToken token)
	{
		if (token.Type == JTokenType.Object && token["instanceId"] != null)
		{
			int refId = token["instanceId"].Value<int>();

			if (deserializedObjects.TryGetValue(refId, out var referenced))
			{
				return referenced;
			}

			return null;
		}

		return token.ToObject(targetType);
	}

	private Component DeserializeComponent(JObject compObj, GameObject owner)
	{
		if (!compObj.TryGetValue("type", out var typeToken)) return null;
		var typeName = typeToken.ToString();
		var type = TypeRepository.GetTypes()
			.FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);

		if (type == null || !typeof(Component).IsAssignableFrom(type)) return null;

		var comp = owner.AddComponent(type);

		foreach (var prop in compObj.Properties())
		{
			if (prop.Name == "type" || prop.Name == "instanceId") continue;

			var member = type.GetMember(prop.Name, BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();

			if (member is FieldInfo field)
			{
				try
				{
					var value = ConvertToken(field.FieldType, prop.Value);

					if (!field.IsStatic)
					{
						field.SetValue(comp, value);
					}
				}
				catch (Exception ex)
				{
					RTUDebug.LogWarning($"Failed to set field '{prop.Name}' on {type.Name}: {ex.Message}");
				}
			}
			else if (member is PropertyInfo property && property.CanWrite)
			{
				try
				{
					var value = ConvertToken(property.PropertyType, prop.Value);

					if (!property.GetMethod.IsStatic)
					{
						property.SetValue(comp, value);
					}
				}
				catch (Exception ex)
				{
					RTUDebug.LogWarning($"Failed to set property '{prop.Name}' on {type.Name}: {ex.Message}");
				}
			}
		}

		return comp;
	}

	private bool IsReferenceToken(JToken token)
	{
		return token.Type == JTokenType.Object && token["instanceId"] != null;
	}

	public void ResolveDeferredReferences()
	{
		foreach (var (child, parentId) in pendingParents)
		{
			if (deserializedObjects.TryGetValue(parentId, out var parentObj) && parentObj is GameObject parentGo)
			{
				child.SetParent(parentGo.transform, false);
			}
		}

		pendingParents.Clear();

		foreach (var def in deferredReferences)
		{
			if (def.ValueToken["instanceId"] is JToken idToken)
			{
				int refId = idToken.Value<int>();
				if (deserializedObjects.TryGetValue(refId, out var targetRef))
				{
					if (def.Member is FieldInfo field)
						field.SetValue(def.TargetObject, targetRef);
					else if (def.Member is PropertyInfo prop)
						prop.SetValue(def.TargetObject, targetRef);
				}
			}
		}

		deferredReferences.Clear();
	}
}