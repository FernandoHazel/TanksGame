using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using System.Runtime.ConstrainedExecution;

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
    public float turnSensitivity = 1f;
    public float maxSteerAngle = 30f;
    public float steerSpeed = 0.3f;
    public Vector3 centerOfMass;
    public List<Wheel> wheels;

    private Rigidbody carRb;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => move =
    ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => move = Vector2.zero;
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
        Move();
        Steer();
    }

    private void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = move.y * maxAcceleration;
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
