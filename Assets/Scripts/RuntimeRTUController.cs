using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp.Server;

namespace RemoteUpdate
{
	public class RuntimeRTUController : MonoBehaviour
	{
		private CancellationTokenSource cancellationTokenSource;
		private WebSocketServer webSocketServer;
		private Thread serverThread;
		public TaskScheduler Schedular { get; private set; }
		public IntScriptableObject Port;
		private readonly string servicePathPropertyName = "Path";
		private string WebSocketServiceAddMethodName = "AddWebSocketService";

		private void Awake()
		{
			Debug.Log("awake");

			gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		}

		void Start()
		{
			Debug.Log("start");

			cancellationTokenSource = new CancellationTokenSource();
			serverThread = new Thread(() =>
			{
				try
				{
					StartServer(cancellationTokenSource.Token);
					Debug.Log("Server Created");
				}
				catch
				{
					webSocketServer = null;
					serverThread = null;
				}
			});
			serverThread.Start();
			Debug.Log("Server started");

			Schedular = TaskScheduler.FromCurrentSynchronizationContext();
		}

		private void StartServer(CancellationToken token)
		{
			int count = 0;

			try
			{
				RTUDebug.Log("Starting Server");
				if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
				{
					var localIP = GetLocalIP();
				}
				else
				{
					RTUDebug.LogWarning("Not connected to network - unable to support RTU");
					return;
				}

				Debug.Log("server ini");

				webSocketServer = new WebSocketServer(IPAddress.Any, Port.Value);
				var services = TypeRepository.GetFromBase<WebSocketBehavior>();
				var method = webSocketServer.GetType().GetMethods()
					.FirstOrDefault(x => x.Name == WebSocketServiceAddMethodName && x.GetParameters().Length == 1);
				foreach (var service in services)
				{
					try
					{
						var genericMethod = method.MakeGenericMethod(service);
						var path = service.GetProperty(servicePathPropertyName).GetValue(service) as string;
						genericMethod.Invoke(webSocketServer, new object[] {path});
						count++;
					}
					catch (Exception e)
					{
						RTUDebug.LogError(
							$"Unable to add WebSocket service: {service.Name}, check that the Path is correct");
					}
				}
				Debug.Log("webSocketServer.Start()");
				webSocketServer.Start();
			}
			catch (Exception e)
			{
				RTUDebug.Log($"err {e}");
				return;
			}

			RTUDebug.Log($"Server started with {count} services. Waiting for connections...");

			while (true)
			{
				token.ThrowIfCancellationRequested();
			}
		}

		private static string GetLocalIP()
		{
			try
			{
				using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
				socket.Connect("8.8.8.8", 65530);
				var localIP = (socket.LocalEndPoint as IPEndPoint).Address.ToString();
				RTUDebug.Log(localIP);
				return localIP;
			}
			catch (Exception e)
			{
				RTUDebug.LogException(e);
				return string.Empty;
			}
		}

		void OnApplicationQuit()
		{
			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
			webSocketServer?.Stop();
			RTUDebug.Log("WebSocket server shutting down.");
		}
	}
}