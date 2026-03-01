using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [System.Serializable]
    public struct WheelInfo
    {
        public WheelCollider collider;
        public Transform visualMesh;
    }

    [Header("Ruedas")]
    public List<WheelInfo> frontWheels;
    public List<WheelInfo> backWheels;
    public Transform centerOfMass;

    [Header("Ajustes")]
    public float motorForce = 20000f;
    public float brakeForce = 7000f;
    public float maxSteerAngle = 60f;
    [Range(0.5f, 1.0f)] public float driftFactor = 0.85f;

    private Vector2 moveInput;
    private bool isBraking;
    private Rigidbody rb;


    [Header("Input System Setup")]
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction brakeAction;

    void OnEnable()
    {
        moveAction = inputActions.FindActionMap("Player").FindAction("Move");
        brakeAction = inputActions.FindActionMap("Player").FindAction("Brake");

        moveAction.Enable();
        brakeAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        brakeAction.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass) rb.centerOfMass = centerOfMass.localPosition;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        isBraking = brakeAction.ReadValue<float>() > 0.1f;
    }

    void FixedUpdate()
    {
        ApplyPhysics();
        UpdateAllVisuals();
    }

    private void ApplyPhysics()
    {
        foreach (var wheel in frontWheels)
        {
            wheel.collider.motorTorque = moveInput.y * motorForce;
            wheel.collider.steerAngle = moveInput.x * maxSteerAngle;
            wheel.collider.brakeTorque = isBraking ? brakeForce : 0f;
        }

        foreach (var wheel in backWheels)
        {
            wheel.collider.brakeTorque = isBraking ? brakeForce : 0f;

            // Lógica de Drift en ruedas traseras
            WheelFrictionCurve f = wheel.collider.sidewaysFriction;
            f.stiffness = (Mathf.Abs(moveInput.x) > 0.5f && rb.linearVelocity.magnitude > 5f) ? driftFactor : 2.0f;
            wheel.collider.sidewaysFriction = f;
        }
    }

    private void UpdateAllVisuals()
    {
        foreach (var wheel in frontWheels) UpdateWheelMesh(wheel);
        foreach (var wheel in backWheels) UpdateWheelMesh(wheel);
    }

    private void UpdateWheelMesh(WheelInfo wheel)
    {
        Vector3 pos;
        Quaternion rot;
        // Obtenemos la posición física del WheelCollider y se la pasamos al Mesh
        wheel.collider.GetWorldPose(out pos, out rot);
        wheel.visualMesh.position = pos;
        wheel.visualMesh.rotation = rot;
    }
}