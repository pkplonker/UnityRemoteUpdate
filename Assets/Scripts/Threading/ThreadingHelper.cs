using System;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteUpdate.Threading
{
	public class ThreadingHelper
	{
		public static T FunctionOnScheduler<T>(Func<T> action, TaskScheduler scheduler) =>
			Task.Factory.StartNew(action, new CancellationToken(),
				new TaskCreationOptions(), scheduler).Result;

		public static void ActionOnScheduler(Action action, TaskScheduler scheduler, int timeout = -1) =>
			Task.Factory.StartNew(action, new CancellationToken(),
				new TaskCreationOptions(), scheduler).Wait(timeout);

		public static async Task ActionOnSchedulerAsync(Action action, TaskScheduler scheduler) =>
			await Task.Factory.StartNew(action, new CancellationToken(),
				new TaskCreationOptions(), scheduler);
	}
}