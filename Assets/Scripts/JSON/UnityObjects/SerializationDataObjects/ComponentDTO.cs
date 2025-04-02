using System;
using System.Collections.Generic;

namespace RemoteUpdate.SerializationDataObjects
{
	[Serializable]
	public class ComponentDTO
	{
		public string Type;
		public Dictionary<string, object> Properties;
	}
}