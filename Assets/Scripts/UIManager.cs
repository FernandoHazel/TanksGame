using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField]
    private GameObject mobileControls;
    [SerializeField]
    private GameObject sight;
    [SerializeField]
    private GameObject winScreen;
    [SerializeField]
    private GameObject looseScreen;
    
    void Start()
    {
        if (Instance == null)
            Instance = this;

        // Verificar plataforma y mostrar canvas acorde.
    }

    public void ActivateWinScreen()
    {
        DisableChilds();
        winScreen.SetActive(true);
    }

    public void ActivateLooseScreen()
    {
        DisableChilds();
        looseScreen.SetActive(true);
    }

    public void LoadActiveScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevelScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name + 1);
    }

    private void DisableChilds()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
