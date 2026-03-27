using UnityEngine;
using UnityEngine.UI;

public class HealthUIElement : MonoBehaviour
{
    public Transform target;
    public Image fillImage;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    public void setTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public Image getFillImage()
    {
        return fillImage;
    }
    void Update()
    {
        if (target != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            transform.position = screenPos;
        }
    }
}
