using UnityEngine;

public class MIC_CaméraSimple : MonoBehaviour
{
    [Header("Références")]
    public Transform playerBody;
    public Rigidbody rb;

    [Header("Sensibilité")]
    public float mouseSensitivityX = 10f;
    public float mouseSensitivityY = 10f;
    private float sensitivity = 1f;

    [Header("Limites verticales")]
    public float minY = -80f;
    public float maxY = 80f;

    [Header("Dead zone souris")]
    public float mouseDeadZone = 0.01f;

    float xRotation = 0f;
    float yRotation = 0f;

    [Header("Manette")]
    public string rightStickX = "RightStickHorizontal";
    public string rightStickY = "RightStickVertical";
    public float controllerSensitivityX = 200f;
    public float controllerSensitivityY = 200f;
    public float controllerDeadZone = 0.2f;

    private bool wasHoldingLB = false;
    private float lockedYRotation;
    private RigidbodyConstraints originalConstraints;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originalConstraints = rb.constraints;

        // Initialisation propre
        yRotation = playerBody.eulerAngles.y;
    }

    void Update()
    {
        
        // --- INPUT SOURIS ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Dead zone souris
        if (Mathf.Abs(mouseX) < mouseDeadZone) mouseX = 0f;
        if (Mathf.Abs(mouseY) < mouseDeadZone) mouseY = 0f;

        mouseX *= mouseSensitivityX * Time.deltaTime * sensitivity;
        mouseY *= mouseSensitivityY * Time.deltaTime * sensitivity;

        // --- INPUT MANETTE ---
        float stickX = Input.GetAxis(rightStickX);
        float stickY = Input.GetAxis(rightStickY);

        if (Mathf.Abs(stickX) < controllerDeadZone) stickX = 0f;
        if (Mathf.Abs(stickY) < controllerDeadZone) stickY = 0f;

        stickX *= controllerSensitivityX * Time.deltaTime;
        stickY *= -controllerSensitivityY * Time.deltaTime;

        // --- PRIORITÉ MANETTE ---
        float finalX = stickX != 0 ? stickX : mouseX;
        float finalY = stickY != 0 ? stickY : mouseY;

        // Vertical (caméra)
        xRotation -= finalY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal (player)
        yRotation += finalX;
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}