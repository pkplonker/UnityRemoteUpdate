using System;
using System.Collections.Generic;
using System.Linq;
using RemoteUpdate;
using RemoteUpdate.Extensions;

namespace RemoteUpdateEditor
{
	public class EditorRemoteUpdateController
	{
		private List<RemoteUpdateEditorConnection> connections = new();
		private readonly List<IRemoteUpdateEditorHandler> handlers = new();

		public void CreateProcessors()
		{
			ClearHandlers();
			handlers.AddRange(TypeRepository.GetTypesFromInterface<IRemoteUpdateEditorHandler>()
				.ForEach(x => RTUDebug.Log($"Registering Editor Handlers: {x}"))
				.Select(x =>
					(IRemoteUpdateEditorHandler) Activator.CreateInstance(x, new object[] {this}))
				.ToList());
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
			return connections.Any(x => x.IPAddress.Equals(tempValue));
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
			var connection = new RemoteUpdateEditorConnection();
			connections.Add(connection);
			connection.Connect(ip, port, OnConnection(connection),
				() => OnDisconnect(connection));
		}

		private Action OnConnection( RemoteUpdateEditorConnection connection)
		{
			return null;
		}

		private void OnDisconnect(RemoteUpdateEditorConnection connection)
		{
			return;
		}
	}
}