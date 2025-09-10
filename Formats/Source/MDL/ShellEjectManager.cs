using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    internal class ShellEjectManager : MonoBehaviour
    {
        public GameObject ShellPrefab;
        public Transform EjectPoint;
        public float EjectionForce = 1.0f;
        public float ShellLifetime = 5.0f;

        internal void EjectShell()
        {
            if (ShellPrefab == null || EjectPoint == null)
            {
                Debug.LogWarning("ShellPrefab or EjectPoint is not set!");
                return;
            }

            // Instantiate the shell casing
            GameObject shell = Instantiate(ShellPrefab, EjectPoint.position, EjectPoint.rotation);
            Rigidbody shellRb = shell.GetComponent<Rigidbody>();

            if (shellRb != null)
            {
                // Apply ejection force
                Vector3 ejectionDirection = EjectPoint.right + (EjectPoint.up * Random.Range(0.5f, 1.0f));
                shellRb.AddForce(ejectionDirection * EjectionForce, ForceMode.Impulse);
                shellRb.AddTorque(Random.insideUnitSphere * EjectionForce, ForceMode.Impulse);
            }

            // Destroy the shell after a set time
            Destroy(shell, ShellLifetime);
        }
    }
}
