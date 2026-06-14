using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SaveBootstrapScene
{
    public static void Execute()
    {
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
    }
}
