using UnityEngine;
using UnityEngine.SceneManagement;

public class LimitFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.Loose();
        }

        if (other.CompareTag("Enemy-car"))
        {
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Beam"))
        {
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Floor"))
        {
            Destroy(other.gameObject);
        }
    }
}
