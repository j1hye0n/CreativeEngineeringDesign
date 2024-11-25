using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseController : MonoBehaviour
{
    public ChooseLabelController label;
    public GameController gameController;
    public ChooseScene chooseScene;  // Reference to ChooseScene to update StressLevel
    private RectTransform rectTransform;
    private Animator animator;
    private float labelHeight = -1;

    void Start()
    {
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();

        // Ensure there's a valid reference to ChooseScene (if not assigned via Inspector, find it dynamically)
        if (chooseScene == null)
        {
            chooseScene = Resources.Load<ChooseScene>("PathToChooseScene"); // Adjust path as needed
        }
    }

    // Dynamically update StressLevel during runtime
    public void UpdateStressLevel(int newStressLevel)
    {
        if (chooseScene != null)
        {
            // Update StressLevel and refresh choices
            chooseScene.UpdateStressLevel(newStressLevel);

            int visibleChoices = chooseScene.GetVisibleChoices();
            Debug.Log($"Updated visible choices: {visibleChoices}");

            SetupChoose(chooseScene);  // Refresh the displayed choices
        }
    }

    public void SetupChoose(ChooseScene scene)
    {
        DestroyLabels();
        animator.SetTrigger("Show");

        int visibleChoices = scene.GetVisibleChoices();
        if (visibleChoices > scene.labels.Count)
        {
            Debug.LogError($"Visible choices ({visibleChoices}) exceed available labels ({scene.labels.Count}). Adjusting visible choices to fit available labels.");
            visibleChoices = scene.labels.Count; // Fallback to avoid out-of-bounds errors
        }

        Debug.Log($"Setting up {visibleChoices} choices based on StressLevel: {scene.StressLevel}");

        for (int index = 0; index < visibleChoices; index++)
        {
            ChooseLabelController newLabel = Instantiate(label.gameObject, transform).GetComponent<ChooseLabelController>();

            if (labelHeight == -1)
            {
                labelHeight = newLabel.GetHeight();
            }

            newLabel.Setup(scene.labels[index], this, CalculateLabelPosition(index, visibleChoices));
        }

        Vector2 size = rectTransform.sizeDelta;
        size.y = (visibleChoices + 2) * labelHeight;  // Adjust the size based on the visible choices
        rectTransform.sizeDelta = size;
    }

    public void PerformChoose(StoryScene scene)
    {
        gameController.PlayScene(scene);  // Trigger the GameController to load the chosen StoryScene
        animator.SetTrigger("Hide");
    }

    private float CalculateLabelPosition(int labelIndex, int labelCount)
    {
        // Calculate vertical label positions
        if (labelCount % 2 == 0)
        {
            if (labelIndex < labelCount / 2)
            {
                return labelHeight * (labelCount / 2 - labelIndex - 1) + labelHeight / 2;
            }
            else
            {
                return -1 * (labelHeight * (labelIndex - labelCount / 2) + labelHeight / 2);
            }
        }
        else
        {
            if (labelIndex < labelCount / 2)
            {
                return labelHeight * (labelCount / 2 - labelIndex);
            }
            else if (labelIndex > labelCount / 2)
            {
                return -1 * (labelHeight * (labelIndex - labelCount / 2));
            }
            else
            {
                return 0;  // Center position for the middle label
            }
        }
    }

    private void DestroyLabels()
    {
        foreach (Transform childTransform in transform)
        {
            Destroy(childTransform.gameObject);  // Destroy all existing labels to prevent duplication
        }
    }
}
