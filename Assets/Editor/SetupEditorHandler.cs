using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteUpdate;
using UnityEngine;

namespace RemoteUpdateEditor
{
	public class SetupEditorHandler : IEditorChangeHandler
	{
		private readonly IEditorController controller;
		public string Path { get; } = SceneSetupWebSocketBehaviour.Path;

		public SetupEditorHandler(IEditorController controller)
		{
			this.controller = controller;
		}

		public void OnMessage(string data)
		{
			try
			{
				var array = JArray.Parse(data);
				var serializer = JsonSerializer.Create(RuntimeUpdateController.Instance.JsonSettings);
				var converter = new GameObjectJsonConverter();

				List<GameObject> gameObjects = EditorMainThreadDispatcher.Dispatch(() =>
				{
					var result = new List<GameObject>();

					foreach (var token in array)
					{
						var go =
							converter.ReadJson(token.CreateReader(), typeof(GameObject), null,
								serializer) as GameObject;
						if (go != null)
						{
							RTUDebug.Log(
								$"[SetupEditorHandler] Deserialized GameObject: {go.name} (ID: {go.GetInstanceID()})");
							result.Add(go);
						}
					}

					converter.ResolveDeferredReferences();

					return result;
				}).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				RTUDebug.LogError($"[SetupEditorHandler] Failed to deserialize GameObjects: {ex.Message}");
			}
		}
	}
}