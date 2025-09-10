using System.Collections.Generic;
using UnityEngine;
using uSource.Formats.Source.VPK; // Assumes uSource VPK loader is available

namespace uSource.Sound
{
    public class SoundscapeManager : MonoBehaviour
    {
        private Dictionary<string, AudioClip> soundCache = new Dictionary<string, AudioClip>();
        private AudioSource ambientSource;
        private AudioSource musicSource;

        [Header("Audio Settings")]
        public float ambientVolume = 0.5f;
        public float musicVolume = 0.7f;

        private void Awake()
        {
            // Initialize audio sources
            ambientSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();

            ambientSource.loop = true;
            musicSource.loop = true;

            ambientSource.volume = ambientVolume;
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Load an audio clip from the VPK using uSource's VPK loader.
        /// </summary>
        /// <param name="soundPath">Path to the sound file inside the VPK.</param>
        /// <returns>Loaded AudioClip or null if not found.</returns>
        private AudioClip LoadSound(string soundPath)
        {
            if (soundCache.TryGetValue(soundPath, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            // Load from VPK

            VPKFile vPK = new("");

            AudioClip clip = vPK.LoadAudioClip(soundPath);
            if (clip != null)
            {
                soundCache[soundPath] = clip;
                Debug.Log($"Loaded sound: {soundPath}");
                return clip;
            }

            Debug.LogError($"Sound not found in VPK: {soundPath}");
            return null;
        }

        /// <summary>
        /// Play a looping ambient sound.
        /// </summary>
        /// <param name="soundPath">Path to the ambient sound file in the VPK.</param>
        public void PlayAmbientSound(string soundPath)
        {
            AudioClip clip = LoadSound(soundPath);
            if (clip != null)
            {
                ambientSource.clip = clip;
                ambientSource.Play();
            }
        }

        /// <summary>
        /// Stop the currently playing ambient sound.
        /// </summary>
        public void StopAmbientSound()
        {
            ambientSource.Stop();
        }

        /// <summary>
        /// Play background music.
        /// </summary>
        /// <param name="musicPath">Path to the music file in the VPK.</param>
        public void PlayMusic(string musicPath)
        {
            AudioClip clip = LoadSound(musicPath);
            if (clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }

        /// <summary>
        /// Stop the currently playing music.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}
