using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TestSceneLoad : MonoBehaviour
{
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
