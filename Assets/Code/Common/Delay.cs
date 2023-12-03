using System;
using System.Collections;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Common
{
  public static class Delay
  {
    public static void For(int frames, MonoBehaviour behaviour, Action andThen)
    {
      if (frames > 0)
        behaviour.StartCoroutine(DoFrameDelay(frames, andThen));
      else
        andThen();
    }

    public static void For(float seconds, MonoBehaviour behaviour, Action andThen)
    {
      if (seconds > 0)
        behaviour.StartCoroutine(DoDelay(seconds, andThen));
      else
        andThen();
    }

    public static IEnumerator For(MonoBehaviour behaviour, float time, Action andThen)
    {
      IEnumerator coroutine = DoDelay(time, andThen);
      behaviour.StartCoroutine(coroutine);
      return coroutine;
    }

    private static IEnumerator DoFrameDelay(int frames, Action andThen)
    {
      for (int i = 0; i < frames; i++)
        yield return null;

      andThen();
    }

    private static IEnumerator DoDelay(float time, Action andThen)
    {
      yield return new WaitForSeconds(time);
      andThen();
    }
  }
}
