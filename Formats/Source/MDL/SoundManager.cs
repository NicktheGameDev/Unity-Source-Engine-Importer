using System.Collections.Generic;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    public class SoundManager : MonoBehaviour
    {
        // A dictionary to store audio clips by their name for easy access
        private Dictionary<string, AudioClip> soundLibrary;

        // Reference to an AudioSource component for playing sounds
        private AudioSource audioSource;

        // Initialize the SoundManager
        private void Awake()
        {
            // Initialize the sound library
            soundLibrary = new Dictionary<string, AudioClip>();

            // Ensure an AudioSource component is attached to the GameObject
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            Debug.Log("SoundManager initialized.");
        }

        /// <summary>
        /// Load an AudioClip into the sound library.
        /// </summary>
        /// <param name="clip">The AudioClip to load.</param>
        /// <param name="soundName">The name to associate with the clip.</param>
        public void LoadSound(AudioClip clip, string soundName)
        {
            if (!soundLibrary.ContainsKey(soundName))
            {
                soundLibrary.Add(soundName, clip);
                Debug.Log($"Loaded sound: {soundName}");
            }
            else
            {
                Debug.LogWarning($"Sound '{soundName}' is already loaded.");
            }
        }

        /// <summary>
        /// Play a sound by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlaySound(string soundName)
        {
            if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log($"Playing sound: {soundName}");
            }
            else
            {
                Debug.LogError($"Sound '{soundName}' not found in library.");
            }
        }

        /// <summary>
        /// Stop the currently playing sound.
        /// </summary>
        public void StopSound()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Stopped current sound.");
            }
            else
            {
                Debug.LogWarning("No sound is currently playing.");
            }
        }

        /// <summary>
        /// Adjust the volume of the AudioSource.
        /// </summary>
        /// <param name="volume">Volume level between 0.0f and 1.0f.</param>
        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            audioSource.volume = volume;
            Debug.Log($"Set volume to: {volume}");
        }

        /// <summary>
        /// Remove a sound from the library by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to remove.</param>
        public void RemoveSound(string soundName)
        {
            if (soundLibrary.Remove(soundName))
            {
                Debug.Log($"Removed sound: {soundName}");
            }
            else
            {
                Debug.LogWarning($"Sound '{soundName}' not found in library.");
            }
        }

        /// <summary>
        /// Check if a sound is loaded in the library.
        /// </summary>
        /// <param name="soundName">The name of the sound to check.</param>
        /// <returns>True if the sound is loaded, otherwise false.</returns>
        public bool IsSoundLoaded(string soundName)
        {
            return soundLibrary.ContainsKey(soundName);
        }
    }
}
