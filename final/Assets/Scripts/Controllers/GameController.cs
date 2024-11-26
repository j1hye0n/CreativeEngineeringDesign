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
        HandleStressLevelUpdate();

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
    }

    private void HandleStressLevelUpdate()
    {
        // Get StressLevel from ClientSocket
        int stressLevel = FindObjectOfType<ClientSocket>()?.StressLevel ?? 0;

        // Check if StressLevel has changed
        if (stressLevel != lastStressLevel)
        {
            lastStressLevel = stressLevel;
            Debug.Log($"StressLevel updated in GameController: {stressLevel}");

            if (currentScene is ChooseScene chooseScene)
            {
                chooseScene.UpdateStressLevel(stressLevel);  // Sync StressLevel with ChooseScene
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
