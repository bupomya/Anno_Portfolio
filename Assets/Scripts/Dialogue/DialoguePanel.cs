using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialoguePanel : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("대화 텍스트에 사용할 폰트. 비워두면 기본 폰트 사용")]
    [SerializeField] private Font font;

    [Tooltip("화면 하단 대화 패널의 높이(px, 기준 해상도 1920x1080)")]
    [SerializeField] private float panelHeight = 180f;

    [Tooltip("캐릭터 초상화 이미지의 크기(px)")]
    [SerializeField] private float portraitSize = 130f;

    private Canvas canvas;
    private GameObject panelObj;
    private Image playerPortraitImage;
    private Image npcPortraitImage;
    private Text speakerNameText;
    private Text dialogueText;

    private Tweener typingTween;
    private bool isTyping;
    private System.Action onTypingComplete;

    public bool IsTyping => isTyping;

    private void Awake()
    {
        CreatePanelUI();
        panelObj.SetActive(false);
    }

    private void CreatePanelUI()
    {
        // Screen Space Overlay Canvas
        var canvasObj = new GameObject("DialogueCanvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Panel (bottom, full width with margin)
        panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(-40f, panelHeight);
        panelRect.anchoredPosition = new Vector2(0, 20f);

        // Player portrait (left)
        playerPortraitImage = CreatePortrait("PlayerPortrait", true);

        // NPC portrait (right)
        npcPortraitImage = CreatePortrait("NPCPortrait", false);

        // Text container (between portraits)
        var textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(panelObj.transform, false);

        var containerRect = textContainer.AddComponent<RectTransform>();
        float textMargin = portraitSize + 40f;
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = new Vector2(textMargin, 15f);
        containerRect.offsetMax = new Vector2(-textMargin, -15f);

        // Speaker name
        var nameObj = new GameObject("SpeakerName");
        nameObj.transform.SetParent(textContainer.transform, false);

        speakerNameText = nameObj.AddComponent<Text>();
        speakerNameText.fontSize = 24;
        speakerNameText.fontStyle = FontStyle.Bold;
        speakerNameText.color = new Color(1f, 0.85f, 0.4f);
        speakerNameText.alignment = TextAnchor.UpperLeft;
        speakerNameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        ApplyFont(speakerNameText);

        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.7f);
        nameRect.anchorMax = Vector2.one;
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // Dialogue text
        var textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(textContainer.transform, false);

        dialogueText = textObj.AddComponent<Text>();
        dialogueText.fontSize = 22;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAnchor.UpperLeft;
        dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogueText.verticalOverflow = VerticalWrapMode.Overflow;
        ApplyFont(dialogueText);

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = new Vector2(1, 0.65f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private Image CreatePortrait(string objName, bool isLeft)
    {
        var obj = new GameObject(objName);
        obj.transform.SetParent(panelObj.transform, false);

        var image = obj.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.4f);
        image.preserveAspect = true;

        var rect = obj.GetComponent<RectTransform>();
        if (isLeft)
        {
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(20f, 0);
        }
        else
        {
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-20f, 0);
        }
        rect.sizeDelta = new Vector2(portraitSize, portraitSize);

        return image;
    }

    private void ApplyFont(Text text)
    {
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public void Show(Sprite playerPortrait, Sprite npcPortrait)
    {
        if (playerPortrait != null)
            playerPortraitImage.sprite = playerPortrait;
        if (npcPortrait != null)
            npcPortraitImage.sprite = npcPortrait;

        panelObj.SetActive(true);
    }

    public void ShowLine(string speakerName, string text, DialogueData.Speaker speaker, float typingSpeed, System.Action onComplete)
    {
        speakerNameText.text = speakerName;

        // Highlight active speaker, dim the other
        playerPortraitImage.color = speaker == DialogueData.Speaker.Player
            ? Color.white : new Color(1, 1, 1, 0.4f);
        npcPortraitImage.color = speaker == DialogueData.Speaker.NPC
            ? Color.white : new Color(1, 1, 1, 0.4f);

        // Typing effect
        typingTween?.Kill();
        onTypingComplete = onComplete;
        dialogueText.text = "";
        isTyping = true;

        int totalChars = text.Length;
        float duration = totalChars / typingSpeed;

        typingTween = DOTween.To(
            () => 0,
            val => dialogueText.text = text.Substring(0, val),
            totalChars,
            duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isTyping = false;
                onTypingComplete?.Invoke();
            });
    }

    public void CompleteTyping()
    {
        if (!isTyping) return;
        typingTween?.Complete();
    }

    public void Hide()
    {
        typingTween?.Kill();
        isTyping = false;
        if (panelObj != null)
            panelObj.SetActive(false);
    }

    private void OnDestroy()
    {
        typingTween?.Kill();
    }
}
