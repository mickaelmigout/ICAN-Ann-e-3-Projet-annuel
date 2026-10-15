using UnityEngine;

public class OutilsAninertiel : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform positionFire;
    [SerializeField] private float rayDistance = 20f;
    [SerializeField] private LayerMask raycastLayer = ~0;

    [Header("Debug")]
    [SerializeField] private bool showRay = true;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ShootRay();
        }
    }

    private void ShootRay()
    {
        if (positionFire == null)
        {
            Debug.LogWarning("Position Fire n'est pas assigné !");
            return;
        }

        Ray ray = new Ray(positionFire.position,positionFire.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, raycastLayer))
        {
            // GetComponentInParent permet aussi de fonctionner
            // si le collider touché est sur un enfant.
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = false;

                Debug.Log( "Gravité désactivée sur : " + rb.gameObject.name);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showRay || positionFire == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawRay( positionFire.position, positionFire.forward * rayDistance );
    }
}