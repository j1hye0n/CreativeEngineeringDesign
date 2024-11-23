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

    public int StressLevel = 0;  // Default value, updated manually or dynamically

    // Method to update StressLevel at runtime
    public void UpdateStressLevel(int newStressLevel)
    {
        StressLevel = newStressLevel;
        Debug.Log($"StressLevel for {name} updated to: {StressLevel}");
    }

    // Function to get the number of choices to display based on StressLevel
    public int GetVisibleChoices()
    {
        return StressLevel >= 50 ? 3 : 2;
    }
}
