using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChooseScene", menuName = "Data/New Choose Scene")]
[System.Serializable]
public class ChooseScene : GameScene
{
    public List<ChooseLabel> labels;

    [System.Serializable]
    public struct ChooseLabel
    {
        public string text;
        public StoryScene nextScene;
    }

    public int StressLevel = 0;  // Default value, dynamically updated

    // Update StressLevel at runtime
    public void UpdateStressLevel(int newStressLevel)
    {
        StressLevel = newStressLevel;
        Debug.Log($"StressLevel updated to {StressLevel} for ChooseScene {name}");
    }

    // Get number of visible choices based on StressLevel
    public int GetVisibleChoices()
    {
        int visibleChoices = StressLevel >= 50 ? 3 : 2;
        Debug.Log($"StressLevel: {StressLevel}, Visible Choices: {visibleChoices}");

        // Ensure visible choices do not exceed available labels
        if (visibleChoices > labels.Count)
        {
            Debug.LogWarning($"Visible choices ({visibleChoices}) exceed available labels ({labels.Count}). Adjusting.");
            visibleChoices = labels.Count;
        }

        return visibleChoices;
    }
}
