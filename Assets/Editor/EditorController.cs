using System;
using System.Collections.Generic;
using System.Linq;
using RemoteUpdate;
using RemoteUpdate.Extensions;

namespace RemoteUpdateEditor
{
	public class EditorController : IEditorController
	{
		private List<RemoteUpdateEditorConnection> connections = new();
		private Dictionary<string, IEditorChangeHandler> handlers = new();

		public EditorController()
		{
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

		public bool HasConnection(string tempValue)
		{
			return connections.Any(x => x.IPAddress.Equals(tempValue) && x.IsConnected);
		}

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

		private void CloseScene() { }

		public void Connect(string ip, int port)
		{
			var connection = new RemoteUpdateEditorConnection(this);
			connections.Add(connection);
			connection.Connect(ip, port, OnConnection(connection),
				() => OnDisconnect(connection));
		}

		private Action OnConnection(RemoteUpdateEditorConnection connection)
		{
			return null;
		}

		private void OnDisconnect(RemoteUpdateEditorConnection connection)
		{
			return;
		}

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
	}
}