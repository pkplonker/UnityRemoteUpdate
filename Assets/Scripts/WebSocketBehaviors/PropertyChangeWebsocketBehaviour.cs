using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RemoteUpdate
{
	public class PropertyChangeWebsocketBehaviour : WebSocketBehavior
	{
		public static string Path { get; } = "/Property";

		protected override void OnMessage(MessageEventArgs args)
		{
			try
			{
				var data = Encoding.UTF8.GetString(args.RawData);
				RTUDebug.Log($"Message received raw {data}");
				try
				{
					var messages = data.Split("@").Where(x => !string.IsNullOrEmpty(x));
					foreach (var message in messages)
					{
						RTUDebug.Log($"Message received: {message}");
					}
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Unable to handle message {e.Message}");
				}
			}
			catch (Exception e)
			{
				RTUDebug.LogWarning($"Unable to parse message {e.Message}");
			}
		}

		protected override void OnClose(CloseEventArgs args) => RTUDebug.Log($"Disconnected from editor {args.Reason}");

		protected override void OnOpen() => RTUDebug.Log("Connection established");
	}
}