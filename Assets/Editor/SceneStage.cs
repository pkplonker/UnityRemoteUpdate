using UnityEditor.SceneManagement;
using UnityEngine;

namespace RemoteUpdateEditor
{
	public class SceneStage : PreviewSceneStage
	{
		protected override GUIContent CreateHeaderContent() => new GUIContent("Remote Update Preview Scene");

		protected override void OnDisable()
		{
			base.OnDisable();

			if (scene.IsValid())
			{
				EditorSceneManager.CloseScene(scene, true);
			}
		}
	}
}