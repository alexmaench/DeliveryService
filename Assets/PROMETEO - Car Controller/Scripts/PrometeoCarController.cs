using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{

    [Space(20)]
    //[Header("CAR SETUP")]
    [Space(10)]
    [Range(20, 190)]
    public int maxSpeed = 90; //The maximum speed that the car can reach in km/h.
    [Range(10, 120)]
    public int maxReverseSpeed = 45; //The maximum speed that the car can reach while going on reverse in km/h.
    [Range(1, 10)]
    public int accelerationMultiplier = 2; // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27; // The maximum angle that the tires can reach while rotating the steering wheel.
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f; // How fast the steering wheel turns.
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350; // The strength of the wheel brakes.
    [Range(1, 10)]
    public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.
    [Space(10)]
    public Vector3 bodyMassCenter; // This is a vector that contains the center of mass of the car. I recommend to set this value
                                   // in the points x = 0 and z = 0 of your car. You can select the value that you want in the y axis,
                                   // however, you must notice that the higher this value is, the more unstable the car becomes.
                                   // Usually the y value goes from 0 to 1.5.


    //[Header("WHEELS")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;


    [Space(20)]
    //[Header("EFFECTS")]
    [Space(10)]
    //The following variable lets you to set up particle systems in your car
    public bool useEffects = false;

    // The following particle systems are used as tire smoke when the car drifts.
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;

    [Space(10)]
    // The following trail renderers are used as tire skids when the car loses traction.
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;


    [Space(20)]
    //[Header("UI")]
    [Space(10)]
    //The following variable lets you to set up a UI text to display the speed of your car.
    public bool useUI = false;
    public Text carSpeedText; // Used to store the UI object that is going to show the speed of the car.

    [Space(20)]
    //[Header("Sounds")]
    [Space(10)]
    //The following variable lets you to set up sounds for your car such as the car engine or tire screech sounds.
    public bool useSounds = false;
    public AudioSource carEngineSound; // This variable stores the sound of the car engine.
    public AudioSource tireScreechSound; // This variable stores the sound of the tire screech (when the car is drifting).
    float initialCarEngineSoundPitch; // Used to store the initial pitch of the car engine sound.


    [HideInInspector]
    public float carSpeed; // Used to store the speed of the car.
    [HideInInspector]
    public bool isDrifting; // Used to know whether the car is drifting or not.
    [HideInInspector]
    public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.


    Rigidbody carRigidbody; // Stores the car's rigidbody.
    float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;

    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;


    [Space(20)]
    //[Header("InputActions")]
    [Space(10)]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction handbrakeAction;

    private Vector2 moveInput;
    private bool isHandbrakePressed;

    private void Awake()
    {
        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        handbrakeAction = playerMap.FindAction("Handbrake");
    }

    void OnEnable()
    {
        moveAction.Enable();
        handbrakeAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        handbrakeAction.Disable();
    }

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        if (useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else if (!useUI)
        {
            if (carSpeedText != null)
            {
                carSpeedText.text = "0";
            }
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null)
            {
                tireScreechSound.Stop();
            }
        }

        if (!useEffects)
        {
            if (RLWParticleSystem != null)
            {
                RLWParticleSystem.Stop();
            }
            if (RRWParticleSystem != null)
            {
                RRWParticleSystem.Stop();
            }
            if (RLWTireSkid != null)
            {
                RLWTireSkid.emitting = false;
            }
            if (RRWTireSkid != null)
            {
                RRWTireSkid.emitting = false;
            }
        }
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        isHandbrakePressed = handbrakeAction.IsPressed();

        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        if (moveInput.y > 0.1f) // W
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            GoForward();
        }
        else if (moveInput.y < -0.1f) // S
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            GoReverse();
        }

        if (moveInput.x < -0.1f) TurnLeft();   // A
        if (moveInput.x > 0.1f) TurnRight();  // D

        if (isHandbrakePressed)
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            Handbrake();
        }

        if (handbrakeAction.WasReleasedThisFrame())
        {
            RecoverTraction();
        }

        if (Mathf.Abs(moveInput.y) < 0.1f)
        {
            ThrottleOff();
        }

        if (Mathf.Abs(moveInput.y) < 0.1f && !isHandbrakePressed && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }

        if (Mathf.Abs(moveInput.x) < 0.1f && steeringAxis != 0f)
        {
            ResetSteeringAngle();
        }

        AnimateWheelMeshes();
    }

    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }
    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f)
        {
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        }
        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }
    void AnimateWheelMeshes()
    {
        try
        {
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void GoForward()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }
    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }

        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");

        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
        {
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f)
        {
            driftingAxis = 1f;
        }

        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }

        if (driftingAxis < 1f)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }

        isTractionLocked = true;
        DriftCarPS();

    }

    public void DriftCarPS()
    {

        if (useEffects)
        {
            try
            {
                if (isDrifting)
                {
                    RLWParticleSystem.Play();
                    RRWParticleSystem.Play();
                }
                else if (!isDrifting)
                {
                    RLWParticleSystem.Stop();
                    RRWParticleSystem.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

            try
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    RLWTireSkid.emitting = true;
                    RRWTireSkid.emitting = true;
                }
                else
                {
                    RLWTireSkid.emitting = false;
                    RRWTireSkid.emitting = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useEffects)
        {
            if (RLWParticleSystem != null)
            {
                RLWParticleSystem.Stop();
            }
            if (RRWParticleSystem != null)
            {
                RRWParticleSystem.Stop();
            }
            if (RLWTireSkid != null)
            {
                RLWTireSkid.emitting = false;
            }
            if (RRWTireSkid != null)
            {
                RRWTireSkid.emitting = false;
            }
        }

    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;
        }
    }
}