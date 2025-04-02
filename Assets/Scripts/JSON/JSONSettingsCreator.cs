using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace RemoteUpdate
{
	public class JSONSettingsCreator
	{
		private readonly List<JSONCustomConverterAttribute> customConverters;

		public JSONSettingsCreator()
		{
			customConverters = TypeRepository.GetAttributes<JSONCustomConverterAttribute>();
		}

		public JsonSerializerSettings Create()
		{
			var settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				Formatting = Formatting.None, MaxDepth = 5
			};

			foreach (var type in customConverters.Select(x => Type.GetType(x.Type)))
			{
				settings.Converters.Add((JsonConverter) Activator.CreateInstance(type));
			}

			return settings;
		}
	}
}