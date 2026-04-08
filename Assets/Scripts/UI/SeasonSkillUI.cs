using System;
using UnityEngine;
using UnityEngine.UI;

public class SeasonSkillUI : MonoBehaviour
{
    [Serializable]
    public struct SeasonSlotConfig
    {
        [Tooltip("계절 아이콘 스프라이트")]
        public Sprite icon;
    }

    [Header("Slot Prefab")]
    [Tooltip("계절 슬롯 UI 프리팹 (Image 컴포넌트 필수). 이 프리팹을 교체하여 슬롯 외형 변경 가능")]
    [SerializeField] private GameObject slotPrefab;

    [Header("Season Icons")]
    [Tooltip("계절별 아이콘 설정 (SkillManager의 Season Bindings와 동일한 인덱스: 0=봄, 1=여름, 2=가을, 3=겨울)")]
    [SerializeField] private SeasonSlotConfig[] seasonConfigs;

    [Header("References")]
    [Tooltip("PlayerBase 참조 (MP 표시용)")]
    [SerializeField] private PlayerBase player;

    [Header("Layout")]
    [Tooltip("슬롯 크기 (가로 x 세로, 픽셀)")]
    [SerializeField] private Vector2 slotSize = new Vector2(64f, 64f);

    [Tooltip("슬롯 사이 간격 (픽셀)")]
    [SerializeField] private float spacing = 8f;

    [Tooltip("화면 모서리에서 패널까지의 여백 (픽셀)")]
    [SerializeField] private Vector2 margin = new Vector2(20f, 20f);

    [Header("Highlight")]
    [Tooltip("활성 슬롯 테두리 두께 (픽셀)")]
    [SerializeField] private float borderThickness = 4f;

    [Tooltip("활성 슬롯 테두리 색상")]
    [SerializeField] private Color activeBorderColor = new Color(1f, 0.85f, 0.3f, 1f);

    [Header("Colors")]
    [Tooltip("활성화된 계절 아이콘 색상")]
    [SerializeField] private Color activeColor = Color.white;

    [Tooltip("해금되었지만 비활성 상태의 아이콘 색상")]
    [SerializeField] private Color unlockedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

    [Tooltip("잠금 상태의 아이콘 색상")]
    [SerializeField] private Color lockedColor = new Color(0.15f, 0.15f, 0.15f, 0.4f);

    [Header("MP Bar")]
    [Tooltip("MP 바 높이 (픽셀)")]
    [SerializeField] private float mpBarHeight = 12f;

    [Tooltip("스킬 슬롯과 MP 바 사이 간격 (픽셀)")]
    [SerializeField] private float mpBarGap = 6f;

    [Tooltip("MP 바 배경 스프라이트 (null이면 기본 흰색 사용)")]
    [SerializeField] private Sprite mpBarBackgroundSprite;

    [Tooltip("MP 바 배경 색상")]
    [SerializeField] private Color mpBarBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    [Tooltip("MP 바 채움 스프라이트 (Filled 타입에 사용. null이면 Fill이 동작하지 않을 수 있음)")]
    [SerializeField] private Sprite mpBarFillSprite;

    [Tooltip("MP 바 채움 색상")]
    [SerializeField] private Color mpBarFillColor = new Color(0.3f, 0.5f, 1f, 1f);

    private Image[] slotImages;
    private Image[] borderImages;
    private Image mpFillImage;
    private int cachedActiveIndex = -2;
    private int slotCount;

    private void Start()
    {
        CreateUI();
    }

    private void Update()
    {
        if (SkillManager.Instance == null) return;

        int activeIndex = SkillManager.Instance.ActiveSeasonIndex;
        if (activeIndex != cachedActiveIndex)
        {
            cachedActiveIndex = activeIndex;
            RefreshSlots();
        }

        UpdateMpBar();
    }

