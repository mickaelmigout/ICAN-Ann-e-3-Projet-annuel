using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bouton_Objet : MonoBehaviour
{
    [Header("Détection")]
    [Tooltip("Tags des GameObjects qui peuvent activer le bouton.")]
    [SerializeField] private List<string> validTags = new List<string>() { "ObjAni" };

    [Tooltip("Nombre d'objets valides nécessaires dans la zone pour activer le bouton.")]
    [Min(1)]
    [SerializeField] private int requiredCount = 1;

    [Header("Matériaux")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material activatedMaterial;

    [Header("Scripts à activer / désactiver")]
    [SerializeField] private List<MonoBehaviour> scriptsToToggle = new List<MonoBehaviour>();

    [Tooltip("Si coché, les scripts reviennent à leur état d'origine quand le bouton se relâche.")]
    [SerializeField] private bool revertOnRelease = true;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    // Objet -> nombre de colliders de cet objet présents dans la zone
    private readonly Dictionary<GameObject, int> objectsInZone = new Dictionary<GameObject, int>();
    private readonly List<GameObject> toRemove = new List<GameObject>();

    private bool isActivated = false;

    private void Reset()
    {
        // Configure automatiquement le collider en trigger quand on ajoute le script.
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        ApplyMaterial();
    }

    private void Update()
    {
        // OnTriggerExit n'est pas appelé si un objet est détruit ou désactivé dans la zone :
        // on nettoie ici pour éviter un bouton bloqué.
        CleanupInvalidObjects();
        Evaluate();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject owner = GetValidOwner(other);
        if (owner == null) return;

        objectsInZone.TryGetValue(owner, out int count);
        objectsInZone[owner] = count + 1;
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject owner = GetOwner(other);
        if (owner == null) return;

        if (!objectsInZone.TryGetValue(owner, out int count)) return;

        if (count <= 1) objectsInZone.Remove(owner);
        else objectsInZone[owner] = count - 1;
    }

    // Retourne l'objet "propriétaire" du collider (le Rigidbody s'il existe).
    private GameObject GetOwner(Collider col)
    {
        return col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject;
    }

    // Retourne le propriétaire seulement si lui ou le collider a un tag valide.
    private GameObject GetValidOwner(Collider col)
    {
        GameObject owner = GetOwner(col);

        foreach (string tag in validTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;

            if (owner.CompareTag(tag) || col.gameObject.CompareTag(tag))
                return owner;
        }

        return null;
    }

    private void CleanupInvalidObjects()
    {
        toRemove.Clear();

        foreach (GameObject go in objectsInZone.Keys)
        {
            if (go == null || !go.activeInHierarchy)
                toRemove.Add(go);
        }

        foreach (GameObject go in toRemove)
            objectsInZone.Remove(go);
    }

    private void Evaluate()
    {
        bool shouldBeActive = objectsInZone.Count >= requiredCount;

        if (shouldBeActive == isActivated) return;

        isActivated = shouldBeActive;
        ApplyMaterial();

        if (isActivated)
        {
            ToggleScripts();
        }
        else if (revertOnRelease)
        {
            ToggleScripts();
        }

        if (showDebug)
        {
            Debug.Log($"Bouton {name} : {(isActivated ? "activé" : "désactivé")} " + $"({objectsInZone.Count}/{requiredCount} objets)", this);
        }
    }

    private void ToggleScripts()
    {
        foreach (MonoBehaviour script in scriptsToToggle)
        {
            if (script == null) continue;

            script.enabled = !script.enabled;

            if (showDebug)
            {
                Debug.Log($"{script.GetType().Name} sur {script.gameObject.name} : " + (script.enabled ? "activé" : "désactivé"));
            }
        }
    }

    private void ApplyMaterial()
    {
        if (targetRenderer == null) return;

        Material mat = isActivated ? activatedMaterial : defaultMaterial;
        if (mat != null)
            targetRenderer.sharedMaterial = mat;
    }
}
