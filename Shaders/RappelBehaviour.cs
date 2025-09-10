using System.Collections;
using UnityEngine;
using uSource;
using uSource.Formats.Source.VTF;

public class RappelBehavior : MonoBehaviour
{
    public Transform ropeAnchorPrefab;
    public float maxRappelSpeed = 600f;
    public float minRappelSpeed = 60f;
    public float decelerationDistance = 20f * 12f; // 240 units
    public string ziplineAttachmentPoint = "zipline";

    private bool isRappeling = false;
    private bool isOnGround = true;
    private GameObject ropeAnchorInstance;
    private LineRenderer ropeRenderer;
    private Vector3 rappelTarget;
    private VMTFile ropeMaterial;
    private string blackMesaMaterialPath = "materials/cable"; // Adjusted path

    private void Start()
    {
        CreateRopeAnchor();
        LoadRopeMaterial();
    }

    private void LoadRopeMaterial()
    {
        if (ropeRenderer == null)
        {
            Debug.LogWarning("RopeRenderer is not initialized.");
            return;
        }

        if (ropeMaterial == null)
        {
            string materialName = "cable.vmt";
            ropeMaterial = uResourceManager.LoadMaterial(blackMesaMaterialPath);

            if (ropeMaterial != null)
            {
                Material hdrpMaterial = new Material(Shader.Find("HDRP/Lit"));
                hdrpMaterial.CopyPropertiesFromMaterial(ropeMaterial.Material);
                ropeRenderer.material = hdrpMaterial;

                Debug.Log("Rope material loaded with HDRP/Lit shader: " + materialName);
            }
            else
            {
                Debug.LogWarning("Rope material not found at path: " + blackMesaMaterialPath);
            }
        }
    }

    public void BeginRappel(Vector3 targetPosition)
    {
        if (isRappeling)
            return;

        rappelTarget = targetPosition;
        isRappeling = true;
        isOnGround = false;

        StartCoroutine(RappelDown());
    }

    private void CreateRopeAnchor()
    {
        if (ropeAnchorPrefab != null)
        {
            ropeAnchorInstance = Instantiate(ropeAnchorPrefab, transform.position, Quaternion.identity).gameObject;
            ropeRenderer = ropeAnchorInstance.AddComponent<LineRenderer>();
            ropeRenderer.positionCount = 2;
            ropeRenderer.startWidth = 0.3f;
            ropeRenderer.endWidth = 0.3f;
        }
        else
        {
            Debug.LogError("RopeAnchorPrefab is not assigned.");
        }
    }

    private IEnumerator RappelDown()
    {
        while (!isOnGround)
        {
            UpdateRope();
            SetDescentSpeed();
            yield return null;
        }
    }

    private void SetDescentSpeed()
    {
        float distanceToGround = Vector3.Distance(transform.position, rappelTarget);
        float speed = maxRappelSpeed;

        if (distanceToGround <= decelerationDistance)
        {
            float factor = distanceToGround / decelerationDistance;
            speed = Mathf.Max(minRappelSpeed, speed * factor);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.down * speed;
        }
        else
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 1f))
        {
            if (hitInfo.collider.CompareTag("Ground"))
            {
                OnHitGround();
            }
        }
    }

    private void UpdateRope()
    {
        if (ropeRenderer != null && ropeAnchorInstance != null)
        {
            ropeRenderer.SetPosition(0, ropeAnchorInstance.transform.position);
            ropeRenderer.SetPosition(1, transform.position);
        }
    }

    private void OnHitGround()
    {
        isOnGround = true;
        isRappeling = false;

        if (ropeAnchorInstance != null)
        {
            Destroy(ropeAnchorInstance);
        }

        OnRappelTouchdown();
    }

    private void OnRappelTouchdown()
    {
        Debug.Log("Rappeler has landed.");
    }
}
