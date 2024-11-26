using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameScene currentScene;  // Current scene being played
    public BottomBarController bottomBar;
    public SpriteSwitcher backgroundController;
    public ChooseController chooseController;

    private State state = State.IDLE;
    private int lastStressLevel = -1;  // Keep track of the last StressLevel

    private enum State
    {
        IDLE, ANIMATE, CHOOSE
    }

    void Start()
    {
        if (currentScene is StoryScene)
        {
            StoryScene storyScene = currentScene as StoryScene;
            bottomBar.PlayScene(storyScene);
            backgroundController.SetImage(storyScene.background);

            // Send TurnonOutput if required
            if (storyScene.TurnonOutput)
            {
                Debug.Log("TurnonOutput is enabled for this StoryScene.");
                ClientSocket clientSocket = FindObjectOfType<ClientSocket>();
                if (clientSocket != null)
                {
                    clientSocket.TurnonOutput = true;
                    clientSocket.SendTurnonOutputAsync();
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (state == State.IDLE && bottomBar.IsCompleted())
            {
                if (bottomBar.IsLastSentence())
                {
                    // Trigger scene transition
                    PlayScene((currentScene as StoryScene).nextScene);
                }
                else
                {
                    bottomBar.PlayNextSentence();
                }
            }
        }

        // Handle data received from Python
        HandlePythonData();
    }

    private void HandlePythonData()
    {
        // Get StressLevel from ClientSocket
        int stressLevel = FindObjectOfType<ClientSocket>()?.StressLevel ?? 0;

        // Log the StressLevel
        Debug.Log($"Handling StressLevel: {stressLevel} in GameController.");

        // Check if StressLevel has changed
        if (stressLevel != lastStressLevel)
        {
            Debug.Log($"StressLevel changed from {lastStressLevel} to {stressLevel}.");
            lastStressLevel = stressLevel;

            // Transition logic based on StressLevel
            if (currentScene is ChooseScene)
            {
                ChooseScene chooseScene = currentScene as ChooseScene;
                chooseController.UpdateStressLevel(stressLevel);  // Update visible choices based on StressLevel
                Debug.Log($"Updated ChooseScene choices based on StressLevel: {stressLevel}");
            }
        }
    }

    public void PlayScene(GameScene scene)
    {
        if (scene == null)
        {
            Debug.LogError("Cannot play scene: Scene is null.");
            return;
        }

        Debug.Log($"Switching to scene: {scene.name}");
        StartCoroutine(SwitchScene(scene));
    }

    private IEnumerator SwitchScene(GameScene scene)
    {
        state = State.ANIMATE;
        currentScene = scene;
        bottomBar.Hide();
        yield return new WaitForSeconds(1f);

        if (scene is StoryScene)
        {
            StoryScene storyScene = scene as StoryScene;
            backgroundController.SwitchImage(storyScene.background);
            yield return new WaitForSeconds(1f);
            bottomBar.ClearText();
            bottomBar.Show();
            yield return new WaitForSeconds(1f);
            bottomBar.PlayScene(storyScene);
            state = State.IDLE;

            // Check TurnonOutput for new StoryScene
            if (storyScene.TurnonOutput)
            {
                Debug.Log("TurnonOutput is enabled for this StoryScene.");
                ClientSocket clientSocket = FindObjectOfType<ClientSocket>();
                if (clientSocket != null)
                {
                    clientSocket.TurnonOutput = true;
                    clientSocket.SendTurnonOutputAsync();
                }
            }
        }
        else if (scene is ChooseScene)
        {
            state = State.CHOOSE;
            chooseController.SetupChoose(scene as ChooseScene);
        }
    }
}
