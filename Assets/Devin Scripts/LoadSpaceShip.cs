using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSpaceShip : MonoBehaviour
{
    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);

    }

}
