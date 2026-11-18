using UnityEngine;

public class MainMenuScreenController : MonoBehaviour
{
    [SerializeField] string gameSceneName = "DemoScene"; // your existing scene name
    public void OnClickStartGame()
    {
        SceneLoader.Init(this);                // if not already called
        SceneLoader.LoadGame(gameSceneName);   // async load with a loading UI
    }
}
