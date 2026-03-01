using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;
    public Rigidbody targetRB;

    [Header("Posicionamiento")]
    public Vector3 offset = new Vector3(0, 3, -6);
    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;

    [Header("Efecto de Velocidad")]
    public Camera cam;
    public float minFOV = 60f;
    public float maxFOV = 80f;
    public float speedFactor = 3f;

    void FixedUpdate()
    {
        if (!target || !targetRB) return;

        // 1. Seguir posición con suavizado
        Vector3 targetPos = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, positionSmooth * Time.fixedDeltaTime);

        // 2. Mirar siempre al coche
        Quaternion targetRot = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmooth * Time.fixedDeltaTime);

        // 3. Dinámica de FOV (Campo de visión) según velocidad
        float speed = targetRB.linearVelocity.magnitude * 3.6f; // km/h
        float desiredFOV = Mathf.Lerp(minFOV, maxFOV, speed / 150f);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, Time.fixedDeltaTime * 2f);
    }
}