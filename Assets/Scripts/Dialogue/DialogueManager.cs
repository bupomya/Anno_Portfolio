using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public bool IsDialogueActive { get; private set; }

    [Header("Player Info")]
    [Tooltip("대화 패널에 표시될 플레이어 이름")]
    [SerializeField] private string playerName = "???";

    [Tooltip("대화 패널 왼쪽에 표시될 플레이어 초상화 스프라이트")]
    [SerializeField] private Sprite playerPortrait;

    private DialoguePanel panel;
    private InputAction interactAction;
    private DialogueData currentDialogue;
    private PlayerBase player;
    private int currentLineIndex;
    private bool lineComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        interactAction = InputSystem.actions.FindAction("Interact");

        panel = GetComponentInChildren<DialoguePanel>();
        if (panel == null)
            panel = gameObject.AddComponent<DialoguePanel>();
    }

    private void Update()
    {
        if (!IsDialogueActive) return;

        bool pressed = (interactAction != null && interactAction.WasPressedThisFrame())
                     || Mouse.current?.leftButton.wasPressedThisFrame == true;
        if (!pressed) return;

        if (panel.IsTyping)
        {
            panel.CompleteTyping();
            return;
        }

        if (lineComplete)
            AdvanceLine();
    }

    public void StartDialogue(DialogueData dialogue, Sprite npcPortrait)
    {
        if (IsDialogueActive) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        IsDialogueActive = true;

        player = FindAnyObjectByType<PlayerBase>();
        if (player != null)
            player.InputLocked = true;

        panel.Show(playerPortrait, npcPortrait);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        lineComplete = false;
        var line = currentDialogue.lines[currentLineIndex];

        string name = line.speaker == DialogueData.Speaker.NPC
            ? currentDialogue.speakerName
            : playerName;

        panel.ShowLine(name, line.text, line.speaker, currentDialogue.typingSpeed, () => lineComplete = true);
    }

    private void AdvanceLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        panel.Hide();

        if (player != null)
            player.InputLocked = false;

        currentDialogue = null;
        player = null;
    }
}
