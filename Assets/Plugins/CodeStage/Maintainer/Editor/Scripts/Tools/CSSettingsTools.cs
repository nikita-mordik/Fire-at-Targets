#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

using UnityEditor;

namespace CodeStage.Maintainer.Tools
{
	using UnityEngine;

	internal static class CSSettingsTools
	{
		public static Object GetInSceneLightmapSettings()
		{
			var mi = CSReflectionTools.GetGetLightmapSettingsMethodInfo();
			if (mi != null)
			{
				return (Object)mi.Invoke(null, null);
			}

			Debug.LogError(Maintainer.ErrorForSupport("Can't retrieve LightmapSettings object via reflection!"));
			return null;
		}

		public static Object GetInSceneRenderSettings()
		{
			var mi = CSReflectionTools.GetGetRenderSettingsMethodInfo();
			if (mi != null)
			{
				return (Object)mi.Invoke(null, null);
			}

			Debug.LogError(Maintainer.ErrorForSupport("Can't retrieve RenderSettings object via reflection!"));
			return null;
		}

#if !UNITY_6000_0_OR_NEWER
		public static Object GetInSceneNavigationSettings()
		{
			return UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject;
		}
#endif
	}
}