using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DialogueEditorWindow : EditorWindow
{
    // Dialogue Info
    private string npcName = "";
    private string dialogueName = "";
    private float typingSpeed = 30f;

    // Dialogue Lines
    private List<DialogueData.Speaker> speakers = new List<DialogueData.Speaker>();
    private List<string> lines = new List<string>();

    // Preview
    private bool showPreview = true;
    private int previewIndex = 0;

    // Scroll
    private Vector2 scrollPos;

    // Styles
    private GUIStyle previewTextStyle;
    private GUIStyle lineTextAreaStyle;
    private bool stylesInitialized;

    [MenuItem("Tools/Dialogue Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<DialogueEditorWindow>("Dialogue Editor");
        window.minSize = new Vector2(400, 500);
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        previewTextStyle = new GUIStyle(GUI.skin.label);
        previewTextStyle.wordWrap = true;
        previewTextStyle.richText = false;

        lineTextAreaStyle = new GUIStyle(EditorStyles.textArea);
        lineTextAreaStyle.wordWrap = true;

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        InitStyles();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawDialogueInfo();
        EditorGUILayout.Space(10);
        DrawDialogueLines();
        EditorGUILayout.Space(10);
        DrawPreview();
        EditorGUILayout.Space(10);
        DrawCreateButton();

        EditorGUILayout.EndScrollView();
    }

    // ─────────────────────────── Dialogue Info ───────────────────────────

    private void DrawDialogueInfo()
    {
        EditorGUILayout.LabelField("Dialogue Info", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            npcName = EditorGUILayout.TextField("NPC Name", npcName);
            dialogueName = EditorGUILayout.TextField("Dialogue Name", dialogueName);
            typingSpeed = EditorGUILayout.Slider("Typing Speed", typingSpeed, 5f, 100f);

            if (!string.IsNullOrWhiteSpace(npcName) && !string.IsNullOrWhiteSpace(dialogueName))
            {
                string path = $"Assets/ScriptableObject/Dialogue/{npcName}/{dialogueName}.asset";
                EditorGUILayout.HelpBox($"Save Path: {path}", MessageType.Info);
            }
        }
    }

    // ─────────────────────────── Dialogue Lines ──────────────────────────

    private void DrawDialogueLines()
    {
        EditorGUILayout.LabelField("Dialogue Lines", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            for (int i = 0; i < lines.Count; i++)
            {
                using (new EditorGUILayout.VerticalScope("helpBox"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Line {i + 1}", EditorStyles.miniLabel, GUILayout.Width(50));
                        speakers[i] = (DialogueData.Speaker)EditorGUILayout.EnumPopup(speakers[i], GUILayout.Width(80));

                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledScope(i == 0))
                        {
                            if (GUILayout.Button("\u25b2", GUILayout.Width(25)))
                            {
                                SwapLines(i, i - 1);
                                break;
                            }
                        }
                        using (new EditorGUI.DisabledScope(i == lines.Count - 1))
                        {
                            if (GUILayout.Button("\u25bc", GUILayout.Width(25)))
                            {
                                SwapLines(i, i + 1);
                                break;
                            }
                        }
                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            lines.RemoveAt(i);
                            speakers.RemoveAt(i);
                            if (previewIndex >= lines.Count)
                                previewIndex = Mathf.Max(0, lines.Count - 1);
                            break;
                        }
                    }

                    lines[i] = EditorGUILayout.TextArea(lines[i], lineTextAreaStyle, GUILayout.MinHeight(40));
                }
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("+ Add Line"))
            {
                speakers.Add(DialogueData.Speaker.NPC);
                lines.Add("");
            }
        }
    }

    private void SwapLines(int a, int b)
    {
        (lines[a], lines[b]) = (lines[b], lines[a]);
        (speakers[a], speakers[b]) = (speakers[b], speakers[a]);
    }

    // ─────────────────────────── Preview ─────────────────────────────────

    private void DrawPreview()
    {
        showPreview = EditorGUILayout.Foldout(showPreview, "Preview", true);
        if (!showPreview || lines.Count == 0) return;

        using (new EditorGUILayout.VerticalScope("box"))
        {
            // Navigation
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(previewIndex <= 0))
                {
                    if (GUILayout.Button("<", GUILayout.Width(30)))
                        previewIndex--;
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(
                    $"{previewIndex + 1} / {lines.Count}  [{speakers[previewIndex]}]",
                    new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter },
                    GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(previewIndex >= lines.Count - 1))
                {
                    if (GUILayout.Button(">", GUILayout.Width(30)))
                        previewIndex++;
                }
            }

            previewIndex = Mathf.Clamp(previewIndex, 0, Mathf.Max(0, lines.Count - 1));

            // Panel preview
            EditorGUILayout.Space(5);
            var rect = EditorGUILayout.GetControlRect(false, 60);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.85f));

            previewTextStyle.fontSize = 14;
            previewTextStyle.alignment = TextAnchor.UpperLeft;
            previewTextStyle.normal.textColor = Color.white;

            string speakerLabel = speakers[previewIndex] == DialogueData.Speaker.NPC
                ? (string.IsNullOrEmpty(npcName) ? "NPC" : npcName)
                : "Player";

            string previewText = lines[previewIndex];
            if (string.IsNullOrEmpty(previewText))
                previewText = "(empty)";

            // Speaker name
            var nameRect = new Rect(rect.x + 8, rect.y + 4, rect.width - 16, 20);
            var nameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = new Color(1f, 0.85f, 0.4f) },
                fontSize = 12
            };
            EditorGUI.LabelField(nameRect, speakerLabel, nameStyle);

            // Dialogue text
            var textRect = new Rect(rect.x + 8, rect.y + 22, rect.width - 16, rect.height - 26);
            EditorGUI.LabelField(textRect, previewText, previewTextStyle);
        }
    }

    // ─────────────────────────── Create Button ───────────────────────────

    private void DrawCreateButton()
    {
        bool valid = true;
        if (string.IsNullOrWhiteSpace(npcName))
        {
            EditorGUILayout.HelpBox("NPC Name is required.", MessageType.Warning);
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(dialogueName))
        {
            EditorGUILayout.HelpBox("Dialogue Name is required.", MessageType.Warning);
            valid = false;
        }
        if (lines.Count == 0)
        {
            EditorGUILayout.HelpBox("Add at least one dialogue line.", MessageType.Warning);
            valid = false;
        }
        else
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    EditorGUILayout.HelpBox($"Line {i + 1} is empty.", MessageType.Warning);
                    valid = false;
                    break;
                }
            }
        }

        using (new EditorGUI.DisabledScope(!valid))
        {
            if (GUILayout.Button("Create Dialogue SO", GUILayout.Height(30)))
                CreateDialogueSO();
        }
    }

    private void CreateDialogueSO()
    {
        string folderPath = $"Assets/ScriptableObject/Dialogue/{npcName}";
        string assetPath = $"{folderPath}/{dialogueName}.asset";

        var existing = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Overwrite?",
                $"'{assetPath}' already exists.\nOverwrite?", "Overwrite", "Cancel"))
                return;
        }

        EnsureFolderExists(folderPath);

        DialogueData data = existing != null ? existing : ScriptableObject.CreateInstance<DialogueData>();
        data.speakerName = npcName;
        data.typingSpeed = typingSpeed;
        data.lines = new DialogueData.Line[lines.Count];
        for (int i = 0; i < lines.Count; i++)
        {
            data.lines[i] = new DialogueData.Line
            {
                speaker = speakers[i],
                text = lines[i]
            };
        }

        if (existing != null)
        {
            EditorUtility.SetDirty(data);
        }
        else
        {
            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
        EditorGUIUtility.PingObject(Selection.activeObject);

        Debug.Log($"Dialogue SO created: {assetPath}");
    }

    private void EnsureFolderExists(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
