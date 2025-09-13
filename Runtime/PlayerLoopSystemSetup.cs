using System;
using UnityEngine;

namespace KVD.Muninn
{
	[Serializable]
	struct PlayerLoopSystemSetup
	{
		public static readonly PlayerLoopSystemSetup Default = new PlayerLoopSystemSetup
		{
			color = Color.magenta,
		};

		public string name;
		public Color color;
		public bool shouldBeInstrumented;
	}
}
