using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RemoteUpdate.Extensions
{
	public static class GameObjectExtensions
	{
		public static List<GameObject> GetAllGameObjects()
		{
			var gameObjects = new List<GameObject>();

			var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

			foreach (var root in rootObjects)
			{
				gameObjects.Add(root);

				var children = root.GetComponentsInChildren<Transform>(true);
				foreach (var child in children)
				{
					if (child != root.transform)
					{
						gameObjects.Add(child.gameObject);
					}
				}
			}

			return gameObjects;
		}

		public static string Serialize(this GameObject gameObject, JsonSerializerSettings jsonSettings)
		{
			return JsonConvert.SerializeObject(gameObject, jsonSettings);
		}
	}
}