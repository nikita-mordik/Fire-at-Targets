using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FreedLOW.FireAtTargets.Code.Extensions;
using TMPro;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.UI.PopUp
{
    public class GameStartPopUp : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _popUpCanvasGroup;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private float _animDuration = 0.5f;
        [SerializeField] private float _delayAfterAnim = 1f;

        public async UniTask StartCountdownAsync(int startFrom, CancellationToken token, Action onComplete = null)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                _popUpCanvasGroup.State(true);
                for (int i = startFrom; i > 0; i--)
                {
                    token.ThrowIfCancellationRequested();
                    _countdownText.text = i.ToString();
                    _countdownText.transform.localScale = Vector3.zero;

                    await _countdownText.transform
                        .DOScale(Vector3.one, _animDuration)
                        .SetEase(Ease.OutBack)
                        .AsyncWaitForCompletion();

                    await UniTask.Delay(TimeSpan.FromSeconds(_delayAfterAnim), 
                        false, PlayerLoopTiming.Update, token);
                }

                _popUpCanvasGroup.State(false);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                _popUpCanvasGroup.State(false);
                DOTween.Kill(this);
                throw;
            }
            catch (Exception ex)
            {
                _popUpCanvasGroup.State(false);
                Debug.LogError($"Error after countdown: {ex}");
            }
        }
    }
}