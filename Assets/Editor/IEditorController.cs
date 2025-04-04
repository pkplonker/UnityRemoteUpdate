using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace RemoteUpdateEditor
{
	public interface IEditorController
	{
		void OnMessage(string endpoint, string eData);
		Scene? GetScene();
		JsonSerializerSettings JsonSettings { get; }
	}
}