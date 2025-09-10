using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    internal class MuzzleFlashManager : MonoBehaviour
    {
        public ParticleSystem MuzzleFlashParticles;
        public Light MuzzleFlashLight;

        private float lightDuration = 0.05f;

        private void Awake()
        {
            if (MuzzleFlashLight != null)
            {
                MuzzleFlashLight.enabled = false;
            }
        }

        internal void TriggerMuzzleFlash()
        {
            if (MuzzleFlashParticles != null)
            {
                MuzzleFlashParticles.Play();
            }

            if (MuzzleFlashLight != null)
            {
                StartCoroutine(FlashLight());
            }
        }

        private System.Collections.IEnumerator FlashLight()
        {
            MuzzleFlashLight.enabled = true;
            yield return new WaitForSeconds(lightDuration);
            MuzzleFlashLight.enabled = false;
        }
    }
}
