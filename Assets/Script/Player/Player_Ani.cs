
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player_Ani : MonoBehaviour
{
    [Header("État")]
    public bool JoueurAni = false;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.SetAniState(JoueurAni);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("LB"))
        {
            JoueurAni = !JoueurAni;

            if (playerMovement != null)
            {
                playerMovement.SetAniState(JoueurAni);
            }

            Debug.Log(
                JoueurAni
                    ? "Mode aninertiel ACTIVÉ"
                    : "Mode aninertiel DÉSACTIVÉ"
            );
        }
    }
}
