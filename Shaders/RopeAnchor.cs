using UnityEngine;

public class RopeAnchor : MonoBehaviour
{
    private Vector3 velocity;
    private float fallDuration = 3f;

    private void Start()
    {
        Invoke(nameof(RemoveRope), fallDuration);
    }

    private void FixedUpdate()
    {
        // Apply gravity to simulate the rope falling
        velocity += Physics.gravity * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;

        // Apply random motion (like wind)
        velocity.x = Random.Range(-30.0f, 30.0f);
        velocity.y = Random.Range(-30.0f, 30.0f);
    }

    private void RemoveRope()
    {
        Destroy(gameObject); // Clean up rope after falling
    }
}
