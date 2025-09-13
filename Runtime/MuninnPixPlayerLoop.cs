#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_GAMECORE) && (DEBUG || MUNINN_ENABLED || UNITY_EDITOR)
#define MUNINN_PLAYERLOOP_ENABLED
#endif

using System.Collections.Generic;
using Pix;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Rendering;

namespace KVD.Muninn
{
	public class MuninnPixPlayerLoop
	{
		public const string FullFrameName = "FullFrame";
		public const string RenderingName = "Rendering";

		public static MuninnPixPlayerLoop Instance{ get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Initialize()
		{
			Instance = new MuninnPixPlayerLoop();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void AutoInstrument()
		{
			if (!MuninnSettings.Instance.disableAutoPlayerLoopInstrumentation)
			{
				Instance.Instrument();
			}
		}

		bool _instrumented;
		PlayerLoopSystemSetup _renderingSystemSetup;

		MuninnPixPlayerLoop() {}

		public void Instrument()
		{
#if !MUNINN_PLAYERLOOP_ENABLED
			return;
#endif
			if (_instrumented)
			{
				return;
			}
			InstrumentPlayerLoop();
			InstrumentRendering();

			_instrumented = true;
		}

		static void InstrumentPlayerLoop()
		{
			var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			ProcessPlayerLoopSystem(ref playerLoop);

			var systemSettings = MuninnSettings.Instance.GetPlayerLoopSystemSettings(FullFrameName);

			if (systemSettings.shouldBeInstrumented)
			{
				var subsystems = playerLoop.subSystemList;
				var newSubsystems = new PlayerLoopSystem[subsystems.Length + 2];

				var name = FullFrameName;
				var color = systemSettings.color;

				newSubsystems[0] = new PlayerLoopSystem
				{
					type = typeof(FrameStart),
					updateDelegate = () => PixWrapper.StartEvent(color, name),
				};

				for (var i = 0; i < subsystems.Length; i++)
				{
					newSubsystems[i+1] = subsystems[i];
				}

				newSubsystems[subsystems.Length + 1] = new PlayerLoopSystem
				{
					type = typeof(FrameEnd),
					updateDelegate = static () => PixWrapper.EndEvent(),
				};

				playerLoop.subSystemList = newSubsystems;
			}

			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		static void ProcessPlayerLoopSystem(ref PlayerLoopSystem playerLoopSystem)
		{
			var systems = playerLoopSystem.subSystemList;
			if (systems == null)
			{
				return;
			}

			var instrumentedCount = 0;
			for (var i = 0; i < systems.Length; i++)
			{
				var system = systems[i];
				var name = system.type?.Name ?? "Unknown";

				var systemSettings = MuninnSettings.Instance.GetPlayerLoopSystemSettings(name);

				if (systemSettings.shouldBeInstrumented)
				{
					instrumentedCount += 3;
				}
				else
				{
					instrumentedCount += 1;
				}
			}

			var newSystemIndex = 0;
			var newSystems = new PlayerLoopSystem[instrumentedCount];
			for (var i = 0; i < systems.Length; i++)
			{
				var system = systems[i];
				var name = system.type?.Name ?? "Unknown";

				var systemSettings = MuninnSettings.Instance.GetPlayerLoopSystemSettings(name);

				if (systemSettings.shouldBeInstrumented)
				{
					var beginType = typeof(BeginPixLoopSample);
					var endType = typeof(EndPixLoopSample);
#if UNITY_EDITOR
					if (system.type != null)
					{
						beginType = typeof(BeginPixLoopSample<>).MakeGenericType(system.type);
						endType = typeof(EndPixLoopSample<>).MakeGenericType(system.type);
					}
#endif

					var color = systemSettings.color;
					newSystems[newSystemIndex] = new PlayerLoopSystem
					{
						type = beginType,
						updateDelegate = () => PixWrapper.StartEvent(color, name),
					};
					++newSystemIndex;

					newSystems[newSystemIndex] = systems[i];
					ProcessPlayerLoopSystem(ref newSystems[newSystemIndex]);
					++newSystemIndex;

					newSystems[newSystemIndex] = new PlayerLoopSystem
					{
						type = endType,
						updateDelegate = static () => PixWrapper.EndEvent(),
					};
					++newSystemIndex;
				}
				else
				{
					newSystems[newSystemIndex] = systems[i];
					ProcessPlayerLoopSystem(ref newSystems[newSystemIndex]);

					++newSystemIndex;
				}
			}
			playerLoopSystem.subSystemList = newSystems;
		}

		void InstrumentRendering()
		{
			_renderingSystemSetup = MuninnSettings.Instance.GetPlayerLoopSystemSettings(RenderingName);

			RenderPipelineManager.beginContextRendering -= BeginContextRendering;
			RenderPipelineManager.endContextRendering -= EndContextRendering;
			if (_renderingSystemSetup.shouldBeInstrumented)
			{
				RenderPipelineManager.beginContextRendering += BeginContextRendering;
				RenderPipelineManager.endContextRendering += EndContextRendering;
			}
		}

		void BeginContextRendering(ScriptableRenderContext _, List<Camera> __)
		{
			PixWrapper.StartEvent(_renderingSystemSetup.color, RenderingName);
		}

		void EndContextRendering(ScriptableRenderContext _, List<Camera> __)
		{
			PixWrapper.EndEvent();
		}

#if UNITY_EDITOR
		struct BeginPixLoopSample<T>
		{
		}

		struct EndPixLoopSample<T>
		{
		}
#endif
		struct BeginPixLoopSample
		{
		}

		struct EndPixLoopSample
		{
		}

		struct FrameStart {}

		struct FrameEnd {}
	}
}
