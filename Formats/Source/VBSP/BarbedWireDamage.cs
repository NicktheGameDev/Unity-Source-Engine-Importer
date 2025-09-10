using UnityEngine;

public class BarbedWireDamage : MonoBehaviour
{
    public float damagePerSecond = 10f;
    private bool isPlayerInContact = false;
    private GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            isPlayerInContact = true;
            player = other.gameObject;

            // Optional: Play barbed wire sound effect
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Sounds/barbed_hit"), transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            isPlayerInContact = false;
        }
    }

    private void Update()
    {
        if (isPlayerInContact)
        {
            // Apply continuous damage over time
            player.GetComponent<PlayerController>().ApplyDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
