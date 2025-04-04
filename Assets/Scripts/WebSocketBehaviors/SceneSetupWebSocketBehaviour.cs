using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using RemoteUpdate.Extensions;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RemoteUpdate
{
	public class SceneSetupWebSocketBehaviour : WebSocketBehavior
	{
		public static string Path { get; } = "/Setup";
		protected override void OnClose(CloseEventArgs args) => RTUDebug.Log($"Disconnected from editor {args.Reason}");

		protected override void OnOpen()
		{
			RTUDebug.Log("Sending Scene data");

			var serializedObjects = RuntimeUpdateController.Instance
				.DispatchToMainThread(() =>
				{
					var gos = GameObjectExtensions.GetAllGameObjects();
					var messages = new List<string>();
					foreach (var go in gos.Where(x=>(x.hideFlags & HideFlags.HideInInspector)==0))
					{
						var message = go.Serialize(RuntimeUpdateController.Instance.JsonSettings);
						messages.Add(message);
					}

					return messages;
				}).GetAwaiter().GetResult();

			var combined = "[" + string.Join(",", serializedObjects) + "]";
			RuntimeUpdateController.Instance.SendMessageToClient(Path, combined);
		}
	}
}