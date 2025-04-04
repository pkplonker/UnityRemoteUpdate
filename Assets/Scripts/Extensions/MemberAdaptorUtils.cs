using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RemoteUpdate.Extensions
{
	public static class MemberAdaptorUtils
	{
		private static Dictionary<Type, List<IMemberAdapter>> memberAdaptors = new();
		private static Dictionary<Type, Dictionary<string, IMemberAdapter>> memberAdaptorsDict = new();

		public static IMemberAdapter GetMemberAdapter(Type type, string fieldName)
		{
			var members = GetMemberAdapters(type);
			var memberInfo = members.FirstOrDefault(x =>
				string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));
			IMemberAdapter member = null;
			if (memberInfo == null)
			{
				memberInfo = members.FirstOrDefault(x =>
					string.Equals(x.Name, fieldName.Insert(0, "m_"), StringComparison.InvariantCultureIgnoreCase));
			}

			if (memberInfo == null)
			{
				throw new Exception("Member not found");
			}

			return memberInfo;
		}

		private static MemberInfo[] GetMemberInfo(Type type) =>
			type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x=>x.CanWrite)
				.Concat(type
					.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.OfType<MemberInfo>()).ToArray();
		
		public static List<IMemberAdapter> GetMemberAdapters(Type type)
		{
			if (memberAdaptors.ContainsKey(type))
			{
				return memberAdaptors[type];
			}

			
			var newMemberAdaptors = GetMemberInfo(type).Select(CreateMemberAdapter).ToList();
			memberAdaptors.Add(type, newMemberAdaptors);
			return newMemberAdaptors;
		}

		private static IMemberAdapter CreateMemberAdapter(MemberInfo memberInfo)
		{
			return memberInfo switch
			{
				PropertyInfo prop => new PropertyAdapter(prop),
				FieldInfo field => new FieldAdapter(field),
				_ => throw new InvalidOperationException("Unsupported member type.")
			};
		}

		public static Dictionary<string, IMemberAdapter> GetMemberAdaptersAsDict(Type type)
		{
			if (memberAdaptorsDict.ContainsKey(type))
			{
				return memberAdaptorsDict[type];
			}

			var newMemberAdaptors = GetMemberAdapters(type)
				.ToDictionary(a => a.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
			memberAdaptorsDict.Add(type, newMemberAdaptors);
			return newMemberAdaptors;
		}
	}
}