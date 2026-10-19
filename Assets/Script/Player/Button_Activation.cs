
using System.Collections.Generic;
using UnityEngine;

public class Button_Activation : MonoBehaviour
{
    [Header("Tags autorisés")]
    [SerializeField]
    private List<string> objectTags = new List<string>
    {
        "Button"
    };

    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask targetLayers = ~0;

    private void Awake()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            ButtonSwitch();
        }
    }

    private void ButtonSwitch()
    {
        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green,1f);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        Button_Effect bouton = hit.collider.GetComponent<Button_Effect>();

        if (bouton == null)
            return;

        // Vérifie le tag de l'objet portant Button_Effect.
        bool tagAutorise = false;

        foreach (string tag in objectTags)
        {
            if (!string.IsNullOrWhiteSpace(tag) && bouton.gameObject.CompareTag(tag))
            {
                tagAutorise = true;
                break;
            }
        }

        if (!tagAutorise)
            return;

        bouton.ActiverObjet();
    }
}
