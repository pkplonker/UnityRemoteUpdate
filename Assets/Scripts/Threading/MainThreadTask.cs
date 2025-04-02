using System;
using System.Threading.Tasks;

namespace RemoteUpdate.Threading
{
	public class MainThreadTask<T>
	{
		public Func<T> Work;
		public TaskCompletionSource<T> CompletionSource;
	}
}