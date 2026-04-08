using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    public Line[] lines;
    public float typingSpeed = 30f;
    public DialogueData nextDialogue;

    [System.Serializable]
    public class Line
    {
        public Speaker speaker;
        [TextArea(2, 5)] public string text;
    }

    public enum Speaker { NPC, Player }
}
