using System.IO;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace KVD.Muninn
{
	public class MuninnSettings : ScriptableObject
	{
		[Header("Pix player loop")]
		public bool disableAutoPlayerLoopInstrumentation;

		[SerializeField] internal PlayerLoopSystemSetup[] playerLoopSystemSettings =
		{
			new PlayerLoopSystemSetup
			{
				name = MuninnPixPlayerLoop.FullFrameName,
				color = new Color(0, 0, 0),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = MuninnPixPlayerLoop.RenderingName,
				color = new Color(1f, 0.92156863f, 0.015686275f),
				shouldBeInstrumented = true,
			},

			new PlayerLoopSystemSetup
			{
				name = typeof(Initialization).Name,
				color = new Color(0.0f, 0.0f, 1f),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = typeof(EarlyUpdate).Name,
				color = new Color(0.2f, 0.7f, 0.69f),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = typeof(FixedUpdate).Name,
				color = new Color(0.0f, 1f, 0.0f, 1f),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = typeof(PreUpdate).Name,
				color = new Color(0.2f, 0.7f, 0.69f),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = typeof(Update).Name,
				color = new Color(0.0f, 1f, 0.0f, 1f),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = typeof(PreLateUpdate).Name,
				color = new Color(0.2f, 0.7f, 0.69f),
				shouldBeInstrumented = true,
			},
			new PlayerLoopSystemSetup
			{
				name = typeof(PostLateUpdate).Name,
				color = new Color(0.0f, 1f, 0.0f, 1f),
				shouldBeInstrumented = true,
			},
		};

		internal PlayerLoopSystemSetup GetPlayerLoopSystemSettings(string name)
		{
			for (var i = 0; i < playerLoopSystemSettings.Length; i++)
			{
				if (playerLoopSystemSettings[i].name == name)
				{
					return playerLoopSystemSettings[i];
				}
			}

			return PlayerLoopSystemSetup.Default;
		}

#region SINGLETON
		static MuninnSettings _instance;
		public static MuninnSettings Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = CreateInstance<MuninnSettings>();
					var dataPath = Path.Combine(Application.streamingAssetsPath, "MuninnSettings.asset");
					if (File.Exists(dataPath))
					{
						var json = File.ReadAllText(dataPath);
						JsonUtility.FromJsonOverwrite(json, _instance);
					}
				}

				return _instance;
			}
		}

#if UNITY_EDITOR
		public void OnValidate()
		{
			if (_instance != this)
			{
				return;
			}
			var dataPath = Path.Combine(Application.streamingAssetsPath, "MuninnSettings.asset");
			if (!Directory.Exists(Application.streamingAssetsPath))
			{
				Directory.CreateDirectory(Application.streamingAssetsPath);
			}
			var json = JsonUtility.ToJson(this, true);
			File.WriteAllText(dataPath, json);
		}
#endif
#endregion SINGLETON
	}
}
