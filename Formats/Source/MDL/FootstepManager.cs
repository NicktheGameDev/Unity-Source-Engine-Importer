using System.Collections.Generic;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    internal class FootstepManager : MonoBehaviour
    {
        [System.Serializable]
        public class SurfaceFootstep
        {
            public string SurfaceType;
            public AudioClip[] FootstepSounds;
        }

        public List<SurfaceFootstep> SurfaceFootsteps = new List<SurfaceFootstep>();
        public string DefaultSurface = "Concrete";

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        internal void PlayFootstep(string surfaceType = null)
        {
            // Determine the surface type
            string type = surfaceType ?? DefaultSurface;

            // Find the matching surface type and play a random footstep sound
            foreach (var surface in SurfaceFootsteps)
            {
                if (surface.SurfaceType == type)
                {
                    if (surface.FootstepSounds.Length > 0)
                    {
                        AudioClip clip = surface.FootstepSounds[Random.Range(0, surface.FootstepSounds.Length)];
                        audioSource.PlayOneShot(clip);
                        return;
                    }
                }
            }

            // Fallback to default surface if no match is found
            Debug.LogWarning($"No footstep sounds found for surface type '{type}'. Playing default sound.");
            PlayFootstep(DefaultSurface);
        }
    }
}
