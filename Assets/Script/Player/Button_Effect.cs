using System.Collections.Generic;
using UnityEngine;

public class Button_Effect : MonoBehaviour
{
    [Header("Scripts à activer / désactiver")]
    [SerializeField]
    private List<MonoBehaviour> scriptsToToggle = new List<MonoBehaviour>();

    [Header("Sécurité")]
    [Tooltip("Délai minimum entre deux activations (évite les doubles appels).")]
    [SerializeField] private float cooldown = 0.2f;

    private float lastToggleTime = -999f;

    // À appeler depuis ton UnityEvent / ta détection de bouton.
    public void ActiverObjet()
    {
        if (Time.time - lastToggleTime < cooldown)
            return;

        lastToggleTime = Time.time;

        foreach (MonoBehaviour script in scriptsToToggle)
        {
            if (script == null) continue;

            script.enabled = !script.enabled;

            Debug.Log($"{script.GetType().Name} sur {script.gameObject.name} : " +
                      (script.enabled ? "activé" : "désactivé"));
        }
    }
}
