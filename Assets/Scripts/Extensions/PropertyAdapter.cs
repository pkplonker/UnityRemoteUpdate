using System;
using System.Reflection;
using UnityEngine;

namespace RemoteUpdate.Extensions
{
	public class PropertyAdapter : IMemberAdapter
	{
		private readonly PropertyInfo property;

		public PropertyAdapter(PropertyInfo property)
		{
			this.property = property;
		}

		public object GetValue(object component)
		{
			if (component is MeshFilter meshFilter)
			{
				if (property.Name.Equals("mesh", StringComparison.InvariantCultureIgnoreCase))
				{
					return meshFilter.sharedMesh;
				}
			}

			if (component.GetType().IsSubclassOf(typeof(Renderer)) && component is Renderer renderer)
			{
				if (property.Name.Equals("material", StringComparison.InvariantCultureIgnoreCase))
				{
					return renderer.sharedMaterial;
				}

				if (property.Name.Equals("materials", StringComparison.InvariantCultureIgnoreCase))
				{
					return renderer.sharedMaterials;
				}
			}

			return GetValueInternal(component);
		}

		public object GetValueInternal(object component)
		{
			try
			{
				return property?.GetValue(component);
			}
			catch
			{
				return null;
			}
		}

		public void SetValue(object component, object value)
		{
			property?.SetValue(component, value);
		}

		public Type MemberType => property?.PropertyType;
		public T GetCustomAttribute<T>() where T : Attribute => property?.GetCustomAttribute<T>();

		public string Name => property.Name;
		public MemberInfo GetMemberInfo() => property;
	}
}