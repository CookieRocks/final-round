using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public sealed class AftermathRoomController : MonoBehaviour
{
    private const string AftermathSceneName = "AftermathRoom";
    private const string DeskSceneName = "DeskScene";
    private const string InterviewRoomSceneName = "InterviewRoom";

    private readonly Color backgroundColor = new Color32(5, 7, 10, 255);
    private readonly Color panelColor = new Color32(18, 21, 28, 238);
    private readonly Color textColor = new Color32(235, 239, 244, 255);
    private readonly Color mutedTextColor = new Color32(166, 174, 186, 255);
    private readonly Color accentColor = new Color32(120, 214, 190, 255);

    [SerializeField] private bool generateSceneShell = true;
    [SerializeField] private float recoveryDelta = 1f;

    private TMP_Text titleText;
    private TMP_Text objectiveText;
    private TMP_Text statusText;
    private Slider composureSlider;
    private Button returnToDeskButton;
    private Button mainMenuButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != AftermathSceneName
            || FindAnyObjectByType<AftermathRoomController>() != null)
        {
            return;
        }

        GameObject root = new GameObject("AftermathRoomRoot");
        root.AddComponent<AftermathRoomController>();
    }

    private void Start()
    {
        if (generateSceneShell)
        {
            BuildSceneShell();
        }

        BuildUi();
        RefreshState();
    }

    private void BuildSceneShell()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Aftermath Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.transform.SetPositionAndRotation(new Vector3(0f, 1.55f, -5.8f), Quaternion.Euler(10f, 0f, 0f));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;

        if (FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObject = new GameObject("Aftermath Overhead Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.45f;
            light.color = new Color32(220, 232, 240, 255);
            lightObject.transform.rotation = Quaternion.Euler(56f, -22f, 0f);
        }

        Transform root = FindOrCreateChildRoot("Aftermath Symbolic Room");
        if (root.childCount > 0)
        {
            return;
        }

        Material floorMaterial = CreateMaterial("Aftermath Floor Material", new Color32(24, 25, 31, 255));
        Material wallMaterial = CreateMaterial("Aftermath Wall Material", new Color32(18, 20, 27, 255));
        Material propMaterial = CreateMaterial("Aftermath Prop Material", new Color32(61, 65, 74, 255));
        Material accentMaterial = CreateMaterial("Aftermath Accent Material", new Color32(93, 178, 162, 255));
        Material paperMaterial = CreateMaterial("Aftermath Paper Material", new Color32(205, 205, 190, 255));

        CreateCube("Aftermath Room Floor", new Vector3(0f, -0.05f, 0f), new Vector3(7.4f, 0.1f, 7.4f), floorMaterial, root);
        CreateCube("Aftermath Back Wall", new Vector3(0f, 1.55f, 3.6f), new Vector3(7.4f, 3.1f, 0.12f), wallMaterial, root);
        CreateCube("Aftermath Left Wall", new Vector3(-3.7f, 1.55f, 0f), new Vector3(0.12f, 3.1f, 7.4f), wallMaterial, root);
        CreateCube("Aftermath Right Wall", new Vector3(3.7f, 1.55f, 0f), new Vector3(0.12f, 3.1f, 7.4f), wallMaterial, root);
        CreateCube("Empty Interview Table", new Vector3(0f, 0.72f, 0.65f), new Vector3(4.8f, 0.2f, 1.55f), propMaterial, root);
        CreateCube("Empty Chair Left", new Vector3(-1.45f, 0.55f, 1.85f), new Vector3(0.58f, 1.1f, 0.5f), propMaterial, root);
        CreateCube("Empty Chair Center", new Vector3(0f, 0.55f, 2.05f), new Vector3(0.58f, 1.1f, 0.5f), propMaterial, root);
        CreateCube("Empty Chair Right", new Vector3(1.45f, 0.55f, 1.85f), new Vector3(0.58f, 1.1f, 0.5f), propMaterial, root);
        CreateCube("Aftermath Laptop", new Vector3(-1.15f, 0.92f, 0.15f), new Vector3(0.8f, 0.08f, 0.52f), accentMaterial, root);
        CreateCube("Aftermath Whiteboard", new Vector3(-2.75f, 1.78f, 3.5f), new Vector3(1.45f, 0.9f, 0.04f), paperMaterial, root);
        CreateCube("Job Ad Panel", new Vector3(2.35f, 1.7f, 3.5f), new Vector3(1.45f, 1.05f, 0.04f), paperMaterial, root);

        CreatePhrase("Unfortunately", new Vector3(-2.6f, 2.35f, 3.43f), root);
        CreatePhrase("After careful consideration", new Vector3(0f, 2.42f, 3.43f), root);
        CreatePhrase("We went with another candidate", new Vector3(2.2f, 2.25f, 3.43f), root);
        CreatePhrase("Strong profile", new Vector3(-3.55f, 1.45f, -1.2f), root, Quaternion.Euler(0f, 90f, 0f));
        CreatePhrase("No feedback available", new Vector3(3.55f, 1.36f, -0.35f), root, Quaternion.Euler(0f, -90f, 0f));
        CreatePhrase("Circle back", new Vector3(0f, 1.05f, 0.62f), root);

        CreateCube("Nameplate Hiring Manager", new Vector3(-1.45f, 0.88f, 0.02f), new Vector3(0.7f, 0.08f, 0.18f), accentMaterial, root);
        CreateCube("Nameplate Architect", new Vector3(0f, 0.88f, 0.02f), new Vector3(0.7f, 0.08f, 0.18f), accentMaterial, root);
        CreateCube("Nameplate Sales Director", new Vector3(1.45f, 0.88f, 0.02f), new Vector3(0.7f, 0.08f, 0.18f), accentMaterial, root);
        CreateCube("Feedback Forms Stack", new Vector3(1.05f, 0.92f, 0.25f), new Vector3(0.72f, 0.06f, 0.46f), paperMaterial, root);
    }

    private Transform FindOrCreateChildRoot(string rootName)
    {
        Transform existing = transform.Find(rootName);
        if (existing != null)
        {
            return existing;
        }

        GameObject childRoot = new GameObject(rootName);
        childRoot.transform.SetParent(transform, false);
        return childRoot.transform;
    }

    private void BuildUi()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("Aftermath Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);

        GameObject panel = new GameObject("Aftermath Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = panelColor;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(36f, -36f);
        panelRect.sizeDelta = new Vector2(680f, 260f);

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(26, 26, 22, 22);
        layout.spacing = 10;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        titleText = CreateText("Aftermath Title", panel.transform, "THE AFTERMATH", 36, FontStyles.Bold);
        titleText.color = textColor;
        objectiveText = CreateText("Aftermath Objective", panel.transform, "The panel has left. The room has not.\nClear the Room.", 23, FontStyles.Normal);
        objectiveText.color = mutedTextColor;
        statusText = CreateText("Aftermath Status", panel.transform, string.Empty, 19, FontStyles.Normal);
        statusText.color = accentColor;

        composureSlider = CreateSlider("Composure Meter", panel.transform);
        composureSlider.value = 0f;

        GameObject row = new GameObject("Aftermath Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup rowLayout = row.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 12;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = true;
        row.GetComponent<LayoutElement>().preferredHeight = 44f;

        returnToDeskButton = CreateButton("Return To Desk", row.transform, ReturnToDesk);
        mainMenuButton = CreateButton("Main Menu", row.transform, ReturnToMainMenu);
    }

    private void RefreshState()
    {
        bool hasRejectAftermath = HasActiveRejectAftermath(out CandidateState state);
        if (hasRejectAftermath)
        {
            SetText(statusText, state.AftermathCompleted
                ? "Composure 100% - aftermath already processed."
                : "Composure 0% - P36 skeleton only. Destruction arrives in P37.");
            composureSlider.value = state.AftermathCompleted ? 1f : 0f;
            return;
        }

        SetText(objectiveText, "No active aftermath run.\nThis room is safe to exit.");
        SetText(statusText, "Return to Desk or Main Menu.");
        composureSlider.value = 0f;
    }

    private void ReturnToDesk()
    {
        if (HasActiveRejectAftermath(out CandidateState state))
        {
            state.AftermathCompleted = true;
            state.Energy += Mathf.RoundToInt(recoveryDelta);
            state.CandidateConfidence += Mathf.RoundToInt(recoveryDelta);
            Debug.Log("Final Round P36: returning to Desk after aftermath.\n" + state.BuildDebugSummary());
        }

        SceneManager.LoadScene(DeskSceneName);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene(InterviewRoomSceneName);
    }

    private static bool HasActiveRejectAftermath(out CandidateState state)
    {
        return FinalRoundRunState.TryGetActiveState(out state)
            && state.HasActiveDeskRun
            && state.AftermathAvailable
            && state.RoomOutcome == nameof(InterviewOutcomeType.Reject);
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static Transform CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        return cube.transform;
    }

    private static TMP_Text CreatePhrase(string text, Vector3 position, Transform parent)
    {
        return CreatePhrase(text, position, parent, Quaternion.identity);
    }

    private static TMP_Text CreatePhrase(string text, Vector3 position, Transform parent, Quaternion rotation)
    {
        GameObject labelObject = new GameObject($"Phrase - {text}", typeof(RectTransform), typeof(TextMeshPro));
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = position;
        labelObject.transform.localRotation = rotation;
        labelObject.transform.localScale = Vector3.one * 0.08f;

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = 2.5f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color32(210, 230, 226, 255);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Left;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    private static Slider CreateSlider(string name, Transform parent)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderObject.transform, false);
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = new Color32(43, 48, 58, 255);
        Stretch(background.GetComponent<RectTransform>());

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color32(120, 214, 190, 255);
        Stretch(fill.GetComponent<RectTransform>());

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = fillImage;
        sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);
        return slider;
    }

    private Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color32(32, 82, 90, 255);
        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredWidth = label.Length > 12 ? 180f : 140f;
        layout.preferredHeight = 40f;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        TMP_Text text = CreateText("Text", buttonObject.transform, label, 17, FontStyles.Bold);
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        Stretch(text.GetComponent<RectTransform>());
        return button;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
