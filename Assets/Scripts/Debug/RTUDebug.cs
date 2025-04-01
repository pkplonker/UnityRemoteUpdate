using System;
using UnityEngine;

namespace RemoteUpdate
{
	public static class RTUDebug
	{
#if DEBUG
		private const bool IsDebugEnabled = true;
#else
		private const bool IsDebugEnabled = false;
#endif
		public const string PREMESSAGE = "RTU: ";

		public static void Log(object message)
		{
			if (IsDebugEnabled)
			{
				Debug.Log(PREMESSAGE+message);
			}
		}

		public static void LogWarning(object message)
		{
			if (IsDebugEnabled)
			{
				Debug.LogWarning(PREMESSAGE+message);
			}
		}

		public static void LogError(object message)
		{
			if (IsDebugEnabled)
			{
				Debug.LogError(PREMESSAGE+message);
			}
		}

		public static void LogException(Exception exception)
		{
			if (IsDebugEnabled && exception != null)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}
}