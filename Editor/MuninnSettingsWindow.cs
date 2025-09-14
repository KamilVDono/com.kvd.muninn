using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace KVD.Muninn.Editor
{
	public class MuninnSettingsWindow : EditorWindow
	{
		static readonly string[] DefaultEnabledSystems = new string[]
		{
			MuninnPixPlayerLoop.FullFrameName, MuninnPixPlayerLoop.RenderingName, typeof(Initialization).Name, typeof(EarlyUpdate).Name,
			typeof(FixedUpdate).Name, typeof(PreUpdate).Name, typeof(Update).Name, typeof(PreLateUpdate).Name, typeof(PostLateUpdate).Name,
		};

		Vector2 _instrumentationScroll;
		GUIStyle _titleStyle;

		GUIStyle TitleStyle
		{
			get
			{
				if (_titleStyle == null)
				{
					_titleStyle = new GUIStyle(EditorStyles.boldLabel)
					{
						alignment = TextAnchor.MiddleCenter,
						fontStyle = FontStyle.Bold,
						fontSize = 20,
					};
				}

				return _titleStyle;
			}
		}

		[MenuItem("Window/Analysis/Muninn Settings", false, 11)]
		static void ShowWindow()
		{
			var window = GetWindow<MuninnSettingsWindow>();
			window.titleContent = new GUIContent("Muninn settings");
			window.Show();
		}

		void OnGUI()
		{
			var settings = MuninnSettings.Instance;
			var serializedSettings = new SerializedObject(settings);

			EditorGUILayout.LabelField("Muninn Settings", TitleStyle);

			serializedSettings.Update();

			var prop = serializedSettings.GetIterator();

			EditorGUI.BeginChangeCheck();

			for (var expanded = true; prop.NextVisible(expanded); expanded = false)
			{
				if (prop.name == "m_Script")
				{
					continue;
				}
				if (prop.name == nameof(MuninnSettings.playerLoopSystemSettings))
				{
					continue;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(prop, true);
				if (EditorGUI.EndChangeCheck())
				{
				}
			}

			var changed = EditorGUI.EndChangeCheck();

			serializedSettings.ApplyModifiedProperties();

			if (changed)
			{
				settings.OnValidate();
			}

			DrawDefines();

			DrawInstrumentationSettings();
		}

		void DrawDefines()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Defines", EditorStyles.boldLabel);

			++EditorGUI.indentLevel;

			// MUNINN_ENABLED
			var muninnEnabled =
#if MUNINN_ENABLED
					true
#else
					false
#endif
				;
			DrawDefine(muninnEnabled, "MUNINN_ENABLED");

			// PIX_DISABLED
			var pixDisabled =
#if PIX_DISABLED
					true
#else
					false
#endif
				;
			DrawDefine(pixDisabled, "PIX_DISABLED");

			// SUPERLUMINAL_DISABLED
			var superluminalDisabled =
#if SUPERLUMINAL_DISABLED
					true
#else
					false
#endif
				;
			DrawDefine(superluminalDisabled, "SUPERLUMINAL_DISABLED");

			--EditorGUI.indentLevel;
		}

		void DrawDefine(bool defined, string define)
		{
			EditorGUILayout.BeginHorizontal();
			if (defined)
			{
				EditorGUILayout.LabelField($"{define} defined");
				if (GUILayout.Button("Remove define", GUILayout.Width(100)))
				{
					SetDefine(define, false);
				}
			}
			else
			{
				EditorGUILayout.LabelField($"{define} absent");
				if (GUILayout.Button("Add define", GUILayout.Width(100)))
				{
					SetDefine(define, true);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		void DrawInstrumentationSettings()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Instrumentation settings", EditorStyles.boldLabel);

			_instrumentationScroll = EditorGUILayout.BeginScrollView(_instrumentationScroll);

			var settings = MuninnSettings.Instance;

			ref var systemSettings = ref settings.playerLoopSystemSettings;

			var newItems = PopulatePlayerLoopSettings(ref systemSettings);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("None", GUILayout.Width(80)))
			{
				for (var i = 0; i < systemSettings.Length; i++)
				{
					systemSettings[i].shouldBeInstrumented = false;
				}
			}
			if (GUILayout.Button("All", GUILayout.Width(80)))
			{
				for (var i = 0; i < systemSettings.Length; i++)
				{
					systemSettings[i].shouldBeInstrumented = true;
				}
			}
			if (GUILayout.Button("Defaults", GUILayout.Width(80)))
			{
				for (var i = 0; i < systemSettings.Length; i++)
				{
					systemSettings[i].shouldBeInstrumented = Array.IndexOf(DefaultEnabledSystems, systemSettings[i].name) != -1;
				}
			}
			EditorGUILayout.EndHorizontal();

			var fullFameIndex = Array.FindIndex(systemSettings, s => s.name == MuninnPixPlayerLoop.FullFrameName);
			DrawInstrumentationSettings(ref systemSettings[fullFameIndex]);

			var renderingIndex = Array.FindIndex(systemSettings, s => s.name == MuninnPixPlayerLoop.RenderingName);
			DrawInstrumentationSettings(ref systemSettings[renderingIndex]);

			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			var subsystems = playerLoop.subSystemList;
			for (var i = 0; i < subsystems.Length; i++)
			{
				DrawInstrumentationSystemSettings(ref systemSettings, subsystems[i]);
			}

			if (newItems || EditorGUI.EndChangeCheck())
			{
				settings.OnValidate();
			}

			EditorGUILayout.EndScrollView();
		}

		void DrawInstrumentationSettings(ref PlayerLoopSystemSetup setup)
		{
			EditorGUILayout.BeginHorizontal();
			setup.shouldBeInstrumented = EditorGUILayout.ToggleLeft(setup.name, setup.shouldBeInstrumented, GUILayout.ExpandWidth(true));
			setup.color = EditorGUILayout.ColorField(setup.color, GUILayout.Width(80), GUILayout.ExpandWidth(false));
			EditorGUILayout.EndHorizontal();
		}

		void DrawInstrumentationSystemSettings(ref PlayerLoopSystemSetup[] setup, PlayerLoopSystem system)
		{
			var systemName = system.type.Name;
			var index = Array.FindIndex(setup, setup => setup.name == systemName);

			DrawInstrumentationSettings(ref setup[index]);

			var subsystems = system.subSystemList;
			if (subsystems == null)
			{
				return;
			}

			++EditorGUI.indentLevel;
			for (var i = 0; i < subsystems.Length; i++)
			{
				DrawInstrumentationSystemSettings(ref setup, subsystems[i]);
			}
			--EditorGUI.indentLevel;
		}

		static void SetDefine(string define, bool state)
		{
			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
			var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

			PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

			if (state)
			{
				var index = Array.IndexOf(defines, define);
				if (index != -1)
				{
					return;
				}

				var newDefines = new string[defines.Length + 1];
				Array.Copy(defines, 0, newDefines, 0, defines.Length);
				newDefines[^1] = define;

				PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
			}
			else
			{
				var index = Array.IndexOf(defines, define);
				if (index == -1)
				{
					return;
				}

				var newDefines = new string[defines.Length - 1];
				Array.Copy(defines, 0, newDefines, 0, index);
				Array.Copy(defines, index + 1, newDefines, index, defines.Length - index -1);

				PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
			}
		}

		static bool PopulatePlayerLoopSettings(ref PlayerLoopSystemSetup[] setup)
		{
			var added = false;

			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();

			var subsystems = playerLoop.subSystemList;
			for (var i = 0; i < subsystems.Length; i++)
			{
				added = PopulatePlayerLoopSettings(ref setup, subsystems[i]) || added;
			}

			return added;
		}

		static bool PopulatePlayerLoopSettings(ref PlayerLoopSystemSetup[] setup, PlayerLoopSystem system)
		{
			var added = false;
			var systemName = system.type.Name;
			var existingIndex = Array.FindIndex(setup, setup => setup.name == systemName);

			if (existingIndex == -1)
			{
				AddSystem(ref setup, systemName);
				added = true;
			}

			var subsystems = system.subSystemList;
			if (subsystems == null)
			{
				return added;
			}

			for (var i = 0; i < subsystems.Length; i++)
			{
				added = PopulatePlayerLoopSettings(ref setup, subsystems[i]) || added;
			}

			return added;
		}

		static void AddSystem(ref PlayerLoopSystemSetup[] setup, string name)
		{
			Debug.Log($"Adding system {name}");

			var newSetup = new PlayerLoopSystemSetup[setup.Length + 1];
			Array.Copy(setup, 0, newSetup, 0, setup.Length);
			newSetup[setup.Length] = PlayerLoopSystemSetup.Default;
			newSetup[setup.Length].name = name;

			setup = newSetup;
		}
	}
}
