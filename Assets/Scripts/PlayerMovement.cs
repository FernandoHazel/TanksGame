using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private float movementX;
    private float movementY;

    [Tooltip("The speed in unity unit/second the tank move at")]
    public float speed = 12f;                 // How fast the tank moves forward and back.
    [Tooltip("The speed in deg/s that tank will rotate at")]
    public float turnSpeed = 180f;            // How fast the tank turns in degrees per second.

    public bool m_IsDirectControl;
    public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
    public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
    public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
    public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

    [Tooltip("Is set to true this will be controlled by the computer and not a player")]
    public bool m_IsComputerControlled = false; // Is this tank player or computer controlled

    private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
    private string m_TurnAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private float m_MovementInputValue;         // The current value of the movement input.
    private float m_TurnInputValue;             // The current value of the turn input.
    private Vector3 m_ExplosionForceValue;      // The current value of the force  applied on the tank from an explosion.
    private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
    private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks

    private InputAction m_MoveAction;             // The InputAction used to move, retrieved from TankInputUser
    private InputAction m_TurnAction;             // The InputAction used to shot, retrieved from TankInputUser

    private Vector3 m_RequestedDirection;       // In Direct Control mode, store the direction the user *wants* to go toward

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);
    }
}
