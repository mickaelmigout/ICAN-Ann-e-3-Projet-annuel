
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class TirAninertielle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseSpeed = 20f;
    [SerializeField] private float maxDistance = 100f;

    [Header("Impact")]
    [Tooltip("Tous les tags que le projectile peut affecter.")]
    [SerializeField]
    private List<string> objectTags = new List<string>
    {
        "ObjAni"
    };

    private Rigidbody rb;
    private Vector3 startPosition;

    private V2OutilAninertiel outil;

    public void Initialize(V2OutilAninertiel outilTireur)
    {
        outil = outilTireur;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * baseSpeed;
    }

    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody targetRb = other.attachedRigidbody;

        // Si aucun Rigidbody n'est trouvé, on ne peut pas figer l'objet.
        if (targetRb == null)
        {
            Debug.Log("Pas de Rigidbody sur l'objet touché : " + other.name);
            return;
        }

        // Vérifie le tag du GameObject qui porte le Rigidbody.
        bool tagAccepted = false;

        foreach (string acceptedTag in objectTags)
        {
            if (!string.IsNullOrWhiteSpace(acceptedTag) && targetRb.gameObject.CompareTag(acceptedTag))
            {
                tagAccepted = true;
                break;
            }
        }

        if (!tagAccepted)
            return;

        Debug.Log("Impact sur un objet autorisé : " + targetRb.gameObject.name);

        if (outil != null)
        {
            outil.ToggleObject(targetRb);
        }
        else
        {
            Debug.LogWarning("Le projectile n'a pas reçu de référence vers V2OutilAninertiel.");
        }

        Destroy(gameObject);
    }
}
