using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class moves the player using transform
/// </summary>
public class PlayerMovement2 : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    public float speed = 12f;

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

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(move.x, 0.0f, move.y) * speed *
        Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
