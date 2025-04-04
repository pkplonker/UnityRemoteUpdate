using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteUpdate;
using RemoteUpdate.Threading;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace RemoteUpdateEditor
{
	public class EditorController : IEditorController
	{
		private List<RemoteUpdateEditorConnection> connections = new();
		private Dictionary<string, IEditorChangeHandler> handlers = new();
		public event Action<RemoteUpdateScene> SceneChanged;

		private RemoteUpdateScene scene;
		private readonly TaskScheduler scheduler;
		private bool setup;
		public JsonSerializerSettings JsonSettings { get; } = new JSONSettingsCreator().Create();
		

		public RemoteUpdateScene Scene
		{
			get => scene;
			private set
			{
				scene = value;
				SceneChanged?.Invoke(scene);
			}
		}

		public EditorController()
		{
			AssemblyReloadEvents.beforeAssemblyReload += () => Scene?.Close();
			scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			CreateProcessors();
		}

		public void CreateProcessors()
		{
			ClearHandlers();
			handlers = TypeRepository.GetTypesFromInterface<IEditorChangeHandler>()
				.Select(type =>
				{
					var instance = (IEditorChangeHandler) Activator.CreateInstance(type, new object[] {this});
					RTUDebug.Log($"Registering Editor Handler: {type} with path: {instance.Path}");
					return new KeyValuePair<string, IEditorChangeHandler>(instance.Path, instance);
				})
				.ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		private void ClearHandlers()
		{
			foreach (var handler in handlers.OfType<IDisposable>())
			{
				handler.Dispose();
			}

			handlers.Clear();
		}

		private void CloseScene() => Scene?.Close();

		public void ShowScene() => Scene?.ShowScene();

		private void NewScene()
		{
			Scene = new RemoteUpdateScene(scheduler);
			Scene.ShowScene();
		}

		public bool HasConnection(string ip) =>
			connections.Any(x => x.IPAddress.Equals(ip) && x.IsConnected);

		public void Disconnect(string ip)
		{
			var connection = connections.FirstOrDefault(x => x.IPAddress.Equals(ip));
			connections.Remove(connection);
			connection?.Disconnect();

			if (!connections.Any())
			{
				CloseScene();
			}
		}

		public void DisconnectAll()
		{
			CloseScene();
			connections.ForEach(x => Disconnect(x.IPAddress));
		}

		public void Connect(string ip, int port)
		{
			var connection = new RemoteUpdateEditorConnection(this);
			connections.Add(connection);
			connection.Connect(ip, port, () => OnConnection(connection),
				() => OnDisconnect(connection));
		}

		private async void OnConnection(RemoteUpdateEditorConnection connection)
		{
			if (!setup)
			{
				await ThreadingHelper.ActionOnSchedulerAsync(() => Selection.objects = null, scheduler);
				NewScene();
			}
		}

		private void OnDisconnect(RemoteUpdateEditorConnection connection) => setup = false;

		public void OnMessage(string endpoint, string data)
		{
			if (handlers.TryGetValue(endpoint, out var handler))
			{
				handler.OnMessage(data);
			}
			else
			{
				RTUDebug.LogError($"Missing handler for endpoint {endpoint}");
			}
		}

		public Scene? GetScene() => scene.GetScene();
	}
}