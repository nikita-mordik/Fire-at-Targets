using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Extensions
{
    public static class UITool
    {
        /// <summary>
        /// Changing CanvasGroup state between visible and not visible
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="state"></param>
        public static void State(this CanvasGroup canvasGroup, bool state)
        {
            canvasGroup.alpha = state ? 1.0f : 0.0f;
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }
    }
}