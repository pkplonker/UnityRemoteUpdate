using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemoteUpdate.Threading;
using UnityEngine;
using WebSocketSharp.Server;

namespace RemoteUpdate
{
	public class RuntimeUpdateController : MonoBehaviour
	{
		private CancellationTokenSource cancellationTokenSource;
		private WebSocketServer webSocketServer;
		private Thread serverThread;
		public TaskScheduler Schedular { get; private set; }
		public JsonSerializerSettings JsonSettings;

		public IntScriptableObject Port;
		private static readonly ConcurrentQueue<MainThreadTask<object>> mainThreadQueue = new();
		public static readonly string ServicePathPropertyName = "Path";
		public static readonly string WebSocketServiceAddMethodName = "AddWebSocketService";
		public static RuntimeUpdateController Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			JsonSettings = new JSONSettingsCreator().Create();
		}

		void Start()
		{

			cancellationTokenSource = new CancellationTokenSource();
			serverThread = new Thread(() =>
			{
				try
				{
					StartServer(cancellationTokenSource.Token);
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

		public Task<T> DispatchToMainThread<T>(Func<T> work)
		{
			var task = new MainThreadTask<object>
			{
				Work = () => work(),
				CompletionSource = new TaskCompletionSource<object>()
			};

			mainThreadQueue.Enqueue(task);
			return task.CompletionSource.Task.ContinueWith(t => (T) t.Result);
		}
		public Task DispatchToMainThread(Action work)
		{
			var task = new MainThreadTask<object>
			{
				Work = () =>
				{
					work();
					return null;
				},
				CompletionSource = new TaskCompletionSource<object>()
			};

			mainThreadQueue.Enqueue(task);
			return task.CompletionSource.Task;
		}


		private void Update()
		{
			//SendMessageToClient(PropertyChangeWebsocketBehaviour.Path, "Test Message");
			while (mainThreadQueue.TryDequeue(out var task))
			{
				try
				{
					
					var result = task.Work();
					task.CompletionSource.SetResult(result);
				}
				catch (Exception ex)
				{
					task.CompletionSource.SetException(ex);
				}
			}
		}

		public void SendMessageToClient(string path, string message)
		{
			if (webSocketServer == null || !webSocketServer.IsListening)
			{
				RTUDebug.LogWarning("WebSocket server is not running.");
				return;
			}

			if (!webSocketServer.WebSocketServices.TryGetServiceHost(path, out var serviceHost))
			{
				RTUDebug.LogWarning($"No WebSocket service registered at path: {path}");
				return;
			}

			serviceHost.Sessions.Broadcast(message);
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
				
				webSocketServer = new WebSocketServer(IPAddress.Any, Port.Value);
				var services = TypeRepository.GetFromBase<WebSocketBehavior>();
				var method = webSocketServer.GetType().GetMethods()
					.FirstOrDefault(x => x.Name == WebSocketServiceAddMethodName && x.GetParameters().Length == 1);
				foreach (var service in services)
				{
					try
					{
						var genericMethod = method.MakeGenericMethod(service);
						var path = service.GetProperty(ServicePathPropertyName).GetValue(service) as string;
						genericMethod.Invoke(webSocketServer, new object[] {path});
						count++;
					}
					catch (Exception e)
					{
						RTUDebug.LogError(
							$"Unable to add WebSocket service: {service.Name}, check that the Path is correct");
					}
				}

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