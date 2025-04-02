using System;
using System.Collections.Generic;

namespace RemoteUpdate.SerializationDataObjects
{
	[Serializable]
	public class GameObjectDTO
	{
		public int InstanceId { get; set; }

		public string Name { get; set; }
		public string Tag { get; set; }
		public bool Active { get; set; }
		public string Layer { get; set; }

		public List<ComponentDTO> Components { get; set; }
		public List<GameObjectDTO> Children { get; set; }

		public bool IsReference { get; set; }
	}

}