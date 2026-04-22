using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Starter.Core;

namespace Starter.Bootstrap
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        public event Action<float> OnProgress;
        public event Action OnComplete;

        public void Load(string sceneName) => StartCoroutine(LoadAsync(sceneName));
        public void Load(int index) => StartCoroutine(LoadAsync(index));

        IEnumerator LoadAsync(string sceneName)
            => Run(SceneManager.LoadSceneAsync(sceneName));

        IEnumerator LoadAsync(int index)
            => Run(SceneManager.LoadSceneAsync(index));

        IEnumerator Run(AsyncOperation op)
        {
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
            {
                OnProgress?.Invoke(op.progress / 0.9f);
                yield return null;
            }
            OnProgress?.Invoke(1f);
            op.allowSceneActivation = true;
            yield return op;
            OnComplete?.Invoke();
        }
    }
}
