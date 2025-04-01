using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RemoteUpdate
{
	public static class TypeRepository
	{
		private static IEnumerable<Type> types = null;

		public static IEnumerable<Type> GetTypes()
		{
			return types ??= AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes());
		}

		public static List<T> GetAttributes<T>() where T : Attribute =>
			GetTypes()
				.Where(type => type.GetCustomAttribute<T>() != null)
				.Select(type => type.GetCustomAttribute<T>())
				.ToList();

		public static List<Type> GetFromBase<T>() =>
			GetTypes()
				.Where(type => type.BaseType == typeof(T)).ToList();

		public static IEnumerable<Type> GetTypesFromInterface<T>() =>
			GetTypes()
				.Where(x => x.GetInterfaces().Contains(typeof(T))).ToList();
	}
}