using System;
using System.Collections.Generic;
using UnityEngine;
using static DriveController;

public class DriveController : MonoBehaviour
{
    // FWD: Only apply motor torque to the front wheels for a front-wheel drive car (normal cars)
    // RWD: Only apply motor torque to the rear wheels for a rear-wheel drive car (sports cars, this allow drifting)
    // AWD: Apply motor torque to all wheels for an all-wheel drive car (off-road vehicles, SUVs)
    public enum DriveType
    {
        FWD,
        RWD,
        AWD
    }
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    [Serializable]
    public struct FrictionSettings
    {
        public float extremumSlip;
        public float extremumValue;
        public float asymptoteSlip;
        public float asymptoteValue;
        public float stiffness;

        public WheelFrictionCurve ToCurve() => new WheelFrictionCurve
        {
            extremumSlip = extremumSlip,
            extremumValue = extremumValue,
            asymptoteSlip = asymptoteSlip,
            asymptoteValue = asymptoteValue,
            stiffness = stiffness
        };
    }

    PlayerControls controls;
    Vector2 move;

    [Tooltip("FWD: Only apply motor torque to the front wheels for a front-wheel drive car (normal cars)\r\n RWD: Only apply motor torque to the rear wheels for a rear-wheel drive car (sports cars, this allow drifting)\r\n AWD: Apply motor torque to all wheels for an all-wheel drive car (off-road vehicles, SUVs)s")]
    public DriveType driveType = DriveType.RWD;
    public float rbMass = 1500f;
    public float maxAcceleration = 30f;
    public float brakeTorque = 1000f;
    public float linearDrag = 0.1f;
    [Tooltip("Soft brake torque applied when there is no input")]
    public float coastBrakeTorque = 150f;
    [Tooltip("Speed (m/s) below which the car is considered stopped")]
    public float stopSpeedThreshold = 0.5f;
    [Tooltip("Simulated air resistance, assigned to Rigidbody.linearDamping")]
    public float turnSensitivity = 1f;
    public float maxSteerAngle = 30f;
    public float steerSpeed = 0.3f;
    public Vector3 centerOfMass;
    public List<Wheel> wheels;

    private Rigidbody carRb;
    private bool isAccelerating;
    private bool isBraking;

    [Header("Wheels Friction curves")]
    public FrictionSettings forwardFriction = new FrictionSettings
    {
        extremumSlip = 0.4f,
        extremumValue = 1f,
        asymptoteSlip = 0.8f,
        asymptoteValue = 0.5f,
        stiffness = 1f
    };

    public FrictionSettings sidewaysFriction = new FrictionSettings
    {
        extremumSlip = 0.2f,
        extremumValue = 1f,
        asymptoteSlip = 0.5f,
        asymptoteValue = 0.75f,
        stiffness = 1f
    };

    // TEMP: For testing or youtube video, remove later
    [Header("TEMP: For testing or youtube video, remove later")]
    public GameObject accelerateImage;
    public GameObject decelerateImage;
    public GameObject brakingImage;
    public GameObject releaseBrakeImage;

    private void Awake()
    {
        controls = new PlayerControls();

        // Set up input callbacks for steering
        controls.Player.Move.performed += ctx => move =
    ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => move = Vector2.zero;

        // Set up input callbacks for acceleration and braking
        controls.Player.Accelerate.performed += ctx => isAccelerating = true;
        controls.Player.Accelerate.canceled += ctx => isAccelerating = false;
        controls.Player.Brake.performed += ctx => isBraking = true;
        controls.Player.Brake.canceled += ctx => isBraking = false;
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = centerOfMass;
        carRb.mass = rbMass;
        carRb.linearDamping = linearDrag;
        setupWheelPhysics();

        accelerateImage.SetActive(false);
        decelerateImage.SetActive(true);
        brakingImage.SetActive(false);
        releaseBrakeImage.SetActive(true);
    }

    private void Update()
    {
        AnimateWheels();
    }

    private void FixedUpdate()
    {
        UpdateDrive();
        Steer();
    }

    private void setupWheelPhysics()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.forwardFriction = forwardFriction.ToCurve();
            wheel.wheelCollider.sidewaysFriction = sidewaysFriction.ToCurve();

        }
    }

    private void UpdateDrive()
    {
        if (wheels == null || wheels.Count == 0) return;

        float forwardSpeed = Vector3.Dot(carRb.linearVelocity, transform.forward);

        if (isBraking && forwardSpeed > stopSpeedThreshold)
        {
            accelerateImage.SetActive(false);
            decelerateImage.SetActive(true);
            brakingImage.SetActive(true);
            releaseBrakeImage.SetActive(false);

            // Moving forward: brake
            SetDriveTorque(0f, brakeTorque);
        }
        else if (isBraking)
        {
            accelerateImage.SetActive(false);
            decelerateImage.SetActive(true);
            brakingImage.SetActive(true);
            releaseBrakeImage.SetActive(false);

            // Stopped or moving backward: reverse
            SetDriveTorque(-maxAcceleration, 0f);
        }
        else if (isAccelerating)
        {
            accelerateImage.SetActive(true);
            decelerateImage.SetActive(false);
            brakingImage.SetActive(false);
            releaseBrakeImage.SetActive(true);

            SetDriveTorque(maxAcceleration, 0f);
        }
        else if (Mathf.Abs(forwardSpeed) < stopSpeedThreshold)
        {
            accelerateImage.SetActive(false);
            decelerateImage.SetActive(true);
            brakingImage.SetActive(false);
            releaseBrakeImage.SetActive(true);

            // No input and nearly stopped: hold the car
            SetDriveTorque(0f, brakeTorque);
        }
        else
        {
            accelerateImage.SetActive(false);
            decelerateImage.SetActive(true);
            brakingImage.SetActive(false);
            releaseBrakeImage.SetActive(true);

            // No input: coast with soft braking
            SetDriveTorque(0f, coastBrakeTorque);
        }
    }

    private bool IsDriveWheel(Wheel wheel)
    {
        return driveType == DriveType.AWD
            || (driveType == DriveType.FWD && wheel.axel == Axel.Front)
            || (driveType == DriveType.RWD && wheel.axel == Axel.Rear);
    }

    private void SetDriveTorque(float torque, float brake)
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.brakeTorque = brake;
            wheel.wheelCollider.motorTorque = IsDriveWheel(wheel) ? torque : 0f;
        }
    }

    private void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var steerAngle = move.x * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, steerSpeed);
            }
        }
    }

    private void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}
