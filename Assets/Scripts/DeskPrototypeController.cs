using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public sealed class DeskPrototypeController : MonoBehaviour
{
    public enum DeskPrototypeState
    {
        Standby,
        LaptopFocus,
        JobListing,
        ApplicationChoice,
        Recruiter,
        Prep,
        TransitioningToRoom
    }

    private const string PlaceholderJobId = "NCS-SE-001";
    private const string PlaceholderCompany = "Northbridge Cyber Systems";
    private const string PlaceholderRole = "Senior Solutions Engineer - Security Presales";
    private const string PlaceholderSummary =
        "A controlled P27 listing shell for the VS2 Desk prototype. The full application strategy, recruiter thread, and prep choices arrive in later milestones.";

    [Header("Scene")]
    [SerializeField] private string interviewRoomSceneName = "InterviewRoom";
    [SerializeField] private bool generatePrototypeSceneObjects = true;
    [SerializeField] private Camera deskCamera;
    [SerializeField] private Transform laptopInteractable;

    [Header("Run State")]
    [SerializeField] private DeskPrototypeState currentState = DeskPrototypeState.Standby;
    [SerializeField] private JobListingData defaultJobListing;

    [Header("Input")]
#if !ENABLE_INPUT_SYSTEM
    [SerializeField] private KeyCode openLaptopKey = KeyCode.E;
    [SerializeField] private KeyCode alternateOpenLaptopKey = KeyCode.Space;
#endif

    private Canvas canvas;
    private GameObject laptopPanel;
    private TMP_Text debugText;
    private TMP_Text listingSummaryText;

    public DeskPrototypeState CurrentState => currentState;

    private void Start()
    {
        if (generatePrototypeSceneObjects)
        {
            BuildPrototypeSceneShell();
        }

        BuildPrototypeUi();
        RefreshDebugDisplay();
        Debug.Log("Final Round P27: Desk prototype ready. Press E/Space or click the laptop to open the placeholder job UI.");
    }

    private void Update()
    {
        if (WasOpenLaptopPressed() || WasLaptopClicked())
        {
            OpenLaptopInterface();
        }
    }

    public void BeginDeskRun()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        state.SelectedJobId = GetActiveJobId();
        currentState = DeskPrototypeState.LaptopFocus;
        Debug.Log("Final Round P27: Desk run started.\n" + state.BuildDebugSummary());
        RefreshDebugDisplay();
    }

    public CandidateState CreateNeutralCandidateState()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        state.SelectedJobId = GetActiveJobId();
        currentState = DeskPrototypeState.Standby;
        RefreshDebugDisplay();
        return state;
    }

    public void OpenLaptopInterface()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        currentState = DeskPrototypeState.JobListing;
        if (laptopPanel != null)
        {
            laptopPanel.SetActive(true);
        }

        RefreshDebugDisplay();
    }

    public void ContinuePlaceholderListing()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        currentState = DeskPrototypeState.ApplicationChoice;
        if (listingSummaryText != null)
        {
            listingSummaryText.text =
                "P27 stops here: this confirms the job shell is readable and the run state exists. P28 will add real application choices and trade-offs.";
        }

        Debug.Log("Final Round P27: placeholder listing continued. Full choices are intentionally deferred.");
        RefreshDebugDisplay();
    }

    public void GoToInterviewRoom()
    {
        CandidateState state = FinalRoundRunState.HasActiveRun()
            ? FinalRoundRunState.Instance.State
            : FinalRoundRunState.CreateNeutralRun();

        state.SelectedJobId = string.IsNullOrWhiteSpace(state.SelectedJobId)
            ? GetActiveJobId()
            : state.SelectedJobId;

        currentState = DeskPrototypeState.TransitioningToRoom;
        Debug.Log(
            $"Final Round P27: Desk-to-Room transition requested. Scene: {interviewRoomSceneName}\n" +
            state.BuildDebugSummary());

        RefreshDebugDisplay();
        SceneManager.LoadScene(interviewRoomSceneName);
    }

    public void ResetDeskRun()
    {
        if (FinalRoundRunState.HasInstance)
        {
            FinalRoundRunState.Instance.ResetRun();
        }

        currentState = DeskPrototypeState.Standby;
        if (laptopPanel != null)
        {
            laptopPanel.SetActive(false);
        }

        if (listingSummaryText != null)
        {
            listingSummaryText.text = GetListingSummary();
        }

        Debug.Log("Final Round P27: Desk run reset.");
        RefreshDebugDisplay();
    }

    private void BuildPrototypeSceneShell()
    {
        if (deskCamera == null)
        {
            deskCamera = Camera.main;
        }

        if (deskCamera == null)
        {
            GameObject cameraObject = new GameObject("Desk Camera");
            deskCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        deskCamera.transform.SetPositionAndRotation(new Vector3(0f, 2.2f, -4.8f), Quaternion.Euler(24f, 0f, 0f));
        deskCamera.clearFlags = CameraClearFlags.SolidColor;
        deskCamera.backgroundColor = new Color32(13, 16, 22, 255);

        if (FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObject = new GameObject("Desk Soft Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color32(218, 229, 236, 255);
            lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        Material deskMaterial = CreateMaterial("P27 Desk Surface Material", new Color32(55, 50, 43, 255));
        Material laptopMaterial = CreateMaterial("P27 Laptop Material", new Color32(11, 15, 20, 255));
        Material screenMaterial = CreateMaterial("P27 Laptop Screen Material", new Color32(8, 59, 71, 255));
        Material roomMaterial = CreateMaterial("P27 Desk Room Material", new Color32(29, 33, 42, 255));
        Material chairMaterial = CreateMaterial("P27 Desk Chair Material", new Color32(34, 38, 49, 255));

        CreateCube("Desk Room Back Wall", new Vector3(0f, 1.4f, 1.75f), new Vector3(7f, 2.8f, 0.18f), roomMaterial);
        CreateCube("Desk Floor", new Vector3(0f, -0.08f, -1.2f), new Vector3(7f, 0.16f, 6f), roomMaterial);
        CreateCube("Desk Surface", new Vector3(0f, 0.78f, -0.15f), new Vector3(4.4f, 0.18f, 1.75f), deskMaterial);
        CreateCube("Desk Chair Back", new Vector3(0f, 1.03f, -2.05f), new Vector3(1.15f, 1.15f, 0.18f), chairMaterial);
        CreateCube("Desk Chair Seat", new Vector3(0f, 0.47f, -1.95f), new Vector3(1.2f, 0.18f, 1.0f), chairMaterial);
        CreateCube("Desk Notebook Placeholder", new Vector3(-1.42f, 0.91f, -0.38f), new Vector3(0.88f, 0.04f, 0.56f), CreateMaterial("P27 Notebook Material", new Color32(200, 190, 150, 255)));

        Transform laptopBase = CreateCube("Laptop Click Target", new Vector3(0f, 0.93f, -0.08f), new Vector3(1.45f, 0.08f, 0.9f), laptopMaterial);
        Transform laptopScreen = CreateCube("Laptop Screen", new Vector3(0f, 1.35f, 0.32f), new Vector3(1.45f, 0.72f, 0.08f), screenMaterial);
        laptopScreen.rotation = Quaternion.Euler(-10f, 0f, 0f);
        laptopInteractable = laptopBase;
    }

    private void BuildPrototypeUi()
    {
        EnsureEventSystem();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Desk Prototype Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        CreateHeader(canvas.transform);
        CreateDebugPanel(canvas.transform);
        CreateLaptopPanel(canvas.transform);
    }

    private void CreateHeader(Transform parent)
    {
        TMP_Text header = CreateText("Desk Header", parent, "FINAL ROUND  /  THE DESK", 28, FontStyles.Bold, TextAlignmentOptions.Left);
        RectTransform rect = header.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(42f, -34f);
        rect.sizeDelta = new Vector2(700f, 48f);

        TMP_Text prompt = CreateText("Desk Prompt", parent, "Press E / Space or click the laptop", 20, FontStyles.Normal, TextAlignmentOptions.Left);
        RectTransform promptRect = prompt.rectTransform;
        promptRect.anchorMin = new Vector2(0f, 1f);
        promptRect.anchorMax = new Vector2(0f, 1f);
        promptRect.pivot = new Vector2(0f, 1f);
        promptRect.anchoredPosition = new Vector2(42f, -78f);
        promptRect.sizeDelta = new Vector2(700f, 34f);
        prompt.color = new Color32(151, 170, 188, 255);
    }

    private void CreateLaptopPanel(Transform parent)
    {
        laptopPanel = CreatePanel("Laptop Job UI", parent, new Color32(13, 18, 24, 238));
        RectTransform panelRect = laptopPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -16f);
        panelRect.sizeDelta = new Vector2(820f, 520f);

        VerticalLayoutGroup layout = laptopPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 28, 28);
        layout.spacing = 14f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateText("Laptop Title", laptopPanel.transform, "Northbridge Jobs", 34, FontStyles.Bold, TextAlignmentOptions.Left);
        CreateText("Laptop Role", laptopPanel.transform, GetRoleLine(), 24, FontStyles.Bold, TextAlignmentOptions.Left);
        CreateText("Laptop Company", laptopPanel.transform, GetCompanyLine(), 20, FontStyles.Normal, TextAlignmentOptions.Left);

        listingSummaryText = CreateText("Laptop Summary", laptopPanel.transform, GetListingSummary(), 21, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        listingSummaryText.color = new Color32(209, 217, 224, 255);
        listingSummaryText.rectTransform.sizeDelta = new Vector2(0f, 128f);
        listingSummaryText.GetComponent<LayoutElement>().preferredHeight = 150f;

        GameObject buttonRow = new GameObject("Laptop Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        buttonRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 14f;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        buttonRow.GetComponent<LayoutElement>().preferredHeight = 52f;

        CreateButton("Continue", buttonRow.transform, ContinuePlaceholderListing);
        CreateButton("Go To Interview Room", buttonRow.transform, GoToInterviewRoom);
        CreateButton("Reset Desk Run", buttonRow.transform, ResetDeskRun);

        laptopPanel.SetActive(false);
    }

    private void CreateDebugPanel(Transform parent)
    {
        GameObject debugPanel = CreatePanel("Desk Debug Readout", parent, new Color32(9, 13, 18, 200));
        RectTransform rect = debugPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-32f, 28f);
        rect.sizeDelta = new Vector2(520f, 180f);

        debugText = CreateText("Desk Debug Text", debugPanel.transform, string.Empty, 17, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform textRect = debugText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 14f);
        textRect.offsetMax = new Vector2(-18f, -14f);
    }

    private void RefreshDebugDisplay()
    {
        if (debugText == null)
        {
            return;
        }

        string summary = FinalRoundRunState.TryGetActiveState(out CandidateState state)
            ? state.BuildDebugSummary()
            : "No active CandidateState.\nDirect Room launch will use neutral VS1 fallback.";

        debugText.text =
            $"P27 Desk Prototype\nState: {currentState}\nTarget Scene: {interviewRoomSceneName}\n\n{summary}";
    }

    private string GetActiveJobId()
    {
        return defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.jobId)
            ? defaultJobListing.jobId
            : PlaceholderJobId;
    }

    private string GetRoleLine()
    {
        string role = defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.roleTitle)
            ? defaultJobListing.roleTitle
            : PlaceholderRole;
        return $"Role: {role}";
    }

    private string GetCompanyLine()
    {
        string company = defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.companyName)
            ? defaultJobListing.companyName
            : PlaceholderCompany;
        return $"Company: {company}";
    }

    private string GetListingSummary()
    {
        string summary = defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.summary)
            ? defaultJobListing.summary
            : PlaceholderSummary;
        return $"Listing summary\n{summary}";
    }

    private bool WasOpenLaptopPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(openLaptopKey) || Input.GetKeyDown(alternateOpenLaptopKey);
