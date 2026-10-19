using UnityEngine;
using System.Collections.Generic;

public class MIC_MovingPlatform_Path_V1 : MonoBehaviour
{
    [Tooltip("La plateforme déplacée")]
    public Rigidbody platformRb;

    [Header("Path Points")]
    public List<Transform> points = new List<Transform>();

    [Header("Movement")]
    public float baseSpeed = 2f;
    public float smoothDuration = 0.3f;
    public float reachDistance = 0.05f;

    [Header("Debug")]
    public bool loop = false;

    private int currentPointIndex = 0;
    private int direction = 1;

    private float currentSpeed;
    private float speedVelocity;

    public Vector3 velocity;
    public Vector3 frameDelta;

    private Vector3 lastPosition;

    private void Awake()
    {
        // -------- AUTO FIND PLATFORM --------
        if (platformRb == null)
        {
            platformRb = GetComponentInChildren<Rigidbody>();
        }

        // -------- SETUP PLATFORM --------
        if (platformRb != null)
        {
            platformRb.isKinematic = true;
            platformRb.useGravity = false;

            lastPosition = platformRb.position;
        }

        // -------- AUTO FIND POINTS --------
        if (points.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Point"))
                {
                    points.Add(child);
                }
            }
        }

        // sécurité
        if (points.Count > 0 && platformRb != null)
        {
            platformRb.position = points[0].position;
        }
    }

    // Appelé à chaque réactivation (via le bouton) : repart proprement.
    private void OnEnable()
    {
        if (platformRb != null)
            lastPosition = platformRb.position;

        currentSpeed = 0f;
        speedVelocity = 0f;
    }

    // Appelé à chaque désactivation : la plateforme s'arrête et
    // les scripts qui lisent velocity (portage du joueur) ne reçoivent plus rien.
    private void OnDisable()
    {
        velocity = Vector3.zero;
        frameDelta = Vector3.zero;
        currentSpeed = 0f;
        speedVelocity = 0f;
    }

    private void FixedUpdate()
    {
        if (platformRb == null)
            return;

        if (points == null || points.Count < 2)
            return;

        Transform targetPoint = points[currentPointIndex];

        currentSpeed = Mathf.SmoothDamp(currentSpeed, baseSpeed, ref speedVelocity, smoothDuration);

        Vector3 newPosition = Vector3.MoveTowards(
            platformRb.position,
            targetPoint.position,
            currentSpeed * Time.fixedDeltaTime);

        // -------- MOVE --------
        platformRb.MovePosition(newPosition);

        // -------- VELOCITY --------
        frameDelta = newPosition - lastPosition;
        velocity = frameDelta / Time.fixedDeltaTime;

        lastPosition = newPosition;

        // -------- NEXT POINT --------
        if (Vector3.Distance(newPosition, targetPoint.position) <= reachDistance)
        {
            currentPointIndex += direction;

            // -------- LOOP MODE --------
            if (loop)
            {
                if (currentPointIndex >= points.Count)
                    currentPointIndex = 0;

                if (currentPointIndex < 0)
                    currentPointIndex = points.Count - 1;
            }
            else
            {
                // -------- PING PONG --------
                if (currentPointIndex >= points.Count)
                {
                    currentPointIndex = points.Count - 2;
                    direction = -1;
                }

                if (currentPointIndex < 0)
                {
                    currentPointIndex = 1;
                    direction = 1;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (points == null || points.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null)
                continue;

            Gizmos.DrawSphere(points[i].position, 0.15f);

            if (i < points.Count - 1 && points[i + 1] != null)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
    }
#endif
}