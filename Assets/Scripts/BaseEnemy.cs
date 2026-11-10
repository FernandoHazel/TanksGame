using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public static int enemyCount = 0;
    void Start()
    {
        enemyCount++;
    }

    private void OnDestroy()
    {
        enemyCount--;
        if(enemyCount <= 0)
        {
            GameManager.Instance.Win();
        }
    }
}
