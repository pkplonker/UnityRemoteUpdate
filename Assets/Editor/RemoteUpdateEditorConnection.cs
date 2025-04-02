using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteUpdate;
using RemoteUpdate.Extensions;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RemoteUpdateEditor
{
	public class RemoteUpdateEditorConnection
	{
		private Dictionary<string, WebSocket> socketsDict = new();
		private readonly IEditorRemoteUpdateController controller;
		private IEnumerable<WebSocket> sockets => socketsDict.Values;

		public bool IsConnected => socketsDict?.Values.All(x => x.ReadyState == WebSocketState.Open) ?? false;
		public string IPAddress { get; private set; } = string.Empty;
		public int Port { get; private set; } = -1;

		public RemoteUpdateEditorConnection(IEditorRemoteUpdateController controller)
		{
			this.controller = controller;
		}
		public void Connect(string ipAddress, int port, Action completeCallback = null,
			Action disconnectCallback = null)
		{
			IPAddress = ipAddress;
			Port = port;
			var services = TypeRepository.GetFromBase<WebSocketBehavior>();
			foreach (var service in services)
			{
				try
				{
					var path = service.GetProperty(RuntimeUpdateController.ServicePathPropertyName)
						.GetValue(service) as string;
					socketsDict.Add(path, new WebSocket($"ws://{ipAddress}:{port}{path}"));
				}
				catch (Exception e)
				{
					RTUDebug.LogError($"Unable to create game connection {e.Message}");
					return;
				}
			}

			sockets.ForEach(x => x.OnOpen += (_, args) =>
			{
				RTUDebug.Log("Connected to the server");
				completeCallback?.Invoke();
			});
			sockets.ForEach(x => x.OnClose += (_, args) =>
			{

					RTUDebug.Log("Closed connection to game");


				disconnectCallback?.Invoke();
			});
			sockets.ForEach(x => x.OnMessage += OnMessage);
			sockets.ForEach(x => x.OnError += (_, args) =>
			{
				RTUDebug.Log($"Error connection to game: {args.Message}");
			});
			sockets.ForEach(x => x.Connect());
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
			RTUDebug.Log($"Message received: {e.Data}");
			
		}

		public void SendMessageToGame(string endpoint, string message)
		{
			if (!IsConnected)
			{
				RTUDebug.LogError("Not connected to the game server.");
				return;
			}

			if (socketsDict.TryGetValue(endpoint, out var socket))
			{
				socket.Send(Encoding.UTF8.GetBytes(message));
			}
		}

		public void Disconnect()
		{
			foreach (var socket in sockets)
			{
				socket.Close();
			}

			socketsDict.Clear();
		}
	}
}