using System;
using System.Threading.Tasks;
using RemoteUpdate.Threading;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RemoteUpdateEditor
{
	public class RemoteUpdateScene
	{
		private SceneStage customStage;
		private readonly TaskScheduler scheduler;
		public event Action<Scene?> SceneCreated;

		public RemoteUpdateScene(TaskScheduler scheduler)
		{
			this.scheduler = scheduler;
		}

		public bool IsVisible() => StageUtility.GetCurrentStage() == customStage;
		public Scene? GetScene() => customStage?.scene;

		public async void Close()
		{
			await ThreadingHelper.ActionOnSchedulerAsync(() =>
			{
				if (IsVisible())
				{
					StageUtility.GoToMainStage();
				}
			}, scheduler);
		}

		public void ShowScene()
		{
			customStage = ScriptableObject.CreateInstance<SceneStage>();
			StageUtility.GoToStage(customStage, true);
			var scene = customStage.scene;
			SceneCreated?.Invoke(scene);
		}
	}
}