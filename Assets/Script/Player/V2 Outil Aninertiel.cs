using System.Collections.Generic;
using UnityEngine;

public class V2OutilAninertiel : MonoBehaviour
{
    [System.Serializable]
    private class ObjetFige
    {
        public Rigidbody rb;

        public Vector3 linearVelocity;
        public Vector3 angularVelocity;

        public RigidbodyConstraints anciennesContraintes;
        public bool ancienneGravite;
    }

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootDelay = 0.2f;

    private float shootTimer;

    // Liste des objets actuellement figés
    private List<ObjetFige> objetsFiges = new List<ObjetFige>();


    private void Update()
    {
        shootTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootDelay;
        }
    }


    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        // Création du projectile
        GameObject proj = Instantiate(projectilePrefab,firePoint.position,firePoint.rotation);

        // Récupération du script du projectile
        TirAninertielle tir = proj.GetComponent<TirAninertielle>();

        // Transmission de cet outil au projectile
        if (tir != null)
        {
            tir.Initialize(this);
        }
        else
        {
            Debug.LogWarning("Le projectile ne possède pas le composant TirAninertielle.");
        }
    }


    public void ToggleObject(Rigidbody rb)
    {
        if (rb == null)
            return;

        // Cherche si l'objet est déjà figé
        ObjetFige objet = GetObjetFige(rb);

        if (objet != null)
        {
            LibererObjet(objet);
        }
        else
        {
            FigerObjet(rb);
        }
    }


    private void FigerObjet(Rigidbody rb)
    {
        ObjetFige nouvelObjet = new ObjetFige();

        nouvelObjet.rb = rb;

        // Mémorise le mouvement au moment précis de l'impact
        nouvelObjet.linearVelocity = rb.linearVelocity;
        nouvelObjet.angularVelocity = rb.angularVelocity;

        // Mémorise l'état précédent du Rigidbody
        nouvelObjet.anciennesContraintes = rb.constraints;
        nouvelObjet.ancienneGravite = rb.useGravity;

        objetsFiges.Add(nouvelObjet);

        // Stoppe complètement le mouvement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Désactive la gravité
        rb.useGravity = false;

        // Bloque complètement le Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Debug.Log("Objet figé : " + rb.gameObject.name);
    }


    private void LibererObjet(ObjetFige objet)
    {
        Rigidbody rb = objet.rb;

        if (rb == null)
        {
            objetsFiges.Remove(objet);
            return;
        }

        // Remet les contraintes qu'il avait avant
        rb.constraints = objet.anciennesContraintes;

        // Remet la gravité qu'il avait avant
        rb.useGravity = objet.ancienneGravite;

        // Restitue le mouvement mémorisé
        rb.linearVelocity = objet.linearVelocity;
        rb.angularVelocity = objet.angularVelocity;

        objetsFiges.Remove(objet);

        Debug.Log("Objet libéré : " + rb.gameObject.name);
    }


    private ObjetFige GetObjetFige(Rigidbody rb)
    {
        foreach (ObjetFige objet in objetsFiges)
        {
            if (objet.rb == rb)
                return objet;
        }

        return null;
    }
}