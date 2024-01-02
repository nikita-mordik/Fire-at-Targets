using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Common
{
  public class SelfDestructor : MonoBehaviour
  {
    public float Countdown = 3.0f;

    private void Update()
    {
      Countdown -= Time.deltaTime;
      if (Countdown <= 0)
        Destroy(gameObject);
    }
  }
}