using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using RemoteUpdate.Threading;
using UnityEditor;

namespace RemoteUpdateEditor
{
	[InitializeOnLoad]
	public static class EditorMainThreadDispatcher
	{
		private static readonly ConcurrentQueue<MainThreadTask<object>> queue = new();

		static EditorMainThreadDispatcher()
		{
			EditorApplication.update += DrainQueue;
		}

		public static Task<T> Dispatch<T>(Func<T> work)
		{
			var task = new MainThreadTask<object>
			{
				Work = () => work(),
				CompletionSource = new TaskCompletionSource<object>()
			};

			queue.Enqueue(task);
			return task.CompletionSource.Task.ContinueWith(t => (T) t.Result);
		}

		public static Task Dispatch(Action work)
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

			queue.Enqueue(task);
			return task.CompletionSource.Task;
		}

		private static void DrainQueue()
		{
			while (queue.TryDequeue(out var task))
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
	}
}