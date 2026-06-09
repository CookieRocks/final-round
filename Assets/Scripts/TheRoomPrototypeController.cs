using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class TheRoomPrototypeController : MonoBehaviour
{
    private const string BootstrapSceneName = "InterviewRoom";
    private const float QuestionRevealDelay = 1f;

    [SerializeField] private SimpleFirstPersonWalkController playerController;
    [SerializeField] private InterviewSeat interviewSeat;
    [SerializeField] private Transform standingStartPoint;
    [SerializeField] private Transform seatedCameraPoint;
    [SerializeField] private float sitSnapDuration = 0.5f;
    [Header("RC8 Room Readability")]
    [SerializeField] private Color chairMarkerIdleColor = new Color32(62, 130, 118, 150);
    [SerializeField] private Color chairMarkerActiveColor = new Color32(112, 238, 206, 255);
    [SerializeField] private float chairKeyLightIntensity = 2.6f;
    [SerializeField] private float tableKeyLightIntensity = 2.1f;
    [SerializeField] private float interviewerKeyLightIntensity = 3.0f;

    private InterviewGameManager gameManager;
    private CybersecurityPresalesInterviewFlow interviewFlow;
    private InterviewerPlaceholder[] interviewers;
    private Renderer chairHighlightRenderer;
    private Coroutine sitCoroutine;
    private bool isSeated;
    private string promptText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != BootstrapSceneName
            || FindAnyObjectByType<TheRoomPrototypeController>() != null)
        {
            return;
        }

        new GameObject("The Room Prototype Controller").AddComponent<TheRoomPrototypeController>();
    }

    private void Awake()
    {
        BuildPrototypeIfNeeded();
    }

    private void Start()
    {
        gameManager = FindAnyObjectByType<InterviewGameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("Final Round RC7: InterviewGameManager was not found. Room HUD and pause UI integration may be limited.");
        }

        interviewFlow = GetComponent<CybersecurityPresalesInterviewFlow>();
        if (interviewFlow == null)
        {
            interviewFlow = gameObject.AddComponent<CybersecurityPresalesInterviewFlow>();
        }

        ResetRoomState();
    }

    private void Update()
    {
        SyncUiFocus();

        if (WasResetPressed())
        {
            ResetRun();
            return;
        }

        if (IsUiFocusActive())
        {
            promptText = string.Empty;
            UpdateChairHighlight(false);
            return;
        }

        if (isSeated || interviewSeat == null || playerController == null)
        {
            promptText = string.Empty;
            return;
        }

        Camera playerCamera = playerController.PlayerCamera;
        Transform viewTransform = playerCamera == null ? playerController.transform : playerCamera.transform;
        bool canSit = interviewSeat.CanInteract(playerController.transform, viewTransform);
        promptText = canSit ? "Press E to sit" : string.Empty;
        UpdateChairHighlight(canSit);

        if (canSit && WasInteractPressed())
        {
            SitForInterview();
        }
    }

    private void SyncUiFocus()
    {
        if (playerController == null)
        {
            return;
        }

        playerController.SetUiFocusActive(IsUiFocusActive());
    }

    private bool IsUiFocusActive()
    {
        return (gameManager != null && gameManager.IsRoomUiFocusActive())
            || (interviewFlow != null && interviewFlow.IsDebugPanelVisible);
    }

    public void SetInterviewerReaction(int index, InterviewerReaction reaction)
    {
        if (interviewers == null || index < 0 || index >= interviewers.Length || interviewers[index] == null)
        {
            return;
        }

        interviewers[index].SetReaction(reaction);
    }

    public void SetAllInterviewers(InterviewerReaction reaction)
    {
        if (interviewers == null)
        {
            return;
        }

        for (int i = 0; i < interviewers.Length; i++)
        {
            SetInterviewerReaction(i, reaction);
        }
    }

    public void ApplyJudgementReaction(ReactionSpeaker speaker, ReactionTone tone)
    {
        InterviewerReaction reaction = tone switch
        {
            ReactionTone.Positive => InterviewerReaction.Positive,
            ReactionTone.Awkward => InterviewerReaction.Awkward,
            ReactionTone.Concerned => InterviewerReaction.Concerned,
            _ => InterviewerReaction.Listening
        };

        SetAllInterviewers(InterviewerReaction.Listening);

        switch (speaker)
        {
            case ReactionSpeaker.HiringManager:
                SetInterviewerReaction(0, reaction);
                break;
            case ReactionSpeaker.SecurityArchitect:
                SetInterviewerReaction(1, reaction);
                break;
            case ReactionSpeaker.SalesDirector:
                SetInterviewerReaction(2, reaction);
                break;
            default:
                SetAllInterviewers(reaction);
                break;
        }
    }

    public void ClearJudgementReaction()
    {
        if (isSeated)
        {
            SetAllInterviewers(InterviewerReaction.Listening);
        }
        else
        {
            SetAllInterviewers(InterviewerReaction.Neutral);
        }
    }

    public void SetRoomObjectiveText(string objectiveText)
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<InterviewGameManager>();
        }

        if (gameManager != null)
        {
            gameManager.SetRoomObjectiveText(objectiveText);
        }
    }

    private void SitForInterview()
    {
        if (seatedCameraPoint == null)
        {
            return;
        }

        isSeated = true;
        promptText = string.Empty;
        UpdateChairHighlight(false);
        playerController.SetMovementEnabled(false);
        SetAllInterviewers(InterviewerReaction.Listening);

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<InterviewGameManager>();
        }

        SetRoomObjectiveText("Interview in progress.");

        sitCoroutine = StartCoroutine(SnapToSeatedView());
    }

    private IEnumerator SnapToSeatedView()
    {
        Camera camera = playerController.PlayerCamera;
        Transform cameraTransform = camera == null ? playerController.transform : camera.transform;
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;
        float elapsed = 0f;

        while (elapsed < sitSnapDuration)
        {
            elapsed += Time.deltaTime;
            float t = sitSnapDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / sitSnapDuration);
            t = t * t * (3f - 2f * t);
            cameraTransform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, seatedCameraPoint.position, t),
                Quaternion.Slerp(startRotation, seatedCameraPoint.rotation, t));
            yield return null;
        }

        cameraTransform.SetPositionAndRotation(seatedCameraPoint.position, seatedCameraPoint.rotation);
        yield return new WaitForSeconds(QuestionRevealDelay);

        if (interviewFlow == null)
        {
            interviewFlow = GetComponent<CybersecurityPresalesInterviewFlow>();
        }

        if (interviewFlow != null)
        {
            interviewFlow.BeginInterview();
        }

        sitCoroutine = null;
    }

    public void ResetRun()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<InterviewGameManager>();
        }

        if (sitCoroutine != null)
        {
            StopCoroutine(sitCoroutine);
            sitCoroutine = null;
        }

        if (gameManager != null)
        {
            gameManager.ResetRoomPrototypeRun();
        }

        if (interviewFlow != null)
        {
            interviewFlow.ResetFlow();
        }

        ResetRoomState();
    }

    private void ResetRoomState()
    {
        isSeated = false;
        promptText = string.Empty;
        SetAllInterviewers(InterviewerReaction.Neutral);
        UpdateChairHighlight(false);

        if (playerController == null || standingStartPoint == null)
        {
            return;
        }

        playerController.WarpTo(standingStartPoint.position, standingStartPoint.rotation);
        playerController.SetUiFocusActive(false);
        playerController.SetMovementEnabled(true);
    }

    private void UpdateChairHighlight(bool active)
    {
        if (chairHighlightRenderer == null)
        {
            return;
        }

        Color color = active ? chairMarkerActiveColor : chairMarkerIdleColor;
        chairHighlightRenderer.material.color = color;
        if (chairHighlightRenderer.material.HasProperty("_BaseColor"))
        {
            chairHighlightRenderer.material.SetColor("_BaseColor", color);
        }
        if (chairHighlightRenderer.material.HasProperty("_Color"))
        {
            chairHighlightRenderer.material.SetColor("_Color", color);
        }
    }

    private void BuildPrototypeIfNeeded()
    {
        if (playerController != null && interviewSeat != null && seatedCameraPoint != null)
        {
            return;
        }

        ConfigureReadablePrototypeLighting();

        Transform root = new GameObject("The Room Greybox").transform;
        root.SetParent(transform, false);

        standingStartPoint = CreateMarker("Standing Start Point", new Vector3(-0.28f, 1.05f, -7.65f), Quaternion.Euler(0f, 2.5f, 0f), root);
        seatedCameraPoint = CreateMarker("Seated Camera Point", new Vector3(0f, 1.45f, -0.92f), Quaternion.Euler(8f, 0f, 0f), root);

        CreateRoomShell(root);
        CreateFurniture(root);
        interviewers = CreateInterviewers(root);
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.GetComponent<Camera>();
        }

        GameObject player = new GameObject("Room Player", typeof(CharacterController), typeof(SimpleFirstPersonWalkController));
        player.transform.position = standingStartPoint.position;
        player.transform.rotation = standingStartPoint.rotation;

        CharacterController characterController = player.GetComponent<CharacterController>();
        characterController.height = 1.75f;
        characterController.radius = 0.32f;
        characterController.center = new Vector3(0f, 0.88f, 0f);

        mainCamera.transform.SetParent(player.transform, false);
        mainCamera.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        mainCamera.transform.localRotation = Quaternion.identity;
        mainCamera.nearClipPlane = 0.03f;
        mainCamera.fieldOfView = 66f;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color32(24, 26, 30, 255);
        DisableSceneViewCameraHelpers(mainCamera);

        playerController = player.GetComponent<SimpleFirstPersonWalkController>();
        playerController.ConfigureCamera(mainCamera);
    }

    private static void ConfigureReadablePrototypeLighting()
    {
        RenderSettings.fog = false;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color32(150, 154, 164, 255);
        RenderSettings.ambientIntensity = 1.2f;
    }

    private static void DisableSceneViewCameraHelpers(Camera mainCamera)
    {
        Behaviour[] behaviours = mainCamera.GetComponents<Behaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];
            if (behaviour != null && behaviour.GetType().Name == "FreeCamera")
            {
                behaviour.enabled = false;
            }
        }
    }

    private void CreateRoomShell(Transform root)
    {
        CreateCube("Hall Floor", new Vector3(0f, -0.05f, -5.1f), new Vector3(4.8f, 0.1f, 6.8f), new Color32(56, 58, 64, 255), root);
        CreateCube("Hall Runner Strip", new Vector3(0f, 0.012f, -5.1f), new Vector3(1.65f, 0.025f, 6.45f), new Color32(36, 42, 48, 255), root);
        CreateCube("Room Floor", new Vector3(0f, -0.05f, 0.7f), new Vector3(6.8f, 0.1f, 6.0f), new Color32(44, 47, 54, 255), root);
        CreateCube("Room Perimeter Line", new Vector3(0f, 0.018f, -2.17f), new Vector3(6.6f, 0.025f, 0.055f), new Color32(96, 104, 116, 255), root);
        CreateCube("Back Wall", new Vector3(0f, 1.5f, 3.65f), new Vector3(6.8f, 3f, 0.12f), new Color32(70, 74, 82, 255), root);
        CreateCube("Left Wall", new Vector3(-3.4f, 1.5f, 0.7f), new Vector3(0.12f, 3f, 6.0f), new Color32(61, 65, 73, 255), root);
        CreateCube("Right Wall", new Vector3(3.4f, 1.5f, 0.7f), new Vector3(0.12f, 3f, 6.0f), new Color32(61, 65, 73, 255), root);
        CreateCube("Left Hall Wall", new Vector3(-2.4f, 1.45f, -5.55f), new Vector3(0.12f, 2.9f, 6.2f), new Color32(48, 52, 60, 255), root);
        CreateCube("Right Hall Wall", new Vector3(2.4f, 1.45f, -5.55f), new Vector3(0.12f, 2.9f, 6.2f), new Color32(48, 52, 60, 255), root);
        CreateCube("Front Left Wall", new Vector3(-2.25f, 1.5f, -2.25f), new Vector3(2.3f, 3f, 0.12f), new Color32(65, 68, 76, 255), root);
        CreateCube("Front Right Wall", new Vector3(2.25f, 1.5f, -2.25f), new Vector3(2.3f, 3f, 0.12f), new Color32(65, 68, 76, 255), root);
        CreateCube("Door Header", new Vector3(0f, 2.45f, -2.25f), new Vector3(2.2f, 1.1f, 0.12f), new Color32(65, 68, 76, 255), root);
        CreateCube("Door Frame Left", new Vector3(-1.12f, 1.15f, -2.36f), new Vector3(0.08f, 2.3f, 0.12f), new Color32(82, 94, 106, 255), root);
        CreateCube("Door Frame Right", new Vector3(1.12f, 1.15f, -2.36f), new Vector3(0.08f, 2.3f, 0.12f), new Color32(82, 94, 106, 255), root);
        CreateCube("Door Threshold Highlight", new Vector3(0f, 0.025f, -2.34f), new Vector3(2.08f, 0.035f, 0.16f), new Color32(70, 100, 104, 255), root);
        CreateCube("Ceiling Soft Panel", new Vector3(0f, 3f, 0.6f), new Vector3(6.8f, 0.08f, 6.0f), new Color32(52, 55, 62, 255), root);
        CreateCube("Wall Screen", new Vector3(0f, 1.82f, 3.55f), new Vector3(2.35f, 0.86f, 0.055f), new Color32(24, 31, 40, 255), root);
        CreateCube("Wall Screen Glow", new Vector3(0f, 1.82f, 3.51f), new Vector3(2.05f, 0.62f, 0.025f), new Color32(34, 70, 76, 255), root);
        CreateCube("Interviewer Backlight Strip", new Vector3(0f, 1.18f, 3.49f), new Vector3(4.72f, 0.075f, 0.035f), new Color32(78, 126, 132, 255), root);
        CreateCube("Whiteboard", new Vector3(-2.35f, 1.68f, 3.54f), new Vector3(1.15f, 0.76f, 0.05f), new Color32(150, 156, 160, 255), root);
        CreateCube("Meeting Room Sign", new Vector3(1.72f, 1.58f, -2.42f), new Vector3(0.86f, 0.36f, 0.035f), new Color32(78, 90, 102, 255), root);
        CreateWorldLabel("FINAL ROUND", new Vector3(1.72f, 1.585f, -2.47f), Quaternion.Euler(0f, 0f, 0f), 0.075f, 2.8f, new Color32(226, 234, 238, 255), root);

        CreatePointLight("Hall Guide Light", new Vector3(0f, 2.25f, -5.1f), 5.5f, 3.5f, new Color32(226, 238, 255, 255), root);
        CreatePointLight("Doorway Guide Light", new Vector3(0f, 2.35f, -2.1f), 5.2f, 2.3f, new Color32(255, 242, 220, 255), root);
        CreatePointLight("Chair Key Light", new Vector3(0f, 1.55f, -1.75f), 2.8f, chairKeyLightIntensity, new Color32(112, 220, 194, 255), root);
        CreatePointLight("Table Key Light", new Vector3(0f, 2.18f, 0.52f), 4.2f, tableKeyLightIntensity, new Color32(255, 236, 205, 255), root);
        CreatePointLight("Panel Fill Light", new Vector3(0f, 2.15f, 2.45f), 5f, interviewerKeyLightIntensity, new Color32(176, 218, 255, 255), root);
    }

    private static void CreatePointLight(string name, Vector3 position, float range, float intensity, Color color, Transform parent)
    {
        Light light = new GameObject(name, typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(parent, false);
        light.transform.localPosition = position;
        light.type = LightType.Point;
        light.range = range;
        light.intensity = intensity;
        light.color = color;
    }

    private void CreateFurniture(Transform root)
    {
        CreateCube("Interview Table", new Vector3(0f, 0.72f, 0.95f), new Vector3(4.65f, 0.18f, 1.72f), new Color32(94, 76, 58, 255), root);
        CreateCube("Table Light Edge", new Vector3(0f, 0.83f, -0.02f), new Vector3(4.7f, 0.055f, 0.055f), new Color32(154, 126, 90, 255), root);
        CreateCube("Table Front Modesty Panel", new Vector3(0f, 0.38f, 0.25f), new Vector3(4.35f, 0.62f, 0.12f), new Color32(58, 45, 35, 255), root);
        CreateCube("Table Left Leg", new Vector3(-1.85f, 0.34f, 1.52f), new Vector3(0.16f, 0.68f, 0.16f), new Color32(56, 43, 34, 255), root);
        CreateCube("Table Right Leg", new Vector3(1.85f, 0.34f, 1.52f), new Vector3(0.16f, 0.68f, 0.16f), new Color32(56, 43, 34, 255), root);
        CreateCube("Candidate Chair Seat", new Vector3(0f, 0.4f, -1.35f), new Vector3(0.74f, 0.18f, 0.7f), new Color32(58, 86, 104, 255), root);
        CreateCube("Candidate Chair Back", new Vector3(0f, 0.91f, -1.68f), new Vector3(0.78f, 0.86f, 0.13f), new Color32(62, 92, 110, 255), root);
        CreateCube("Candidate Chair Left Arm", new Vector3(-0.48f, 0.61f, -1.34f), new Vector3(0.08f, 0.3f, 0.62f), new Color32(44, 64, 78, 255), root);
        CreateCube("Candidate Chair Right Arm", new Vector3(0.48f, 0.61f, -1.34f), new Vector3(0.08f, 0.3f, 0.62f), new Color32(44, 64, 78, 255), root);
        CreateCube("Chair Back Highlight", new Vector3(0f, 1.08f, -1.765f), new Vector3(0.58f, 0.055f, 0.035f), new Color32(116, 196, 184, 255), root);
        chairHighlightRenderer = CreateCube("Interview Chair Floor Marker", new Vector3(0f, 0.012f, -1.35f), new Vector3(1.22f, 0.025f, 1.02f), chairMarkerIdleColor, root).GetComponent<Renderer>();
        CreateCube("Chair Direction Arrow", new Vector3(0f, 0.03f, -0.74f), new Vector3(0.46f, 0.025f, 0.1f), new Color32(94, 176, 160, 190), root);
        CreateCube("Laptop Base", new Vector3(-1.25f, 0.86f, 0.68f), new Vector3(0.66f, 0.045f, 0.42f), new Color32(26, 30, 36, 255), root);
        CreateCube("Laptop Screen", new Vector3(-1.25f, 1.08f, 0.87f), new Vector3(0.66f, 0.42f, 0.045f), new Color32(36, 74, 82, 255), root);
        CreateCube("Candidate Notepad", new Vector3(0.82f, 0.855f, 0.2f), new Vector3(0.46f, 0.03f, 0.32f), new Color32(190, 184, 154, 255), root);
        CreateCube("Notepad Line 1", new Vector3(0.82f, 0.88f, 0.14f), new Vector3(0.36f, 0.012f, 0.018f), new Color32(82, 88, 92, 255), root);
        CreateCube("Water Glass", new Vector3(1.34f, 0.93f, 0.28f), new Vector3(0.16f, 0.22f, 0.16f), new Color32(130, 170, 184, 180), root);

        GameObject seatObject = new GameObject("InterviewChair");
        seatObject.transform.SetParent(root, false);
        seatObject.transform.localPosition = new Vector3(0f, 0.55f, -1.25f);
        interviewSeat = seatObject.AddComponent<InterviewSeat>();
        interviewSeat.Configure(seatedCameraPoint, 2.25f, 38f);
    }

    private InterviewerPlaceholder[] CreateInterviewers(Transform root)
    {
        InterviewerPlaceholder[] placeholders = new InterviewerPlaceholder[3];
        Vector3[] positions =
        {
            new Vector3(-1.68f, 1.22f, 2.1f),
            new Vector3(0f, 1.26f, 2.24f),
            new Vector3(1.68f, 1.22f, 2.1f)
        };
        CreateCube("Interviewer Side Dais", new Vector3(0f, 0.08f, 2.42f), new Vector3(5.4f, 0.16f, 0.84f), new Color32(32, 36, 44, 255), root);

        for (int i = 0; i < placeholders.Length; i++)
        {
            GameObject placeholder = new GameObject($"Interviewer Placeholder {i + 1}");
            placeholder.transform.SetParent(root, false);
            placeholder.transform.localPosition = positions[i];
            placeholder.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Renderer panel = CreateCube("Panel", Vector3.zero, new Vector3(0.98f, 1.28f, 0.08f), new Color32(44, 50, 62, 255), placeholder.transform).GetComponent<Renderer>();
            CreateCube("Panel Top Edge", new Vector3(0f, 0.66f, -0.055f), new Vector3(1.02f, 0.045f, 0.035f), new Color32(86, 102, 116, 255), placeholder.transform);
            Renderer head = CreateSphere("Avatar Head", new Vector3(0f, 0.16f, -0.18f), new Vector3(0.3f, 0.34f, 0.3f), new Color32(178, 184, 194, 255), placeholder.transform).GetComponent<Renderer>();
            CreateCapsule("Avatar Body", new Vector3(0f, -0.29f, -0.18f), new Vector3(0.4f, 0.36f, 0.2f), new Color32(94, 103, 118, 255), placeholder.transform);
            Renderer reaction = CreateCube("Reaction Hook Strip", new Vector3(0f, -0.58f, -0.22f), new Vector3(0.78f, 0.065f, 0.04f), new Color32(72, 82, 96, 255), placeholder.transform).GetComponent<Renderer>();
            CreateCube("Nameplate Backing", new Vector3(0f, 0.78f, -0.225f), new Vector3(1.26f, 0.24f, 0.035f), new Color32(28, 34, 42, 255), placeholder.transform);
            CreateInterviewerLabel(GetInterviewerLabel(i), new Vector3(0f, 0.775f, -0.268f), placeholder.transform);

            placeholders[i] = placeholder.AddComponent<InterviewerPlaceholder>();
            placeholders[i].Configure(panel, head, reaction);
        }

        return placeholders;
    }

    private static string GetInterviewerLabel(int index)
    {
        return index switch
        {
            0 => "Hiring Manager",
            1 => "Principal Security Architect",
            _ => "Sales Director"
        };
    }

    private static void CreateInterviewerLabel(string label, Vector3 position, Transform parent)
    {
        CreateWorldLabel(label, position, Quaternion.identity, 0.088f, 2.15f, new Color32(232, 238, 244, 255), parent);
    }

    private static void CreateWorldLabel(string label, Vector3 position, Quaternion rotation, float scale, float fontSize, Color color, Transform parent)
    {
        GameObject labelObject = new GameObject(label + " Label", typeof(TextMeshPro));
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = position;
        labelObject.transform.localRotation = rotation;
        labelObject.transform.localScale = Vector3.one * scale;

        TextMeshPro text = labelObject.GetComponent<TextMeshPro>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = color;
        text.rectTransform.sizeDelta = new Vector2(8.5f, 1.2f);
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(promptText))
        {
            return;
        }

        GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold
        };
        promptStyle.normal.textColor = new Color32(228, 248, 242, 255);
        GUI.Label(new Rect(Screen.width * 0.5f - 130f, Screen.height - 104f, 260f, 32f), promptText, promptStyle);
    }

    private static Transform CreateMarker(string name, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = position;
        marker.transform.localRotation = rotation;
        return marker.transform;
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().material = CreateMaterial(color);
        return cube;
    }

    private static GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent, false);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = scale;
        sphere.GetComponent<Renderer>().material = CreateMaterial(color);
        return sphere;
    }

    private static GameObject CreateCapsule(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = name;
        capsule.transform.SetParent(parent, false);
        capsule.transform.localPosition = position;
        capsule.transform.localScale = scale;
        capsule.GetComponent<Renderer>().material = CreateMaterial(color);
        return capsule;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = "Room Prototype Material";
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private static bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private static bool WasResetPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}
