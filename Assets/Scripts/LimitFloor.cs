using UnityEngine;
using UnityEngine.SceneManagement;

public class LimitFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (other.CompareTag("Beam"))
        {
            other.gameObject.SetActive(false);
        }

        if (other.CompareTag("Floor"))
        {
            other.gameObject.SetActive(false);
        }
    }
}
