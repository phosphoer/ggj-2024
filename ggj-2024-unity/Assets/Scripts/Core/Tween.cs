using UnityEngine;
using System.Collections;

public static class Tween
{
  public static IEnumerator WaitForTime(float duration)
  {
    for (float timer = 0; timer < duration; timer += Time.deltaTime)
    {
      yield return null;
    }
  }

  public static IEnumerator WaitForTimeUnscaled(float duration)
  {
    for (float timer = 0; timer < duration; timer += Time.unscaledDeltaTime)
    {
      yield return null;
    }
  }

  public static IEnumerator DelayCall(float delaySeconds, System.Action callback)
  {
    yield return WaitForTime(delaySeconds);
    callback?.Invoke();
  }

  // Usage: yield return WaitForEvent(f => MyEvent += f, f => MyEvent -= f)
  public static IEnumerator WaitForEvent(System.Action<System.Action> subscribeDelegate, System.Action<System.Action> unsubscribeDelegate)
  {
    bool invoked = false;

    System.Action listener = () => invoked = true;

    subscribeDelegate?.Invoke(listener);

    // Wait for event
    while (!invoked)
      yield return null;

    unsubscribeDelegate?.Invoke(listener);
  }

  public static IEnumerator WaitForEvent<T>(System.Action<System.Action<T>> subscribeDelegate, System.Action<System.Action<T>> unsubscribeDelegate)
  {
    bool invoked = false;

    System.Action<T> listener = (obj) => invoked = true;

    subscribeDelegate?.Invoke(listener);

    // Wait for event
    while (!invoked)
      yield return null;

    unsubscribeDelegate?.Invoke(listener);
  }

  public static IEnumerator WaitForEvent<T1, T2>(System.Action<System.Action<T1, T2>> subscribeDelegate, System.Action<System.Action<T1, T2>> unsubscribeDelegate)
  {
    bool invoked = false;

    System.Action<T1, T2> listener = (a, b) => invoked = true;

    subscribeDelegate?.Invoke(listener);

    // Wait for event
    while (!invoked)
      yield return null;

    unsubscribeDelegate?.Invoke(listener);
  }

  public static IEnumerator CustomTween(float duration, System.Action<float> tweenFunc)
  {
    for (float timer = 0; timer < duration; timer += Time.deltaTime)
    {
      tweenFunc?.Invoke(timer / duration);
      yield return null;
    }

    tweenFunc?.Invoke(1);
  }

  public static IEnumerator CustomTweenUnscaled(float duration, System.Action<float> tweenFunc)
  {
    for (float timer = 0; timer < duration; timer += Time.unscaledDeltaTime)
    {
      tweenFunc?.Invoke(timer / duration);
      yield return null;
    }

    tweenFunc?.Invoke(1);
  }

  public static IEnumerator HermiteScale(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
  {
    for (float timer = 0; timer < duration; timer += Time.deltaTime)
    {
      float t = Mathf.Clamp01(timer / duration);
      t = Mathfx.Hermite(0.0f, 1.0f, t);
      if (transform != null)
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
      else
        yield break;

      yield return null;
    }

    if (transform != null)
      transform.localScale = endScale;
  }

  public static IEnumerator HermiteScaleRealtime(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
  {
    float startTime = Time.unscaledTime;
    while (Time.unscaledTime < startTime + duration)
    {
      float t = Mathf.Clamp01((Time.unscaledTime - startTime) / duration);
      t = Mathfx.Hermite(0.0f, 1.0f, t);
      if (transform != null)
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
      else
        yield break;

      yield return null;
    }
  }

  public static IEnumerator CurveScaleRealtime(Transform transform, float duration, AnimationCurve curve)
  {
    yield return CurveScaleRealtime(transform, duration, curve, Vector3.one);
  }

  public static IEnumerator CurveScaleRealtime(Transform transform, float duration, AnimationCurve curve, Vector3 startScale)
  {
    for (float timer = 0; timer < duration; timer += Time.unscaledDeltaTime)
    {
      float t = Mathf.Clamp01(timer / duration);
      float scaleValue = curve.Evaluate(t);
      if (transform != null)
        transform.localScale = startScale * scaleValue;
      yield return null;
    }
  }
}