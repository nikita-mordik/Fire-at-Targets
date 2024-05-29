using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Extensions
{
    public static class Vibratior
    {
#if UNITY_ANDROID
        private static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        private static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        private static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

        public static void Vibrate(long milliseconds = 250)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                vibrator.Call("vibrate", milliseconds);
            }
            else
            {
                Handheld.Vibrate();
            }
        }

        public static void Cancel()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                vibrator.Call("cancel");
            }
        }
#endif

    }
}