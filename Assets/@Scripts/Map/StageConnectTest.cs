using UnityEngine;
using UnityEngine.SceneManagement;
public class StageConnectTest : MonoBehaviour
{
    public void NextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
