using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [Tooltip("이 NPC의 대화 데이터 SO. 대화 시작 시 DialogueManager에 전달됨")]
    [SerializeField] private DialogueData dialogue;

    [Tooltip("대화 패널 오른쪽에 표시될 NPC 초상화 스프라이트")]
    [SerializeField] private Sprite npcPortrait;

    [Tooltip("상호작용 인디케이터('E')가 NPC 위에 표시될 오프셋 위치")]
    [SerializeField] private Vector2 indicatorOffset = new Vector2(0f, 1.5f);

    [Tooltip("커스텀 상호작용 인디케이터. 비워두면 기본 'E' 표시가 자동 생성됨")]
    [SerializeField] private GameObject interactIndicator;

    private InputAction interactAction;
    private bool playerInRange;
    private DialogueData currentDialogue;
    private bool waitOneFrame;
    private GameObject defaultIndicator;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Start()
    {
        if (interactIndicator == null)
        {
            defaultIndicator = CreateDefaultIndicator();
            defaultIndicator.SetActive(false);
        }
        else
        {
            interactIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (DialogueManager.Instance == null) return;

        if (DialogueManager.Instance.IsDialogueActive)
        {
            waitOneFrame = true;
            return;
        }

        if (waitOneFrame)
        {
            waitOneFrame = false;
            ShowInteractIndicator(true);
            return;
        }

        if (interactAction == null || !interactAction.WasPressedThisFrame()) return;

        StartDialogue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        ShowInteractIndicator(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        ShowInteractIndicator(false);
    }

    private void StartDialogue()
    {
        if (dialogue == null) return;

        if (currentDialogue == null)
            currentDialogue = dialogue;

        DialogueManager.Instance.StartDialogue(currentDialogue, npcPortrait);
        ShowInteractIndicator(false);

        if (currentDialogue.nextDialogue != null)
            currentDialogue = currentDialogue.nextDialogue;
    }

    private void ShowInteractIndicator(bool show)
    {
        var target = interactIndicator != null ? interactIndicator : defaultIndicator;
        if (target != null)
            target.SetActive(show);
    }

    private GameObject CreateDefaultIndicator()
    {
        var root = new GameObject("InteractIndicator");
        root.transform.SetParent(transform);
        root.transform.localPosition = (Vector3)indicatorOffset;
        root.transform.localScale = Vector3.one;

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingLayerName = "Player";
        canvas.sortingOrder = 99;

        var canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = Vector2.zero;
        canvasRect.localScale = Vector3.one * 0.01f;

        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(root.transform, false);

        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);

        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(50, 50);
        bgRect.anchoredPosition = Vector2.zero;

        var textObj = new GameObject("KeyText");
        textObj.transform.SetParent(bgObj.transform, false);

        var text = textObj.AddComponent<Text>();
        text.text = "E";
        text.fontSize = 32;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        return root;
    }
}
