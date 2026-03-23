using Tanks.Complete;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class TurrerController : MonoBehaviour
{
    //TODO:
    //Rotation
    //Shooting
    //Particle effect
    //Shooting sound effect

    PlayerControls controls;
    Vector2 aim;
    public float rotationSpeed = 80f;
    public Rigidbody m_Shell;                   // Prefab of the shell.
    public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
    public AudioSource m_ShootingAudio;       // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
    public AudioClip m_ChargingClip;          // Audio that plays when each shot is charging up.
    public AudioClip m_FireClip;              // Audio that plays when each shot is fired.
    public ParticleSystem m_FireSystem;       // Particle system played when each shot is fired.
    [Tooltip("The speed in unit/second the shell have when fired at minimum charge")]
    public float m_MinLaunchForce = 5f;        // The force given to the shell if the fire button is not held.
    [Tooltip("The speed in unit/second the shell have when fired at max charge")]
    public float m_MaxLaunchForce = 20f;        // The force given to the shell if the fire button is held for the max charge time.
    [Tooltip("The maximum time spent charging. When charging reach that time, the shell is fired at MaxLaunchForce")]
    public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
    [Tooltip("The time that must pass before being able to shoot again after a shot")]
    public float m_ShotCooldown = 1.0f;         // The time required between 2 shots
    [Header("Shell Properties")]
    [Tooltip("The amount of health removed to a tank if they are exactly on the landing spot of a shell")]
    public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
    [Tooltip("The force of the explosion at the shell position. Keep it 50 and below")]
    public float m_ExplosionForce = 50f;              // The amount of force added to a tank at the centre of the explosion.
    [Tooltip("The radius of the explosion in Unity unit. Force decrease with distance to the center, and an tank further than this from the shell explosion won't be impacted by the explosion")]
    public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.
    public float cooldownTimer = 1f;                      // A timer to keep track of the cooldown between shots
    private bool canShoot = true;                      // Whether or not the tank can shoot. This is false when the tank is on cooldown after shooting, and true otherwise.
    public float CurrentChargeRatio =>
            (m_CurrentLaunchForce - m_MinLaunchForce) / (m_MaxLaunchForce - m_MinLaunchForce); //The charging amount between 0-1
    public bool IsCharging => m_IsCharging;

    public bool m_IsComputerControlled { get; set; } = false;

    private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
    private bool m_HasSpecialShell;             // has the tank a shell that makes extra damage?
    private float m_SpecialShellMultiplier;     // The amount that the special shell will multiply the damage.
    private bool m_IsCharging = false;          // Are we currently charging the shot

    private void Awake()
    {
        // Aim the turret
        controls = new PlayerControls();
        controls.Player.Aim.performed += ctx => aim =
    ctx.ReadValue<Vector2>();
        controls.Player.Aim.canceled += ctx => aim = Vector2.zero;

        // Shoot
        controls.Player.Shoot.performed += ctx => Fire();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        // When the tank is turned on, reset the launch force, the UI and the power ups
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_HasSpecialShell = false;
        m_SpecialShellMultiplier = 1.0f;
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    { 
        // Rotación en eje Y (izquierda/derecha)
        transform.Rotate(Vector3.up * aim.x * rotationSpeed * Time.deltaTime);

        // Rotación en eje X (arriba/abajo)
        transform.Rotate(Vector3.right * -aim.y * rotationSpeed * Time.deltaTime);
    }

    private void Fire()
    {
        if (!canShoot)
            return;

        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance =
            Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        // Set the shell's velocity to the launch force in the fire position's forward direction.
        shellInstance.linearVelocity = m_CurrentLaunchForce * m_FireTransform.forward;

        ShellExplosion explosionData = shellInstance.GetComponent<ShellExplosion>();
        explosionData.m_ExplosionForce = m_ExplosionForce;
        explosionData.m_ExplosionRadius = m_ExplosionRadius;
        explosionData.m_MaxDamage = m_MaxDamage;

        // Increase the damage if extra damage PowerUp is active
        if (m_HasSpecialShell)
        {
            explosionData.m_MaxDamage *= m_SpecialShellMultiplier;
            // Reset the default values after increasing the damage of the fired shell
            m_HasSpecialShell = false;
            m_SpecialShellMultiplier = 1f;

            PowerUpDetector powerUpDetector = GetComponent<PowerUpDetector>();
            if (powerUpDetector != null)
                powerUpDetector.m_HasActivePowerUp = false;

            PowerUpHUD powerUpHUD = GetComponentInChildren<PowerUpHUD>();
            if (powerUpHUD != null)
                powerUpHUD.DisableActiveHUD();
        }

        // Change the clip to the firing clip and play it.
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
        m_FireSystem.Play();
        canShoot = false;

        // Reset the launch force.  This is a precaution in case of missing button events.
        m_CurrentLaunchForce = m_MinLaunchForce;

        // Wait before shooting again
        StartCoroutine(chargeNewShot());
    }

    IEnumerator chargeNewShot()
    {
        yield return new WaitForSeconds(cooldownTimer);
        canShoot = true;
    }
}
