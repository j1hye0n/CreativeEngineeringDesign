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

    // Add this variable to control the number of choices shown
    public int stressLevel = 0;  // Default stress level, modify as needed

    // Function to get the number of choices to display based on StressLevel
    public int GetVisibleChoices()
    {
        return stressLevel >= 50 ? 3 : 2;  // Show 3 choices if StressLevel >= 50, otherwise 2
    }
}
