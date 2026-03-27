using UnityEngine;

public class PositionUIElement : MonoBehaviour
{
    public Transform target;
    public Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        transform.position = screenPos;
    }
}
