using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteUpdate.SerializationDataObjects;
using UnityEngine;
using UnityEngine.Scripting;

namespace RemoteUpdate
{
	[Preserve]
	[JSONCustomConverter(typeof(GameObjectJsonConverter))]
	public class GameObjectJsonConverter : JsonConverter<GameObject>
	{
		private readonly HashSet<int> serializedGameObjects = new();

		public override void WriteJson(JsonWriter writer, GameObject value, JsonSerializer serializer)
		{
			var dto = ToDto(value, serializer);
			serializer.Serialize(writer, dto);
		}

		public override GameObject ReadJson(JsonReader reader, Type objectType, GameObject existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var dto = serializer.Deserialize<GameObjectDTO>(reader);
			return CreateGameObjectFromDto(dto);
		}

		private GameObjectDTO ToDto(GameObject go, JsonSerializer serializer)
		{
			int id = go.GetInstanceID();

			if (serializedGameObjects.Contains(id))
			{
				return new GameObjectDTO
				{
					InstanceId = id,
					IsReference = true
				};
			}

			serializedGameObjects.Add(id);

			return new GameObjectDTO
			{
				InstanceId = id,
				Name = go.name,
				Tag = go.tag,
				Active = go.activeSelf,
				Layer = LayerMask.LayerToName(go.layer),
				Components = go.GetComponents<Component>()
					.Where(c => c != null && !(c is Transform))
					.Select(c => SerializeComponent(c, serializer))
					.ToList(),
				Children = go.transform.Cast<Transform>()
					.Select(child => ToDto(child.gameObject, serializer))
					.ToList()
			};
		}

		private ComponentDTO SerializeComponent(Component component, JsonSerializer serializer)
		{
			var json = JObject.FromObject(component, serializer);
			ReplaceUnityObjectReferences(json);
			return json.ToObject<ComponentDTO>(serializer);
		}

		private void ReplaceUnityObjectReferences(JToken token)
		{
			if (token.Type == JTokenType.Object)
			{
				var obj = (JObject) token;
				var unityRef = obj.ToObject<UnityEngine.Object>();

				if (unityRef != null)
				{
					obj.RemoveAll();
					obj["instanceId"] = unityRef.GetInstanceID();
					return;
				}

				foreach (var property in obj.Properties().ToList())
				{
					ReplaceUnityObjectReferences(property.Value);
				}
			}
			else if (token.Type == JTokenType.Array)
			{
				var array = (JArray) token;
				for (int i = 0; i < array.Count; i++)
				{
					ReplaceUnityObjectReferences(array[i]);
				}
			}
		}

		private GameObject CreateGameObjectFromDto(GameObjectDTO dto)
		{
			var go = new GameObject(dto.Name)
			{
				tag = dto.Tag,
				layer = LayerMask.NameToLayer(dto.Layer)
			};
			go.SetActive(dto.Active);

			foreach (var compDto in dto.Components)
			{
				var type = Type.GetType(compDto.Type);
				if (type == null || !typeof(Component).IsAssignableFrom(type))
					continue;

				try
				{
					var comp = go.AddComponent(type);
					new ComponentJsonConverter().ApplyDtoToComponent(comp, compDto);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Failed to add/restore component {compDto.Type}: {e.Message}");
				}
			}

			foreach (var childDto in dto.Children)
			{
				var child = CreateGameObjectFromDto(childDto);
				child.transform.SetParent(go.transform);
			}

			return go;
		}
	}
}