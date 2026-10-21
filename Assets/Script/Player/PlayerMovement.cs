using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private Transform cameraTransform;

    [Header("External Forces")]
    [Tooltip("Vitesse à laquelle les poussées horizontales disparaissent.")]
    [SerializeField] private float externalForceDamping = 1.5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Mode aninertiel (vol limité)")]
    [Tooltip("Vitesse de déplacement en mode aninertiel.")]
    [SerializeField] private float aniSpeed = 8f;

    [Tooltip("Énergie maximale du mode aninertiel.")]
    [SerializeField] private float aniMaxEnergy = 100f;

    [Tooltip("Énergie consommée par unité de distance parcourue (distance max = énergie max / ce coût).")]
    [SerializeField] private float aniEnergyCostPerUnit = 5f;

    [Tooltip("Si coché, le joueur se déplace dans la direction de la caméra, hauteur comprise.")]
    [SerializeField] private bool aniUseCameraPitch = false;

    [Tooltip("Bouton pour monter en mode aninertiel (laisser vide pour désactiver).")]
    [SerializeField] private string aniAscendButton = "Jump";

    [Tooltip("Bouton pour descendre en mode aninertiel (laisser vide pour désactiver).")]
    [SerializeField] private string aniDescendButton = "RB";

    [Header("Walls")]
    [Tooltip("Distance de détection des murs devant le joueur.")]
    [SerializeField] private float wallCheckDistance = 0.6f;

    [Tooltip("Couches considérées comme des murs.")]
    [SerializeField] private LayerMask wallLayer = ~0;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool aninertiel;

    private Vector3 savedLinearVelocity;
    private Vector3 savedAngularVelocity;
    private Vector3 savedExternalHorizontalVelocity;
    private bool savedUseGravity;
    private bool savedVelocity;

    // Vitesse horizontale supplémentaire (bumper, par exemple).
    private Vector3 externalHorizontalVelocity;

    private float aniEnergy;

    public bool IsGrounded { get; private set; }
    public bool JustLanded { get; private set; }

    // Pour une jauge d'UI par exemple.
    public float AniEnergy => aniEnergy;
    public float AniEnergyNormalized => aniMaxEnergy > 0f ? aniEnergy / aniMaxEnergy : 0f;

    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        aniEnergy = aniMaxEnergy;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        CheckGround();

        // Recharge : uniquement quand le raycast vers le bas touche le sol.
        // Un mur ne peut donc jamais recharger l'énergie.
        if (IsGrounded)
            aniEnergy = aniMaxEnergy;

        if (aninertiel)
            MoveAni();
        else
            Move();

        if (jumpRequested && IsGrounded && !aninertiel)
            Jump();

        jumpRequested = false;

        ApplyExtraGravity();
    }

    // ---------------------------------------------------------------
    // Mouvement normal
    // ---------------------------------------------------------------
    private void Move()
    {
        Vector3 direction = Vector3.ClampMagnitude(GetInputDirection(false), 1f);

        // Vitesse souhaitée : déplacement normal + poussée du bumper.
        Vector3 desiredVelocity = direction * speed + externalHorizontalVelocity;
        desiredVelocity.y = rb.linearVelocity.y;

        desiredVelocity = BlockWalls(desiredVelocity, true);

        rb.linearVelocity = desiredVelocity;

        // Atténue progressivement la poussée restante.
        externalHorizontalVelocity = Vector3.MoveTowards(
            externalHorizontalVelocity, Vector3.zero, externalForceDamping * Time.fixedDeltaTime);
    }

    // ---------------------------------------------------------------
    // Mouvement en mode aninertiel
    // ---------------------------------------------------------------
    private void MoveAni()
    {
        // Par défaut le joueur est figé sur place (pas de gravité, vitesse nulle).
        Vector3 velocity = Vector3.zero;

        // Sans énergie : il reste figé et ne peut plus se déplacer.
        if (aniEnergy > 0f)
        {
            Vector3 direction = GetInputDirection(aniUseCameraPitch);

            if (!string.IsNullOrEmpty(aniAscendButton) && Input.GetButton(aniAscendButton))
                direction += Vector3.up;

            if (!string.IsNullOrEmpty(aniDescendButton) && Input.GetButton(aniDescendButton))
                direction += Vector3.down;

            direction = Vector3.ClampMagnitude(direction, 1f);

            velocity = direction * aniSpeed;

            // Les murs bloquent le déplacement (et donc ne consomment pas d'énergie).
            velocity = BlockWalls(velocity, false);

            // Consommation proportionnelle à la distance réellement parcourue.
            float distance = velocity.magnitude * Time.fixedDeltaTime;
            aniEnergy = Mathf.Max(0f, aniEnergy - distance * aniEnergyCostPerUnit);
        }

        rb.linearVelocity = velocity;
    }

    // Direction d'entrée du joueur, relative à la caméra.
    private Vector3 GetInputDirection(bool useCameraPitch)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        if (!useCameraPitch)
            cameraForward.y = 0f;

        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        return cameraForward * vertical + cameraRight * horizontal;
    }

    // Supprime la composante horizontale qui pousse dans un mur.
    private Vector3 BlockWalls(Vector3 desiredVelocity, bool cancelExternalVelocity)
    {
        Vector3 horizontalDirection = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);

        if (horizontalDirection.sqrMagnitude <= 0.001f)
            return desiredVelocity;

        horizontalDirection.Normalize();

        Vector3 origin = capsuleCollider.bounds.center;
        float radius = Mathf.Max(0.05f, capsuleCollider.radius * 0.9f);

        if (Physics.SphereCast(origin, radius, horizontalDirection, out RaycastHit hit,
                wallCheckDistance, wallLayer, QueryTriggerInteraction.Ignore))
        {
            // Ne bloque que les surfaces suffisamment verticales.
            if (Mathf.Abs(hit.normal.y) < 0.3f)
            {
                Vector3 horizontalVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
                Vector3 wallNormal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;

                float intoWall = Vector3.Dot(horizontalVelocity, wallNormal);

                if (intoWall < 0f)
                    horizontalVelocity -= wallNormal * intoWall;

                desiredVelocity.x = horizontalVelocity.x;
                desiredVelocity.z = horizontalVelocity.z;

                if (cancelExternalVelocity)
                {
                    // Supprime aussi la poussée du bumper dirigée vers le mur.
                    float externalIntoWall = Vector3.Dot(externalHorizontalVelocity, wallNormal);

                    if (externalIntoWall < 0f)
                        externalHorizontalVelocity -= wallNormal * externalIntoWall;
                }
            }
        }

        return desiredVelocity;
    }

    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ApplyExtraGravity()
    {
        if (aninertiel)
            return;

        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        bool previousGrounded = IsGrounded;

        float rayDistance = capsuleCollider.height / 2f + groundCheckDistance;

        IsGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);

        JustLanded = !previousGrounded && IsGrounded;

        Debug.DrawRay(transform.position, Vector3.down * rayDistance, IsGrounded ? Color.green : Color.red);
    }

    // Appelée par le bumper pour propulser le joueur.
    // La force reçue est exprimée comme une impulsion.
    public void ReceiveBumperForce(Vector3 force)
    {
        if (aninertiel)
            return;

        // Convertit l'impulsion horizontale en vitesse.
        Vector3 horizontalImpulse = new Vector3(force.x, 0f, force.z);

        externalHorizontalVelocity += horizontalImpulse / rb.mass;

        // L'impulsion verticale est gérée par la physique.
        if (force.y != 0f)
        {
            rb.AddForce(Vector3.up * force.y, ForceMode.Impulse);
        }
    }

    public void SetAniState(bool state)
    {
        if (state == aninertiel)
            return;

        if (state)
        {
            // Sauvegarde de l'état physique avant le gel.
            savedLinearVelocity = rb.linearVelocity;
            savedAngularVelocity = rb.angularVelocity;
            savedExternalHorizontalVelocity = externalHorizontalVelocity;
            savedUseGravity = rb.useGravity;

            savedVelocity = true;
            aninertiel = true;

            // Immobilise le personnage sans détruire les valeurs sauvegardées.
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }
        else
        {
            aninertiel = false;

            if (savedVelocity)
            {
                rb.useGravity = savedUseGravity;
                rb.linearVelocity = savedLinearVelocity;
                rb.angularVelocity = savedAngularVelocity;

                externalHorizontalVelocity = savedExternalHorizontalVelocity;

                savedVelocity = false;
            }
        }
    }
}