using UnityEngine;
using Starter.Core;

namespace Game.System
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] AudioSource _bgmSource;
        [SerializeField] AudioSource _sfxSource;

        protected override void Awake()
        {
            base.Awake();
            if (_bgmSource == null) _bgmSource = CreateSource("BGM", loop: true);
            if (_sfxSource == null) _sfxSource = CreateSource("SFX", loop: false);
        }

        public void PlayBGM(AudioClip clip, float volume = 0.5f)
        {
            if (_bgmSource.clip == clip) return;
            _bgmSource.clip = clip;
            _bgmSource.volume = volume;
            _bgmSource.Play();
        }

        public void StopBGM() => _bgmSource.Stop();

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip != null)
                _sfxSource.PlayOneShot(clip, volume);
        }

        public void SetBGMVolume(float v) => _bgmSource.volume = v;
        public void SetSFXVolume(float v) => _sfxSource.volume = v;

        AudioSource CreateSource(string label, bool loop)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            return src;
        }
    }
}