    private void CreateUI()
    {
        slotCount = seasonConfigs != null ? seasonConfigs.Length : 0;
        if (slotCount == 0) return;

        // Canvas
        GameObject canvasObj = new GameObject("SeasonSkillCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Skill Panel
        float totalWidth = slotCount * slotSize.x + (slotCount - 1) * spacing;

        GameObject panel = new GameObject("SkillPanel");
        panel.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(margin.x, -margin.y);
        panelRect.sizeDelta = new Vector2(totalWidth, slotSize.y);

        CreateSlots(panel.transform, totalWidth);
        CreateMpBar(canvasObj.transform, totalWidth);

        RefreshSlots();
    }

    private void CreateSlots(Transform parent, float totalWidth)
    {
        slotImages = new Image[slotCount];
        borderImages = new Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            float xPos = i * (slotSize.x + spacing);

            // Border
            GameObject borderObj = new GameObject($"Border_{i}");
            borderObj.transform.SetParent(parent, false);

            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0f, 1f);
            borderRect.anchorMax = new Vector2(0f, 1f);
            borderRect.pivot = new Vector2(0f, 1f);
            borderRect.anchoredPosition = new Vector2(xPos - borderThickness, borderThickness);
            borderRect.sizeDelta = slotSize + new Vector2(borderThickness * 2f, borderThickness * 2f);

            borderImages[i] = borderObj.AddComponent<Image>();
            borderImages[i].color = Color.clear;

            // Slot
            GameObject slotObj;
            if (slotPrefab != null)
            {
                slotObj = Instantiate(slotPrefab, parent);
                slotObj.name = $"Slot_{i}";
            }
            else
            {
                slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(parent, false);
                slotObj.AddComponent<Image>();
            }

            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            if (slotRect == null) slotRect = slotObj.AddComponent<RectTransform>();

            slotRect.anchorMin = new Vector2(0f, 1f);
            slotRect.anchorMax = new Vector2(0f, 1f);
            slotRect.pivot = new Vector2(0f, 1f);
            slotRect.anchoredPosition = new Vector2(xPos, 0f);
            slotRect.sizeDelta = slotSize;

            slotImages[i] = slotObj.GetComponent<Image>();
            if (slotImages[i] == null)
                slotImages[i] = slotObj.AddComponent<Image>();

            if (seasonConfigs[i].icon != null)
                slotImages[i].sprite = seasonConfigs[i].icon;

            slotImages[i].preserveAspect = true;
        }
    }

    private void CreateMpBar(Transform canvasTransform, float totalWidth)
    {
        // MP bar positioned below skill slots
        float mpBarY = -(margin.y + slotSize.y + mpBarGap);

        // Background
        GameObject bgObj = new GameObject("MpBarBackground");
        bgObj.transform.SetParent(canvasTransform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = new Vector2(margin.x, mpBarY);
        bgRect.sizeDelta = new Vector2(totalWidth, mpBarHeight);

        Image bgImage = bgObj.AddComponent<Image>();
        if (mpBarBackgroundSprite != null)
            bgImage.sprite = mpBarBackgroundSprite;
        bgImage.color = mpBarBackgroundColor;

        // Fill
        GameObject fillObj = new GameObject("MpBarFill");
        fillObj.transform.SetParent(bgObj.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        mpFillImage = fillObj.AddComponent<Image>();
        if (mpBarFillSprite != null)
            mpFillImage.sprite = mpBarFillSprite;
        mpFillImage.color = mpBarFillColor;
        mpFillImage.type = Image.Type.Filled;
        mpFillImage.fillMethod = Image.FillMethod.Horizontal;
        mpFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        mpFillImage.fillAmount = 1f;
    }

    private void RefreshSlots()
    {
        if (SkillManager.Instance == null) return;

        int activeIndex = SkillManager.Instance.ActiveSeasonIndex;

        for (int i = 0; i < slotCount; i++)
        {
            if (slotImages[i] == null) continue;

            bool unlocked = SkillManager.Instance.IsSeasonUnlocked(i);

            if (i == activeIndex && unlocked)
            {
                slotImages[i].color = activeColor;
                if (borderImages[i] != null)
                    borderImages[i].color = activeBorderColor;
            }
            else if (unlocked)
            {
                slotImages[i].color = unlockedColor;
                if (borderImages[i] != null)
                    borderImages[i].color = Color.clear;
            }
            else
            {
                slotImages[i].color = lockedColor;
                if (borderImages[i] != null)
                    borderImages[i].color = Color.clear;
            }
        }
    }

    private void UpdateMpBar()
    {
        if (mpFillImage == null || player == null) return;

        float ratio = player.MaxMp > 0f ? player.CurrentMp / player.MaxMp : 0f;
        mpFillImage.fillAmount = ratio;
    }
}
