using UnityEngine;

/// <summary>
/// El GameManager es el único que accede al UIManager para evitar tener ese
/// código esparcido por todo el juego
/// </summary>

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Start()
    {
        if (Instance == null)
        Instance = this;

        Time.timeScale = 1f;
    }

    public void Win()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ActivateWinScreen();
        Time.timeScale = 0f;
    }

    public void Loose()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ActivateLooseScreen();
        Time.timeScale = 0f;
    }
}
