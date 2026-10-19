using UnityEngine;

public class Bumper : MonoBehaviour
{
    [Header("Forces")]
    [SerializeField] private float forwardForce = 0f;
    [SerializeField] private float upwardForce = 0f;
    [SerializeField] private float horizontalForce = 0f;

    [Header("Detection")]
    [SerializeField] private string objectTag = "ObjAni";
    [SerializeField] private string playerTag = "Player";

    [Header("Options")]
    [SerializeField] private bool pushPlayer = true;
    [SerializeField] private bool pushObjects = true;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private void OnTriggerEnter(Collider other)
    {
        // OnTriggerEnter est appelé même si le script est désactivé :
        // on bloque donc manuellement.
        if (!enabled) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
        {
            if (showDebug)
                Debug.LogWarning("Bumper : aucun Rigidbody sur " + other.name);

            return;
        }

        GameObject target = rb.gameObject;

        if (target.CompareTag(playerTag))
        {
            if (pushPlayer)
                Push(rb);

            return;
        }

        if (target.CompareTag(objectTag) && pushObjects)
        {
            Push(rb);
        }
    }

    private void Push(Rigidbody rb)
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 force =
            forward * forwardForce +
            right * horizontalForce +
            Vector3.up * upwardForce;

        PlayerMovement player = rb.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.ReceiveBumperForce(force);
        }
        else
        {
            // Les objets physiques reçoivent directement l'impulsion.
            rb.AddForce(force, ForceMode.Impulse);
        }

        if (showDebug)
        {
            Debug.Log($"Bumper : {rb.gameObject.name} reçoit l'impulsion {force}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.right * 2f);
    }
}