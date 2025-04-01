using System;
using System.Text;
using RemoteUpdate;
using WebSocketSharp;

namespace RemoteUpdateEditor
{
	public class RemoteUpdateEditorConnection
	{
		private WebSocket socket;
		public bool IsConnected => socket?.ReadyState == WebSocketState.Open;
		public string IPAddress { get; private set; } = string.Empty;
		public int Port { get; private set; } = -1;

		public void Connect(string ipAddress, int port, Action completeCallback = null,
			Action disconnectCallback = null)
		{
			IPAddress = ipAddress;
			Port = port;
			string behaviour = "Property";
			try
			{
				socket = new WebSocket($"ws://{ipAddress}:{port}/{behaviour}");
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Unable to create game connection {e.Message}");
				return;
			}

			socket.OnOpen += (_, args) =>
			{
				RTUDebug.Log("Connected to the server");
				completeCallback?.Invoke();
			};
			socket.OnClose += (_, args) =>
			{
				if (args.WasClean)
				{
					RTUDebug.Log("Closed connection to game");
				}
				else
				{
					RTUDebug.LogWarning($"Unable to establish connection to game. Reason: {args.Reason}");
				}

				disconnectCallback?.Invoke();
			};
			socket.OnMessage += (_, args) => RTUDebug.Log($"Message received: {args.Data}");
			socket.OnError += (_, args) => { RTUDebug.Log($"Error connection to game: {args.Message}"); };

			socket.Connect();
		}

		public void SendMessageToGame(string message)
		{
			if (!IsConnected)
			{
				RTUDebug.LogError("Not connected to the game server.");
				return;
			}

			socket.Send(Encoding.UTF8.GetBytes(message));
		}

		public void Disconnect()
		{
			if (socket != null)
			{
				socket.Close();
			}
		}
	}
}