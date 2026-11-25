using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerMovement;

public class PlayerMovement : MonoBehaviour
{
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

    PlayerControls controls;
    Vector2 move;
    public float maxAcceleration = 30f;
    public float brakeTorque = 1000f;
    public float turnSensitivity = 1f;
    public float maxSteerAngle = 30f;
    public float steerSpeed = 0.3f;
    public Vector3 centerOfMass;
    public List<Wheel> wheels;

    private Rigidbody carRb;

    private void Awake()
    {
        controls = new PlayerControls();

        // Set up input callbacks for steering
        controls.Player.Move.performed += ctx => move =
    ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => move = Vector2.zero;

        // Set up input callbacks for acceleration and braking
        controls.Player.Accelerate.performed += ctx => Accelerate();
        controls.Player.Accelerate.canceled += ctx => Brake();
        controls.Player.Brake.performed += ctx => Reverse();
        controls.Player.Brake.canceled += ctx => Brake();
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
    }

    private void Update()
    {
        AnimateWheels();
    }

    private void FixedUpdate()
    {
        Steer();
    }

    private void Accelerate()
    {
        Debug.Log("Accelarating");
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = maxAcceleration;
            wheel.wheelCollider.brakeTorque = 0f;
        }
    }

    private void Brake()
    {
        Debug.Log("Braking");
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = 0f;
            wheel.wheelCollider.brakeTorque = brakeTorque;
        }
    }

    private void Reverse()
    {
        Debug.Log("Reverse");
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = - maxAcceleration;
            wheel.wheelCollider.brakeTorque = 0f;
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
