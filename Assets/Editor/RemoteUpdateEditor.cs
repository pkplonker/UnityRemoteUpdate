using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using RemoteUpdate;
using UnityEditor;
using UnityEngine;

namespace RemoteUpdateEditor
{
	public class RTUEditorWindow : EditorWindow
	{
		private List<string> potentialConnections = new();
		private EditorRemoteUpdateController controller = new();
		private string gamePath = "S:\\Users\\pkplo\\OneDrive\\Desktop\\RemoteUpdate\\RemoteUpdate.exe";
		private IntScriptableObject PortSO;

		[MenuItem("Window/Remote Update &%r")]
		public static void ShowWindow()
		{
			GetWindow<RTUEditorWindow>("Remote Update Editor Client");
		}

		private void OnGUI()
		{
			PortSO = AssetDatabase.LoadAssetAtPath<IntScriptableObject>("Assets/ScriptableObjects/port.asset");

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Run Game"))
			{
				try
				{
					Process.Start(gamePath);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
				}
			}

			if (GUILayout.Button("Build"))
			{
				try
				{
					BuildPipeline.BuildPlayer(new string[] {"Assets/Scenes/RTUTest.unity"}, gamePath,
						BuildTarget.StandaloneWindows64, BuildOptions.None);

					//Process.Start(gamePath);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
				}
			}

			if (GUILayout.Button("Build + Run Game"))
			{
				try
				{
					BuildPipeline.BuildPlayer(new string[] {"Assets/Scenes/RTUTest.unity"}, gamePath,
						BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			gamePath = EditorGUILayout.TextField("Game Exe Path", gamePath);
			EditorGUILayout.LabelField("Port", GUILayout.MaxWidth(70));
			PortSO.Value = EditorGUILayout.IntField(string.Empty, PortSO.Value);
			EditorGUILayout.EndHorizontal();

			BigSeperator();
			var newConnections = new List<string>();

			foreach (var connection in potentialConnections)
			{
				EditorGUILayout.BeginHorizontal();
				try
				{
					var tempValue = EditorGUILayout.TextField("IP: ", connection);
					if (controller.HasConnection(tempValue))
					{
						if (GUILayout.Button("Disconnect", GUILayout.MaxWidth(75)))
						{
							controller.Disconnect(tempValue);
						}
					}
					else
					{
						if (GUILayout.Button("Connect", GUILayout.MaxWidth(75)) && IPAddress.TryParse(tempValue, out _))
						{
							controller.Connect(tempValue, PortSO.Value);
						}
					}

					if (GUILayout.Button("Remove", GUILayout.MaxWidth(75)))
					{
						controller.Disconnect(tempValue);
					}
					else
					{
						newConnections.Add(tempValue);
					}
				}
				catch (Exception e) { }

				EditorGUILayout.EndHorizontal();
			}

			potentialConnections = newConnections;

			if (GUILayout.Button("Add New Connection"))
			{
				potentialConnections.Add(string.Empty);
			}

			if (GUILayout.Button("Connect All"))
			{
				foreach (var pc in potentialConnections)
				{
					controller.Connect(pc, PortSO.Value);
				}
			}

			BigSeperator();

			SupportActions();
		}

		private static void BigSeperator()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		private void SupportActions()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Reload processors"))
			{
				controller.CreateProcessors();
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}