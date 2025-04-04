using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteUpdate;
using UnityEngine;
using UnityEngine.SceneManagement;

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
				var converter = new GameObjectJsonConverter();
				var scene = controller.GetScene();
				if (scene != null)
				{
					EditorMainThreadDispatcher.Dispatch(() =>
					{
						var serializer = JsonSerializer.Create(controller.JsonSettings);

						var gameObjects = new List<GameObject>();

						foreach (var token in array)
						{
							var go =
								converter.ReadJson(token.CreateReader(), typeof(GameObject), null,
									serializer) as GameObject;
							if (go != null)
							{
								RTUDebug.Log(
									$"[SetupEditorHandler] Deserialized GameObject: {go.name} (ID: {go.GetInstanceID()})");
								gameObjects.Add(go);
							}
						}

						converter.ResolveDeferredReferences();

						foreach (var go in gameObjects.Where(x => x.transform.parent == null))
						{
							SceneManager.MoveGameObjectToScene(go, controller.GetScene().Value);
						}
					}).GetAwaiter().GetResult();
				}
			}
			catch (Exception ex)
			{
				RTUDebug.LogError($"[SetupEditorHandler] Failed to deserialize GameObjects: {ex.Message}");
			}
		}
	}
}