#endif
    }

    private bool WasLaptopClicked()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        Vector2 pointerPosition = Mouse.current.position.ReadValue();
#else
        if (!Input.GetMouseButtonDown(0))
        {
            return false;
        }

        Vector2 pointerPosition = Input.mousePosition;
#endif
        if (deskCamera == null || laptopInteractable == null)
        {
            return false;
        }

        Ray ray = deskCamera.ScreenPointToRay(pointerPosition);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.transform == laptopInteractable;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int size, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = size;
        tmpText.fontStyle = style;
        tmpText.alignment = alignment;
        tmpText.color = new Color32(238, 242, 248, 255);
        tmpText.textWrappingMode = TextWrappingModes.Normal;
        return tmpText;
    }

    private static Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color32(29, 82, 92, 255);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(action);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = label.Length > 12 ? 230f : 150f;
        layoutElement.preferredHeight = 46f;

        TMP_Text text = CreateText("Label", buttonObject.transform, label, 18, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 6f);
        textRect.offsetMax = new Vector2(-10f, -6f);
        return button;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static Transform CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        if (material != null && cube.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = material;
        }

        return cube.transform;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem != null)
        {
#if ENABLE_INPUT_SYSTEM
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    Destroy(legacyModule);
                }

                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
            return;
        }

#if ENABLE_INPUT_SYSTEM
        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
        eventSystemObject.transform.SetAsLastSibling();
    }
}
