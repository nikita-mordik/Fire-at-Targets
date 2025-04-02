using DG.Tweening;
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
        
        /// <summary>
        /// Changing CanvasGroup state by using DoTween animation with params
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="state"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        public static void DoState(this CanvasGroup canvasGroup, bool state, float duration = 0.5f, Ease ease = Ease.InSine)
        {
            canvasGroup.DOFade(state ? 1.0f : 0.0f, duration)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = state;
                    canvasGroup.blocksRaycasts = state;
                });
        }
    }
}