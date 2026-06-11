using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public sealed class AftermathRoomController : MonoBehaviour
{
    private const string AftermathSceneName = "AftermathRoom";
    private const string DeskSceneName = "DeskScene";
    private const string InterviewRoomSceneName = "InterviewRoom";
    private const int ComposureTarget = 100;
    private const float HitFeedbackSeconds = 1.25f;
    private const float DestroyedFeedbackSeconds = 2.5f;
    private const float CompletionFeedbackSeconds = 4f;
    private const float MinLookPitch = -18f;
    private const float MaxLookPitch = 24f;

    private readonly Color backgroundColor = new Color32(5, 7, 10, 255);
    private readonly Color panelColor = new Color32(18, 21, 28, 238);
    private readonly Color textColor = new Color32(235, 239, 244, 255);
    private readonly Color mutedTextColor = new Color32(166, 174, 186, 255);
    private readonly Color accentColor = new Color32(120, 214, 190, 255);

    [SerializeField] private bool generateSceneShell = true;
    [SerializeField] private float recoveryDelta = 1f;
    [SerializeField] private float hitRaycastDistance = 10f;
    [SerializeField] private float movementSpeed = 2.4f;
    [SerializeField] private float lookSensitivity = 1.5f;

    private TMP_Text titleText;
    private TMP_Text objectiveText;
    private TMP_Text statusText;
    private TMP_Text hammerText;
    private Slider composureSlider;
    private Button returnToDeskButton;
    private Button mainMenuButton;
    private Camera aftermathCamera;
    private int composure;
    private bool acceptsDestructibleInput;
    private bool completionReached;
    private float cameraYaw;
    private float cameraPitch = 10f;
    private Coroutine feedbackRoutine;
    private readonly List<AftermathDestructible> destructibles = new List<AftermathDestructible>();

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
        aftermathCamera = Camera.main;
        if (aftermathCamera == null)
        {
            GameObject cameraObject = new GameObject("Aftermath Camera");
            cameraObject.tag = "MainCamera";
            aftermathCamera = cameraObject.AddComponent<Camera>();
        }

        aftermathCamera.transform.SetPositionAndRotation(new Vector3(0f, 1.55f, -5.8f), Quaternion.Euler(10f, 0f, 0f));
        cameraYaw = aftermathCamera.transform.eulerAngles.y;
        cameraPitch = NormalizePitch(aftermathCamera.transform.eulerAngles.x);
        aftermathCamera.clearFlags = CameraClearFlags.SolidColor;
        aftermathCamera.backgroundColor = backgroundColor;

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
        Material brokenMaterial = CreateMaterial("Aftermath Processed Material", new Color32(88, 96, 108, 255));
        Material phraseMaterial = CreateMaterial("Aftermath Phrase Plaque Material", new Color32(42, 45, 54, 255));

        CreateCube("Aftermath Room Floor", new Vector3(0f, -0.05f, 0f), new Vector3(7.4f, 0.1f, 7.4f), floorMaterial, root);
        CreateCube("Aftermath Back Wall", new Vector3(0f, 1.55f, 3.6f), new Vector3(7.4f, 3.1f, 0.12f), wallMaterial, root);
        CreateCube("Aftermath Left Wall", new Vector3(-3.7f, 1.55f, 0f), new Vector3(0.12f, 3.1f, 7.4f), wallMaterial, root);
        CreateCube("Aftermath Right Wall", new Vector3(3.7f, 1.55f, 0f), new Vector3(0.12f, 3.1f, 7.4f), wallMaterial, root);
        CreateCube("Empty Interview Table", new Vector3(0f, 0.72f, 0.65f), new Vector3(4.8f, 0.2f, 1.55f), propMaterial, root);
        CreateDestructibleCube("empty-chair-left", "Empty chair", new Vector3(-1.45f, 0.55f, 1.85f), new Vector3(0.58f, 1.1f, 0.5f), propMaterial, brokenMaterial, root, 2, 12, "The empty chair jolts sideways.", "A little pressure leaves the room.");
        CreateCube("Empty Chair Center", new Vector3(0f, 0.55f, 2.05f), new Vector3(0.58f, 1.1f, 0.5f), propMaterial, root);
        CreateCube("Empty Chair Right", new Vector3(1.45f, 0.55f, 1.85f), new Vector3(0.58f, 1.1f, 0.5f), propMaterial, root);
        CreateDestructibleCube("rejection-laptop", "Rejection laptop", new Vector3(-1.15f, 0.92f, 0.15f), new Vector3(0.8f, 0.08f, 0.52f), accentMaterial, brokenMaterial, root, 2, 14, "The laptop coughs up another polite sentence.", "The inbox is quieter now.");
        CreateCube("Aftermath Whiteboard", new Vector3(-2.75f, 1.78f, 3.5f), new Vector3(1.45f, 0.9f, 0.04f), paperMaterial, root);
        CreateDestructibleCube("job-ad-panel", "Job-ad panel", new Vector3(2.35f, 1.7f, 3.5f), new Vector3(1.45f, 1.05f, 0.04f), paperMaterial, brokenMaterial, root, 2, 12, "The role requirements wobble.", "The listing stops pretending to be clear.");

        CreateDestructiblePhrase("unfortunately-plaque", "Unfortunately plaque", "Unfortunately...", new Vector3(-2.6f, 2.35f, 3.43f), root, Quaternion.identity, phraseMaterial, brokenMaterial, 1, 10, "The word hangs there.", "One phrase processed.");
        CreateDestructiblePhrase("careful-consideration-sign", "Careful consideration sign", "After careful consideration", new Vector3(0f, 2.42f, 3.43f), root, Quaternion.identity, phraseMaterial, brokenMaterial, 1, 10, "Careful consideration rattles.", "The wording loses some of its power.");
        CreateDestructiblePhrase("details-on-file-placard", "Details on file placard", "We'll keep your details on file", new Vector3(2.2f, 2.25f, 3.43f), root, Quaternion.identity, phraseMaterial, brokenMaterial, 1, 10, "The promise feels weightless.", "The file can keep itself.");
        CreateDestructiblePhrase("no-feedback-available", "No feedback available", "No feedback available", new Vector3(3.55f, 1.36f, -0.35f), root, Quaternion.Euler(0f, -90f, 0f), phraseMaterial, brokenMaterial, 1, 10, "The blankness answers back.", "Silence has less leverage.");
        CreatePhrase("Circle back", new Vector3(0f, 1.05f, 0.62f), root);

        CreateDestructibleCube("empty-nameplate", "Empty nameplate", new Vector3(-1.45f, 0.88f, 0.02f), new Vector3(0.7f, 0.08f, 0.18f), accentMaterial, brokenMaterial, root, 1, 8, "The blank nameplate clicks.", "The room remembers fewer titles.");
        CreateCube("Nameplate Architect", new Vector3(0f, 0.88f, 0.02f), new Vector3(0.7f, 0.08f, 0.18f), accentMaterial, root);
        CreateCube("Nameplate Sales Director", new Vector3(1.45f, 0.88f, 0.02f), new Vector3(0.7f, 0.08f, 0.18f), accentMaterial, root);
        CreateDestructibleCube("feedback-form-stack", "Feedback form stack", new Vector3(1.05f, 0.92f, 0.25f), new Vector3(0.72f, 0.06f, 0.46f), paperMaterial, brokenMaterial, root, 1, 10, "The forms rustle without detail.", "A little ambiguity leaves the stack.");
        CreateDestructibleCube("scorecard-shard", "Scorecard shard", new Vector3(0.35f, 0.94f, 0.08f), new Vector3(0.5f, 0.04f, 0.32f), paperMaterial, brokenMaterial, root, 1, 8, "The numbers slide out of alignment.", "The scorecard matters less for a moment.");
        CreateDestructibleCube("calendar-invite-block", "Calendar invite block", new Vector3(-3.55f, 1.15f, 0.2f), new Vector3(0.05f, 0.52f, 0.82f), accentMaterial, brokenMaterial, root, 1, 8, "The calendar invite buzzes.", "The meeting is finally over.", Quaternion.Euler(0f, 90f, 0f));
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
        panelRect.sizeDelta = new Vector2(720f, 315f);

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

        hammerText = CreateText("Feedback Hammer Prompt", panel.transform, "Move: WASD. Look: hold right mouse. Feedback Hammer: Left Click / E / Space.", 16, FontStyles.Normal);
        hammerText.color = textColor;

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
        acceptsDestructibleInput = hasRejectAftermath && !state.AftermathCompleted;
        if (hasRejectAftermath)
        {
            composure = state.AftermathCompleted ? ComposureTarget : 0;
            completionReached = state.AftermathCompleted;
            SetText(statusText, state.AftermathCompleted
                ? "Composure 100% - aftermath already processed."
                : "Composure 0% - Feedback Hammer ready.");
            SetText(hammerText, state.AftermathCompleted
                ? "The room is quiet. Return to Desk when ready."
                : "Move: WASD. Look: hold right mouse. Feedback Hammer: Left Click / E / Space.");
            UpdateComposureUi();
            return;
        }

        acceptsDestructibleInput = false;
        SetText(objectiveText, "No active aftermath run.\nThis room is safe to exit.");
        SetText(statusText, "Return to Desk or Main Menu.");
        SetText(hammerText, "No active aftermath run.");
        composureSlider.value = 0f;
    }

    private void Update()
    {
        if (!acceptsDestructibleInput)
        {
            return;
        }

        UpdateCameraMovement();

        if (IsPointerOverUi())
        {
            return;
        }

        if (WasPrimaryHitPressed())
        {
            TryFeedbackHammerHit(GetPointerPosition());
            return;
        }

        if (WasKeyboardHitPressed())
        {
            TryFeedbackHammerHit(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        }
    }

    private void UpdateCameraMovement()
    {
        if (aftermathCamera == null)
        {
            aftermathCamera = Camera.main;
        }

        if (aftermathCamera == null)
        {
            return;
        }

        Vector3 forward = aftermathCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = aftermathCamera.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 movement = Vector3.zero;
        if (IsMoveKeyHeld(MoveKey.Forward))
        {
            movement += forward;
        }

        if (IsMoveKeyHeld(MoveKey.Back))
        {
            movement -= forward;
        }

        if (IsMoveKeyHeld(MoveKey.Right))
        {
            movement += right;
        }

        if (IsMoveKeyHeld(MoveKey.Left))
        {
            movement -= right;
        }

        if (movement.sqrMagnitude > 0.01f)
        {
            Vector3 nextPosition = aftermathCamera.transform.position + movement.normalized * movementSpeed * Time.deltaTime;
            nextPosition.x = Mathf.Clamp(nextPosition.x, -2.9f, 2.9f);
            nextPosition.y = 1.55f;
            nextPosition.z = Mathf.Clamp(nextPosition.z, -6.15f, -1.15f);
            aftermathCamera.transform.position = nextPosition;
        }

        bool lookRequested = IsLookHeld();
        if (!lookRequested)
        {
            return;
        }

        Vector2 lookDelta = GetLookDelta();
        cameraYaw += lookDelta.x * lookSensitivity;
        cameraPitch -= lookDelta.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, MinLookPitch, MaxLookPitch);
        aftermathCamera.transform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }

    private void ReturnToDesk()
    {
        if (HasActiveRejectAftermath(out CandidateState state))
        {
            state.AftermathCompleted = true;
            state.Energy += Mathf.RoundToInt(recoveryDelta);
            state.CandidateConfidence += Mathf.RoundToInt(recoveryDelta);
            Debug.Log("Final Round P37: aftermath completed; returning to Desk.\n" + state.BuildDebugSummary());
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

    private AftermathDestructible CreateDestructibleCube(
        string objectId,
        string displayLabel,
        Vector3 position,
        Vector3 scale,
        Material intactMaterial,
        Material brokenMaterial,
        Transform parent,
        int hitPoints,
        int catharsisValue,
        string onHitText,
        string onDestroyedText,
        Quaternion? rotation = null)
    {
        GameObject wrapper = new GameObject($"Destructible - {displayLabel}");
        wrapper.transform.SetParent(parent, false);
        wrapper.transform.localPosition = position;
        wrapper.transform.localRotation = rotation ?? Quaternion.identity;

        GameObject intact = GameObject.CreatePrimitive(PrimitiveType.Cube);
        intact.name = $"{displayLabel} Intact";
        intact.transform.SetParent(wrapper.transform, false);
        intact.transform.localScale = scale;
        SetSharedMaterial(intact, intactMaterial);

        GameObject broken = CreateBrokenBlock($"{displayLabel} Processed", scale, brokenMaterial, wrapper.transform);

        AftermathDestructible destructible = wrapper.AddComponent<AftermathDestructible>();
        destructible.Configure(objectId, displayLabel, hitPoints, catharsisValue, intact, broken, onHitText, onDestroyedText);
        destructibles.Add(destructible);
        return destructible;
    }

    private AftermathDestructible CreateDestructiblePhrase(
        string objectId,
        string displayLabel,
        string phrase,
        Vector3 position,
        Transform parent,
        Quaternion rotation,
        Material plaqueMaterial,
        Material brokenMaterial,
        int hitPoints,
        int catharsisValue,
        string onHitText,
        string onDestroyedText)
    {
        GameObject wrapper = new GameObject($"Destructible - {displayLabel}");
        wrapper.transform.SetParent(parent, false);
        wrapper.transform.localPosition = position;
        wrapper.transform.localRotation = rotation;

        GameObject intact = new GameObject($"{displayLabel} Intact");
        intact.transform.SetParent(wrapper.transform, false);

        GameObject plaque = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plaque.name = $"{displayLabel} Plaque";
        plaque.transform.SetParent(intact.transform, false);
        plaque.transform.localScale = new Vector3(1.35f, 0.36f, 0.04f);
        SetSharedMaterial(plaque, plaqueMaterial);

        TMP_Text label = CreatePhrase(phrase, new Vector3(0f, 0.02f, -0.04f), intact.transform);
        label.gameObject.name = $"{displayLabel} Text";
        label.fontSize = phrase.Length > 18 ? 1.75f : 2.25f;

        GameObject broken = CreateBrokenBlock($"{displayLabel} Processed", new Vector3(1.35f, 0.36f, 0.04f), brokenMaterial, wrapper.transform);

        AftermathDestructible destructible = wrapper.AddComponent<AftermathDestructible>();
        destructible.Configure(objectId, displayLabel, hitPoints, catharsisValue, intact, broken, onHitText, onDestroyedText);
        destructibles.Add(destructible);
        return destructible;
    }

    private static GameObject CreateBrokenBlock(string name, Vector3 scale, Material material, Transform parent)
    {
        GameObject brokenRoot = new GameObject(name);
        brokenRoot.transform.SetParent(parent, false);

        for (int i = 0; i < 3; i++)
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = $"Processed Fragment {i + 1}";
            shard.transform.SetParent(brokenRoot.transform, false);
            shard.transform.localPosition = new Vector3((i - 1) * scale.x * 0.22f, -0.03f * i, -0.02f * i);
            shard.transform.localRotation = Quaternion.Euler(0f, 0f, (i - 1) * 8f);
            shard.transform.localScale = new Vector3(scale.x * 0.28f, scale.y * 0.7f, scale.z * 1.1f);
            SetSharedMaterial(shard, material);
        }

        return brokenRoot;
    }

    private void TryFeedbackHammerHit(Vector3 screenPosition)
    {
        if (aftermathCamera == null)
        {
            aftermathCamera = Camera.main;
        }

        if (aftermathCamera == null)
        {
            ShowFeedback("Feedback Hammer has no view into the room.", HitFeedbackSeconds);
            return;
        }

        Ray ray = aftermathCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, hitRaycastDistance))
        {
            ShowFeedback("The Feedback Hammer passes through empty phrasing.", HitFeedbackSeconds);
            return;
        }

        AftermathDestructible destructible = hit.collider.GetComponentInParent<AftermathDestructible>();
        if (destructible == null)
        {
            ShowFeedback("That part of the memory does not need clearing.", HitFeedbackSeconds);
            return;
        }

        if (!destructible.TryProcessHit(out string feedbackText, out bool destroyedThisHit))
        {
            ShowFeedback("Already processed.", HitFeedbackSeconds);
            return;
        }

        if (destroyedThisHit)
        {
            composure = Mathf.Clamp(composure + destructible.CatharsisValue, 0, ComposureTarget);
            UpdateComposureUi();
            Debug.Log($"Final Round P37: destroyed symbolic object '{destructible.ObjectId}' ({destructible.DisplayLabel}). Composure {composure}/{ComposureTarget}.");
            ShowFeedback(feedbackText, DestroyedFeedbackSeconds);

            if (composure >= ComposureTarget && !completionReached)
            {
                completionReached = true;
                ShowFeedback("The room is quieter now.", CompletionFeedbackSeconds);
            }
            return;
        }

        ShowFeedback(feedbackText, HitFeedbackSeconds);
    }

    private void UpdateComposureUi()
    {
        float ratio = Mathf.Clamp01(composure / (float)ComposureTarget);
        if (composureSlider != null)
        {
            composureSlider.value = ratio;
        }

        SetText(statusText, $"Composure {Mathf.RoundToInt(ratio * 100f)}% - {(completionReached ? "The room is quieter now." : "Process symbolic objects with the Feedback Hammer.")}");

        if (returnToDeskButton != null)
        {
            Image buttonImage = returnToDeskButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = completionReached
                    ? new Color32(57, 124, 104, 255)
                    : new Color32(32, 82, 90, 255);
            }
        }
    }

    private void ShowFeedback(string message, float seconds)
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(message, seconds));
    }

    private IEnumerator ShowFeedbackRoutine(string message, float seconds)
    {
        SetText(hammerText, message);
        yield return new WaitForSeconds(seconds);

        if (completionReached)
        {
            SetText(hammerText, "The room is quieter now. Return to Desk when ready.");
        }
        else if (acceptsDestructibleInput)
        {
            SetText(hammerText, "Feedback Hammer: Left Click / E / Space to process symbolic objects.");
        }

        feedbackRoutine = null;
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

    private static void SetSharedMaterial(GameObject target, Material material)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private static bool WasPrimaryHitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private static bool WasKeyboardHitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null
            && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private static bool IsLookHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return Input.GetMouseButton(1);
#endif
    }

    private static Vector2 GetLookDelta()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue();
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }

    private static Vector3 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current == null ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f) : Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }

    private static bool IsMoveKeyHeld(MoveKey key)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        return key switch
        {
            MoveKey.Forward => Keyboard.current.wKey.isPressed,
            MoveKey.Back => Keyboard.current.sKey.isPressed,
            MoveKey.Left => Keyboard.current.aKey.isPressed,
            MoveKey.Right => Keyboard.current.dKey.isPressed,
            _ => false
        };
#else
        return key switch
        {
            MoveKey.Forward => Input.GetKey(KeyCode.W),
            MoveKey.Back => Input.GetKey(KeyCode.S),
            MoveKey.Left => Input.GetKey(KeyCode.A),
            MoveKey.Right => Input.GetKey(KeyCode.D),
            _ => false
        };
#endif
    }

    private static float NormalizePitch(float pitch)
    {
        return pitch > 180f ? pitch - 360f : pitch;
    }

    private enum MoveKey
    {
        Forward,
        Back,
        Left,
        Right
    }
}
