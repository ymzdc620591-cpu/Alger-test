using System;
using System.Collections;
using UnityEngine;

namespace Starter.Runtime
{
    // 用法：Timer.Delay(2f, () => Debug.Log("2秒后"));
    //       var c = Timer.Repeat(1f, Tick);   Timer.Cancel(c);
    public class Timer : MonoBehaviour
    {
        static Timer _instance;

        static Timer Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject("[Timer]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<Timer>();
                return _instance;
            }
        }

        public static Coroutine Delay(float seconds, Action callback)
            => Instance.StartCoroutine(DelayRoutine(seconds, callback));

        public static Coroutine Repeat(float interval, Action callback, float duration = -1f)
            => Instance.StartCoroutine(RepeatRoutine(interval, callback, duration));

        public static void Cancel(Coroutine coroutine)
        {
            if (coroutine != null)
                Instance.StopCoroutine(coroutine);
        }

        static IEnumerator DelayRoutine(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        static IEnumerator RepeatRoutine(float interval, Action callback, float duration)
        {
            var wait = new WaitForSeconds(interval);
            float elapsed = 0f;
            while (duration < 0f || elapsed < duration)
            {
                yield return wait;
                elapsed += interval;
                callback?.Invoke();
            }
        }
    }
}
