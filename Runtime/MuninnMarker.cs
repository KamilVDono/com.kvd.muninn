#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_GAMECORE) && !PIX_DISABLED
#define PIX_AVAILABLE
#endif
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_GAMECORE || UNITY_PS5) && !SUPERLUMINAL_DISABLED
#define SUPERLUMINAL_AVAILABLE
#endif
#if (PIX_AVAILABLE || SUPERLUMINAL_AVAILABLE) && (DEBUG || MUNINN_ENABLED || UNITY_EDITOR)
#define NATIVE_PROFILING
#endif

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pix;
using Superluminal;
using Unity.Profiling;
using UnityEngine;

namespace KVD.Muninn
{
	public readonly struct MuninnMarker
	{
#if NATIVE_PROFILING
		readonly string _name;
		readonly Color _color;
#endif
#if ENABLE_PROFILER
		readonly ProfilerMarker _unityMarker;
#endif

		public MuninnMarker(string name)
		{
#if NATIVE_PROFILING
			_name = name;
			_color = new Color(1.0f, 0.97f, 0.63f);
#endif
#if ENABLE_PROFILER
			_unityMarker = new ProfilerMarker(name);
#endif
		}

		public MuninnMarker(ProfilerCategory category, string name)
		{
#if NATIVE_PROFILING
			_name = name;
			_color = new Color(1.0f, 0.97f, 0.63f);
#endif
#if ENABLE_PROFILER
			_unityMarker = new ProfilerMarker(category, name);
#endif
		}

		public MuninnMarker(in Color color, string name)
		{
#if NATIVE_PROFILING
			_name = name;
			_color = color;
#endif
#if ENABLE_PROFILER
			_unityMarker = new ProfilerMarker(name);
#endif
		}

		public MuninnMarker(ProfilerCategory category, in Color color, string name)
		{
#if NATIVE_PROFILING
			_name = $"{category.Name}/{name}";
			_color = color;
#endif
#if ENABLE_PROFILER
			_unityMarker = new ProfilerMarker(category, name);
#endif
		}

		[Conditional("MUNINN_ENABLED"), Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Begin()
		{
#if ENABLE_PROFILER
			_unityMarker.Begin();
#endif
#if NATIVE_PROFILING
#if SUPERLUMINAL_AVAILABLE
			SuperluminalWrapper.StartEvent(_color, _name);
#endif
#if PIX_AVAILABLE
			PixWrapper.StartEvent(_color, _name);
#endif
#endif
		}

		[Conditional("MUNINN_ENABLED"), Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void End()
		{
#if ENABLE_PROFILER
			_unityMarker.End();
#endif
#if NATIVE_PROFILING
#if SUPERLUMINAL_AVAILABLE
			SuperluminalWrapper.EndEvent();
#endif
#if PIX_AVAILABLE
			PixWrapper.EndEvent();
#endif
#endif
		}

		[Conditional("MUNINN_ENABLED"), Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetMarker()
		{
#if ENABLE_PROFILER
			_unityMarker.Begin();
			_unityMarker.End();
#endif
#if NATIVE_PROFILING
#if SUPERLUMINAL_AVAILABLE
			SuperluminalWrapper.SetMarker(_color, _name);
#endif
#if PIX_AVAILABLE
			PixWrapper.SetMarker(_color, _name);
#endif
#endif
		}

		[Conditional("MUNINN_ENABLED"), Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReportCounter(float value)
		{
#if ENABLE_PROFILER
			_unityMarker.Begin(value);
			_unityMarker.End();
#endif
#if NATIVE_PROFILING
#if SUPERLUMINAL_AVAILABLE
			SuperluminalWrapper.ReportCounter(_name, value);
#endif
#if PIX_AVAILABLE
			PixWrapper.ReportCounter(_name, value);
#endif
#endif
		}

		[Conditional("MUNINN_ENABLED"), Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Begin(float value)
		{
#if ENABLE_PROFILER
			_unityMarker.Begin(value);
#endif
#if NATIVE_PROFILING
#if SUPERLUMINAL_AVAILABLE
			SuperluminalWrapper.StartEvent(_color, _name, value);
#endif
#if PIX_AVAILABLE
			PixWrapper.ReportCounter(_name, value);
#endif
#endif
		}

		[Conditional("MUNINN_ENABLED"), Conditional("DEBUG")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Begin(int value)
		{
#if ENABLE_PROFILER
			_unityMarker.Begin(value);
#endif
#if NATIVE_PROFILING
#if SUPERLUMINAL_AVAILABLE
			SuperluminalWrapper.StartEvent(_color, _name, value);
#endif
#if PIX_AVAILABLE
			PixWrapper.ReportCounter(_name, value);
			PixWrapper.StartEvent(_color, _name);
#endif
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AutoScope Auto()
		{
			return new AutoScope(this);
		}

		public readonly ref struct AutoScope
		{
#if NATIVE_PROFILING || ENABLE_PROFILER
			readonly MuninnMarker _marker;
#endif

			public AutoScope(in MuninnMarker marker)
			{
#if NATIVE_PROFILING || ENABLE_PROFILER
				_marker = marker;
				_marker.Begin();
#endif
			}

			public void Dispose()
			{
#if NATIVE_PROFILING || ENABLE_PROFILER
				_marker.End();
#endif
			}
		}
	}
}
