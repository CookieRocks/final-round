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
    private const float PrefabBoundsPadding = 0.96f;

    private enum PrefabAnchor
    {
        Center,
        BottomCenter,
        BackWallCenter,
        FrontWallCenter
    }

    [SerializeField] private SimpleFirstPersonWalkController playerController;
    [SerializeField] private InterviewSeat interviewSeat;
    [SerializeField] private Transform standingStartPoint;
    [SerializeField] private Transform seatedCameraPoint;
    [SerializeField] private float sitSnapDuration = 0.5f;
    [Header("RC16 Room Asset Slots")]
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject meetingTablePrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject candidateChairPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject interviewerPanelPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject laptopPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject notepadPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject waterGlassPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject wallScreenPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject whiteboardPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject roomSignPrefab;
    [InspectorName("Door/Doorframe Prefab")]
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject doorDoorframePrefab;
    [InspectorName("Corner Prop/Plant Prefab")]
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject plantOrCornerPropPrefab;
    [Tooltip("Drag a prefab asset from the Project window into this field. Leave empty to use generated fallback.")]
    [SerializeField] private GameObject ceilingLightPrefab;
    [Header("RC18 Interviewer Presence")]
    [Tooltip("Optional static or minimally animated human prefab for the Hiring Manager. Empty uses the panel fallback.")]
    [SerializeField] private GameObject hiringManagerPrefab;
    [Tooltip("Optional static or minimally animated human prefab for the Principal Security Architect. Empty uses the panel fallback.")]
    [SerializeField] private GameObject principalSecurityArchitectPrefab;
    [Tooltip("Optional static or minimally animated human prefab for the Sales Director. Empty uses the panel fallback.")]
    [SerializeField] private GameObject salesDirectorPrefab;
    [SerializeField] private Vector3 hiringManagerPositionOffset = new Vector3(-0.04f, 0.04f, 0.02f);
    [SerializeField] private Vector3 hiringManagerRotationOffset = new Vector3(0f, -6f, 1.5f);
    [SerializeField] private Vector3 hiringManagerScaleMultiplier = new Vector3(1.09f, 1.09f, 1.09f);
    [SerializeField] private Vector3 principalSecurityArchitectPositionOffset = new Vector3(0f, 0.06f, -0.02f);
    [SerializeField] private Vector3 principalSecurityArchitectRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 principalSecurityArchitectScaleMultiplier = new Vector3(1.13f, 1.13f, 1.13f);
    [SerializeField] private Vector3 salesDirectorPositionOffset = new Vector3(0.04f, 0.04f, 0.02f);
    [SerializeField] private Vector3 salesDirectorRotationOffset = new Vector3(0f, 6f, -1.5f);
    [SerializeField] private Vector3 salesDirectorScaleMultiplier = new Vector3(1.09f, 1.09f, 1.09f);
    [Tooltip("When enabled, assigned interviewer character prefabs are uniformly scaled and centered into the role slot before offsets are applied.")]
    [SerializeField] private bool autoFitInterviewerCharacterPrefabs = true;
    [Tooltip("Character prefabs are usually best fitted by visible height; imported width/depth bounds can include bind-pose or accessory extents.")]
    [SerializeField] private bool interviewerCharacterFitByHeight = true;
    [SerializeField] private float interviewerCharacterTargetHeight = 1.9f;
    [SerializeField] private Vector3 interviewerCharacterTargetBounds = new Vector3(0.88f, 1.9f, 0.66f);
    [SerializeField] private Vector3 interviewerCharacterTargetCenter = new Vector3(0f, 0f, 0.02f);
    [SerializeField] private float interviewerCharacterFloorY = -0.33f;
    [Tooltip("Disables Animator components on assigned interviewer prefabs so imported idle/sway clips do not distract during the interview.")]
    [SerializeField] private bool disableAssignedInterviewerAnimators = true;
    [Tooltip("Adds a simple chair backing behind assigned interviewer prefabs to sell a seated interview composition without requiring a seated rig pose.")]
    [SerializeField] private bool addChairBackForAssignedInterviewerPrefabs = true;
    [Header("RC21 Character Material And Light")]
    [SerializeField] private Color hiringManagerCharacterTint = new Color32(86, 78, 66, 255);
    [SerializeField] private Color principalSecurityArchitectCharacterTint = new Color32(60, 78, 96, 255);
    [SerializeField] private Color salesDirectorCharacterTint = new Color32(88, 68, 82, 255);
    [Range(0f, 1f)]
    [SerializeField] private float assignedInterviewerCharacterTintStrength = 0.72f;
    [SerializeField] private float interviewerCharacterLightIntensity = 0.04f;
    [SerializeField] private bool replaceAssignedInterviewerTexturesWithTint;
    [Header("RC16 Prefab Placement Tuning")]
    [Tooltip("Adjusts only the imported candidate chair prefab. Use this when a chair prefab pivot makes the chair float or sink. Generated fallback is unaffected.")]
    [SerializeField] private Vector3 candidateChairPrefabPositionOffset = Vector3.zero;
    [Tooltip("Adjusts only the imported candidate chair prefab rotation. Generated fallback is unaffected.")]
    [SerializeField] private Vector3 candidateChairPrefabRotationOffset = Vector3.zero;
    [Tooltip("Adjusts only the imported candidate chair prefab scale. Generated fallback is unaffected.")]
    [SerializeField] private Vector3 candidateChairPrefabScale = Vector3.one;
    [Tooltip("When enabled, the imported candidate chair prefab is lowered or raised so its visible bottom sits on the room floor.")]
    [SerializeField] private bool snapCandidateChairPrefabToFloor = true;
    [Tooltip("World/local room floor height used when snapping the imported candidate chair prefab.")]
    [SerializeField] private float candidateChairPrefabFloorY = 0.03f;
    [Header("RC8 Room Readability")]
    [SerializeField] private Color chairMarkerIdleColor = new Color32(62, 130, 118, 150);
    [SerializeField] private Color chairMarkerActiveColor = new Color32(112, 238, 206, 255);
    [SerializeField] private float chairKeyLightIntensity = 2.6f;
    [SerializeField] private float tableKeyLightIntensity = 2.1f;
    [SerializeField] private float interviewerKeyLightIntensity = 2.05f;
    [Header("RC16 Room Life")]
    [SerializeField] private bool roomLifeEnabled = true;
    [SerializeField] private float chairMarkerPulseAmount = 0.22f;
    [SerializeField] private float laptopGlowPulseAmount = 0.18f;
    [SerializeField] private float wallScreenFlickerAmount = 0.12f;
    [SerializeField] private AudioClip roomHumClip;
    [Range(0f, 1f)]
    [SerializeField] private float roomHumVolume = 0.08f;

    private InterviewGameManager gameManager;
    private CybersecurityPresalesInterviewFlow interviewFlow;
    private InterviewerPlaceholder[] interviewers;
    private Renderer chairHighlightRenderer;
    private Renderer chairBackHighlightRenderer;
    private Renderer laptopScreenRenderer;
    private Renderer wallScreenGlowRenderer;
    private Light laptopGlowLight;
    private Light wallScreenGlowLight;
    private AudioSource roomHumSource;
    private Coroutine sitCoroutine;
    private bool isSeated;
    private bool chairHighlightActive;
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
        if (!EnsureSingleActiveController())
        {
            return;
        }

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

        LogRc16PrefabSlotStatus();
        ConfigureRoomHum();
        ResetRoomState();
    }

    private void Update()
    {
        SyncUiFocus();
        UpdateRoomLife();

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
        chairHighlightActive = active;
        SetRendererColor(chairHighlightRenderer, color);
    }

    private void UpdateRoomLife()
    {
        if (!roomLifeEnabled)
        {
            return;
        }

        float chairPulse = 1f + Mathf.Sin(Time.time * 2.4f) * chairMarkerPulseAmount;
        if (chairHighlightRenderer != null)
        {
            Color baseColor = chairHighlightActive ? chairMarkerActiveColor : chairMarkerIdleColor;
            SetRendererColor(chairHighlightRenderer, Color.Lerp(baseColor, Color.white, chairHighlightActive ? chairPulse * 0.12f : chairPulse * 0.04f));
        }
        if (chairBackHighlightRenderer != null)
        {
            Color backColor = Color.Lerp(new Color32(76, 128, 122, 255), new Color32(140, 226, 210, 255), chairHighlightActive ? chairPulse * 0.26f : chairPulse * 0.08f);
            SetRendererColor(chairBackHighlightRenderer, backColor);
        }

        float laptopPulse = 1f + Mathf.Sin(Time.time * 1.7f + 1.3f) * laptopGlowPulseAmount;
        if (laptopScreenRenderer != null)
        {
            SetRendererColor(laptopScreenRenderer, Color.Lerp(new Color32(36, 74, 82, 255), new Color32(110, 210, 200, 255), laptopPulse * 0.24f));
        }
        if (laptopGlowLight != null)
        {
            laptopGlowLight.intensity = 0.45f + laptopPulse * 0.12f;
        }

        float screenPulse = 1f + Mathf.PerlinNoise(Time.time * 0.8f, 0.42f) * wallScreenFlickerAmount;
        if (wallScreenGlowRenderer != null)
        {
            SetRendererColor(wallScreenGlowRenderer, Color.Lerp(new Color32(34, 70, 76, 255), new Color32(88, 150, 160, 255), screenPulse * 0.18f));
        }
        if (wallScreenGlowLight != null)
        {
            wallScreenGlowLight.intensity = 0.45f + screenPulse * 0.1f;
        }
    }

    private void BuildPrototypeIfNeeded()
    {
        if (playerController != null && interviewSeat != null && seatedCameraPoint != null)
        {
            return;
        }

        ConfigureReadablePrototypeLighting();
        ConfigureRoomHum();

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
        if (!InstantiateFittedPrefab(doorDoorframePrefab, "Door/Doorframe Prefab", new Vector3(0f, 0.03f, -2.36f), Quaternion.identity, new Vector3(2.35f, 2.45f, 0.24f), PrefabAnchor.BottomCenter, root))
        {
            CreateCube("Door Frame Left", new Vector3(-1.12f, 1.15f, -2.36f), new Vector3(0.08f, 2.3f, 0.12f), new Color32(82, 94, 106, 255), root);
            CreateCube("Door Frame Right", new Vector3(1.12f, 1.15f, -2.36f), new Vector3(0.08f, 2.3f, 0.12f), new Color32(82, 94, 106, 255), root);
            CreateCube("Door Threshold Highlight", new Vector3(0f, 0.025f, -2.34f), new Vector3(2.08f, 0.035f, 0.16f), new Color32(70, 100, 104, 255), root);
        }
        if (!InstantiateFittedPrefab(ceilingLightPrefab, "Ceiling Light Prefab", new Vector3(0f, 2.92f, 0.55f), Quaternion.identity, new Vector3(2.1f, 0.32f, 1.15f), PrefabAnchor.Center, root))
        {
            CreateCube("Ceiling Soft Panel", new Vector3(0f, 3f, 0.6f), new Vector3(6.8f, 0.08f, 6.0f), new Color32(52, 55, 62, 255), root);
        }
        GameObject wallScreenInstance = InstantiateFittedPrefab(wallScreenPrefab, "Wall Screen Prefab", new Vector3(0f, 1.82f, 3.58f), Quaternion.identity, new Vector3(2.35f, 0.88f, 0.16f), PrefabAnchor.BackWallCenter, root);
        if (wallScreenInstance == null)
        {
            CreateCube("Wall Screen", new Vector3(0f, 1.82f, 3.55f), new Vector3(2.35f, 0.86f, 0.055f), new Color32(24, 31, 40, 255), root);
        }
        wallScreenGlowRenderer = CreateCube("Wall Screen Glow", new Vector3(0f, 1.82f, 3.51f), new Vector3(2.05f, 0.62f, 0.025f), new Color32(34, 70, 76, 255), root).GetComponent<Renderer>();
        wallScreenGlowLight = CreatePointLight("Wall Screen Idle Glow", new Vector3(0f, 1.85f, 3.05f), 2.1f, 0.55f, new Color32(80, 170, 180, 255), root);
        CreateCube("Interviewer Backlight Strip", new Vector3(0f, 1.18f, 3.49f), new Vector3(4.72f, 0.075f, 0.035f), new Color32(78, 126, 132, 255), root);
        if (!InstantiateFittedPrefab(whiteboardPrefab, "Whiteboard Prefab", new Vector3(-2.35f, 1.68f, 3.58f), Quaternion.identity, new Vector3(1.2f, 0.8f, 0.12f), PrefabAnchor.BackWallCenter, root))
        {
            CreateCube("Whiteboard", new Vector3(-2.35f, 1.68f, 3.54f), new Vector3(1.15f, 0.76f, 0.05f), new Color32(150, 156, 160, 255), root);
            CreateCube("Whiteboard Architecture Line", new Vector3(-2.35f, 1.72f, 3.5f), new Vector3(0.82f, 0.018f, 0.035f), new Color32(70, 88, 96, 255), root);
            CreateCube("Whiteboard Risk Box", new Vector3(-2.62f, 1.58f, 3.5f), new Vector3(0.22f, 0.16f, 0.035f), new Color32(78, 116, 126, 255), root);
        }
        if (!InstantiateFittedPrefab(roomSignPrefab, "Room Sign Prefab", new Vector3(1.72f, 1.58f, -2.42f), Quaternion.identity, new Vector3(0.86f, 0.38f, 0.08f), PrefabAnchor.FrontWallCenter, root))
        {
            CreateCube("Meeting Room Sign", new Vector3(1.72f, 1.58f, -2.42f), new Vector3(0.86f, 0.36f, 0.035f), new Color32(78, 90, 102, 255), root);
            CreateWorldLabel("FINAL ROUND", new Vector3(1.72f, 1.585f, -2.47f), Quaternion.Euler(0f, 0f, 0f), 0.075f, 2.8f, new Color32(226, 234, 238, 255), root);
        }
        if (!InstantiateFittedPrefab(plantOrCornerPropPrefab, "Plant/Corner Prop Prefab", new Vector3(2.78f, 0.03f, 2.78f), Quaternion.Euler(0f, -22f, 0f), new Vector3(0.56f, 0.82f, 0.56f), PrefabAnchor.BottomCenter, root))
        {
            CreateCube("Corner Plant Pot", new Vector3(2.78f, 0.22f, 2.78f), new Vector3(0.32f, 0.44f, 0.32f), new Color32(46, 50, 56, 255), root);
            CreateCube("Corner Plant Silhouette", new Vector3(2.78f, 0.68f, 2.78f), new Vector3(0.46f, 0.58f, 0.08f), new Color32(38, 72, 60, 255), root);
        }

        CreatePointLight("Hall Guide Light", new Vector3(0f, 2.25f, -5.1f), 5.5f, 3.5f, new Color32(226, 238, 255, 255), root);
        CreatePointLight("Doorway Guide Light", new Vector3(0f, 2.35f, -2.1f), 5.2f, 2.3f, new Color32(255, 242, 220, 255), root);
        CreatePointLight("Chair Key Light", new Vector3(0f, 1.55f, -1.75f), 2.8f, chairKeyLightIntensity, new Color32(112, 220, 194, 255), root);
        CreatePointLight("Table Key Light", new Vector3(0f, 2.18f, 0.52f), 4.2f, tableKeyLightIntensity, new Color32(255, 236, 205, 255), root);
        CreatePointLight("Panel Fill Light", new Vector3(0f, 2.15f, 2.45f), 5f, interviewerKeyLightIntensity, new Color32(176, 218, 255, 255), root);
    }

    private static Light CreatePointLight(string name, Vector3 position, float range, float intensity, Color color, Transform parent)
    {
        Light light = new GameObject(name, typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(parent, false);
        light.transform.localPosition = position;
        light.type = LightType.Point;
        light.range = range;
        light.intensity = intensity;
        light.color = color;
        return light;
    }

    private void CreateFurniture(Transform root)
    {
        float tableSurfaceY = 0.86f;
        GameObject tableInstance = InstantiateFittedPrefab(meetingTablePrefab, "Meeting Table Prefab", new Vector3(0f, 0.03f, 0.48f), Quaternion.Euler(0f, 90f, 0f), new Vector3(4.45f, 0f, 1.75f), PrefabAnchor.BottomCenter, root);
        if (tableInstance != null)
        {
            ApplyMaterialOverride(tableInstance, new Color32(92, 96, 88, 255));
            if (TryGetRendererBounds(tableInstance, out Bounds tableBounds))
            {
                tableSurfaceY = tableBounds.max.y + 0.012f;
            }
        }
        else
        {
            CreateCube("Interview Table", new Vector3(0f, 0.72f, 0.48f), new Vector3(4.65f, 0.18f, 1.72f), new Color32(94, 76, 58, 255), root);
            CreateCube("Table Light Edge", new Vector3(0f, 0.83f, -0.49f), new Vector3(4.7f, 0.055f, 0.055f), new Color32(154, 126, 90, 255), root);
            CreateCube("Table Front Modesty Panel", new Vector3(0f, 0.38f, -0.22f), new Vector3(4.35f, 0.62f, 0.12f), new Color32(58, 45, 35, 255), root);
            CreateCube("Table Left Leg", new Vector3(-1.85f, 0.34f, 1.05f), new Vector3(0.16f, 0.68f, 0.16f), new Color32(56, 43, 34, 255), root);
            CreateCube("Table Right Leg", new Vector3(1.85f, 0.34f, 1.05f), new Vector3(0.16f, 0.68f, 0.16f), new Color32(56, 43, 34, 255), root);
        }
        CreateCube("Interviewer Table Rear Modesty Panel", new Vector3(0f, tableSurfaceY - 0.33f, 1.28f), new Vector3(4.42f, 0.52f, 0.08f), new Color32(72, 68, 60, 255), root);
        Vector3 candidateChairPosition = new Vector3(0f, 0.62f, -1.42f);
        Quaternion candidateChairRotation = Quaternion.Euler(candidateChairPrefabRotationOffset);
        Vector3 candidateChairScale = new Vector3(
            Mathf.Max(0.01f, candidateChairPrefabScale.x),
            Mathf.Max(0.01f, candidateChairPrefabScale.y),
            Mathf.Max(0.01f, candidateChairPrefabScale.z));
        GameObject candidateChairInstance = InstantiatePrefab(candidateChairPrefab, "Candidate Chair Prefab", candidateChairPosition + candidateChairPrefabPositionOffset, candidateChairRotation, candidateChairScale, root);
        if (candidateChairInstance != null)
        {
            if (snapCandidateChairPrefabToFloor)
            {
                SnapRendererBoundsBottomToY(candidateChairInstance, candidateChairPrefabFloorY);
            }
        }
        else
        {
            CreateCube("Candidate Chair Seat", new Vector3(0f, 0.4f, -1.35f), new Vector3(0.74f, 0.18f, 0.7f), new Color32(58, 86, 104, 255), root);
            CreateCube("Candidate Chair Back", new Vector3(0f, 0.91f, -1.68f), new Vector3(0.78f, 0.86f, 0.13f), new Color32(62, 92, 110, 255), root);
            CreateCube("Candidate Chair Left Arm", new Vector3(-0.48f, 0.61f, -1.34f), new Vector3(0.08f, 0.3f, 0.62f), new Color32(44, 64, 78, 255), root);
            CreateCube("Candidate Chair Right Arm", new Vector3(0.48f, 0.61f, -1.34f), new Vector3(0.08f, 0.3f, 0.62f), new Color32(44, 64, 78, 255), root);
            chairBackHighlightRenderer = CreateCube("Chair Back Highlight", new Vector3(0f, 1.08f, -1.765f), new Vector3(0.58f, 0.055f, 0.035f), new Color32(116, 196, 184, 255), root).GetComponent<Renderer>();
        }
        chairHighlightRenderer = CreateCube("Interview Chair Floor Marker", new Vector3(0f, 0.012f, -1.35f), new Vector3(1.22f, 0.025f, 1.02f), chairMarkerIdleColor, root).GetComponent<Renderer>();
        CreateCube("Chair Direction Arrow", new Vector3(0f, 0.03f, -0.74f), new Vector3(0.46f, 0.025f, 0.1f), new Color32(94, 176, 160, 190), root);
        GameObject laptopInstance = InstantiateFittedPrefab(laptopPrefab, "Laptop Prefab", new Vector3(-1.1f, tableSurfaceY - 0.035f, 0.2f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.58f, 0.3f, 0.38f), PrefabAnchor.BottomCenter, root);
        if (laptopInstance == null)
        {
            CreateCube("Laptop Base", new Vector3(-1.1f, tableSurfaceY + 0.02f, 0.2f), new Vector3(0.66f, 0.045f, 0.42f), new Color32(26, 30, 36, 255), root);
            laptopScreenRenderer = CreateCube("Laptop Screen", new Vector3(-1.1f, tableSurfaceY + 0.24f, 0.39f), new Vector3(0.66f, 0.42f, 0.045f), new Color32(36, 74, 82, 255), root).GetComponent<Renderer>();
        }
        else
        {
            laptopScreenRenderer = GetFirstRenderer(laptopInstance);
        }
        laptopGlowLight = CreatePointLight("Laptop Screen Glow", new Vector3(-1.1f, tableSurfaceY + 0.2f, -0.14f), 1.4f, 0.52f, new Color32(90, 210, 200, 255), root);
        if (!InstantiateFittedPrefab(notepadPrefab, "Notepad Prefab", new Vector3(0.8f, tableSurfaceY - 0.02f, 0.16f), Quaternion.Euler(0f, 10f, 0f), new Vector3(0.46f, 0.06f, 0.32f), PrefabAnchor.BottomCenter, root))
        {
            CreateCube("Candidate Notepad", new Vector3(0.8f, tableSurfaceY + 0.015f, 0.16f), new Vector3(0.46f, 0.03f, 0.32f), new Color32(190, 184, 154, 255), root);
            CreateCube("Notepad Line 1", new Vector3(0.8f, tableSurfaceY + 0.04f, 0.1f), new Vector3(0.36f, 0.012f, 0.018f), new Color32(82, 88, 92, 255), root);
        }
        if (!InstantiateFittedPrefab(waterGlassPrefab, "Water Glass Prefab", new Vector3(1.24f, tableSurfaceY - 0.02f, 0.18f), Quaternion.identity, new Vector3(0.18f, 0.24f, 0.18f), PrefabAnchor.BottomCenter, root))
        {
            CreateCube("Water Glass", new Vector3(1.24f, tableSurfaceY + 0.11f, 0.18f), new Vector3(0.16f, 0.22f, 0.16f), new Color32(130, 170, 184, 180), root);
        }

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
            new Vector3(-1.38f, 0.05f, 1.84f),
            new Vector3(0f, 0.05f, 1.98f),
            new Vector3(1.38f, 0.05f, 1.84f)
        };
        GameObject[] characterPrefabs =
        {
            hiringManagerPrefab,
            principalSecurityArchitectPrefab,
            salesDirectorPrefab
        };
        Vector3[] positionOffsets =
        {
            hiringManagerPositionOffset,
            principalSecurityArchitectPositionOffset,
            salesDirectorPositionOffset
        };
        Vector3[] rotationOffsets =
        {
            hiringManagerRotationOffset,
            principalSecurityArchitectRotationOffset,
            salesDirectorRotationOffset
        };
        Vector3[] scaleMultipliers =
        {
            hiringManagerScaleMultiplier,
            principalSecurityArchitectScaleMultiplier,
            salesDirectorScaleMultiplier
        };
        Color[] characterTints =
        {
            hiringManagerCharacterTint,
            principalSecurityArchitectCharacterTint,
            salesDirectorCharacterTint
        };
        CreateCube("Interviewer Side Dais", new Vector3(0f, 0.08f, 2.18f), new Vector3(4.8f, 0.16f, 0.68f), new Color32(32, 36, 44, 255), root);

        for (int i = 0; i < placeholders.Length; i++)
        {
            GameObject placeholder = new GameObject($"Interviewer Placeholder {i + 1}");
            placeholder.transform.SetParent(root, false);
            placeholder.transform.localPosition = positions[i];
            placeholder.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Renderer panel;
            Renderer head;
            Renderer nameplate = CreateCube("Nameplate Backing", new Vector3(0f, 0.62f, 0.42f), new Vector3(1.26f, 0.24f, 0.035f), GetNameplateColor(i), placeholder.transform).GetComponent<Renderer>();
            CreateInterviewerLabel(GetInterviewerNameplate(i), new Vector3(0f, 0.635f, 0.38f), placeholder.transform);
            CreateInterviewerDeskProp(i, placeholder.transform);

            GameObject characterInstance = InstantiateInterviewerCharacter(
                characterPrefabs[i],
                GetInterviewerLabel(i) + " Character Prefab",
                positionOffsets[i],
                rotationOffsets[i],
                scaleMultipliers[i],
                placeholder.transform);

            if (characterInstance != null)
            {
                ApplyAssignedInterviewerCharacterTint(characterInstance, characterTints[i]);

                if (addChairBackForAssignedInterviewerPrefabs)
                {
                    CreateInterviewerChairBack(placeholder.transform);
                }

                Renderer[] characterRenderers = characterInstance.GetComponentsInChildren<Renderer>();
                panel = null;
                head = characterRenderers.Length > 0 ? characterRenderers[0] : null;
            }
            else if (interviewerPanelPrefab != null)
            {
                GameObject panelInstance = InstantiatePrefab(interviewerPanelPrefab, "Interviewer Panel Prefab", new Vector3(0f, 1.22f, 0f), Quaternion.identity, Vector3.one, placeholder.transform);
                Renderer[] renderers = panelInstance.GetComponentsInChildren<Renderer>();
                panel = renderers.Length > 0 ? renderers[0] : null;
                head = renderers.Length > 1 ? renderers[1] : panel;
            }
            else
            {
                panel = CreateProceduralInterviewerBust(i, placeholder.transform, out head);
            }
            Renderer reaction = CreateCube("Reaction Hook Strip", new Vector3(0f, 0.42f, 0.4f), new Vector3(0.68f, 0.05f, 0.04f), new Color32(72, 82, 96, 255), placeholder.transform).GetComponent<Renderer>();
            Light emphasisLight = CreatePointLight("Interviewer Subtle Emphasis Light", new Vector3(0f, 1.24f, 0.5f), 1.2f, interviewerCharacterLightIntensity, new Color32(118, 206, 190, 255), placeholder.transform);

            placeholders[i] = placeholder.AddComponent<InterviewerPlaceholder>();
            placeholders[i].Configure(panel, head, reaction, characterInstance == null ? null : characterInstance.transform, nameplate, emphasisLight);
        }

        return placeholders;
    }

    private static Renderer CreateProceduralInterviewerBust(int index, Transform parent, out Renderer headRenderer)
    {
        Color jacketColor = index switch
        {
            0 => new Color32(58, 70, 84, 255),
            1 => new Color32(50, 62, 78, 255),
            _ => new Color32(68, 62, 76, 255)
        };
        Color shirtColor = index switch
        {
            0 => new Color32(130, 170, 170, 255),
            1 => new Color32(142, 154, 176, 255),
            _ => new Color32(170, 148, 128, 255)
        };
        Color skinColor = index switch
        {
            0 => new Color32(188, 150, 118, 255),
            1 => new Color32(168, 128, 98, 255),
            _ => new Color32(205, 174, 138, 255)
        };
        Color hairColor = index switch
        {
            0 => new Color32(58, 48, 42, 255),
            1 => new Color32(36, 38, 42, 255),
            _ => new Color32(82, 70, 58, 255)
        };

        CreateInterviewerChairBack(parent);
        Renderer torso = CreateCapsule("Interviewer Torso", new Vector3(0f, 1.01f, 0.02f), new Vector3(0.44f, 0.48f, 0.22f), jacketColor, parent).GetComponent<Renderer>();
        CreateCube("Interviewer Shirt", new Vector3(0f, 1.04f, 0.16f), new Vector3(0.18f, 0.34f, 0.04f), shirtColor, parent);
        CreateSphere("Interviewer Left Shoulder", new Vector3(-0.31f, 1.09f, 0.01f), new Vector3(0.2f, 0.18f, 0.18f), jacketColor, parent);
        CreateSphere("Interviewer Right Shoulder", new Vector3(0.31f, 1.09f, 0.01f), new Vector3(0.2f, 0.18f, 0.18f), jacketColor, parent);
        CreateCapsule("Interviewer Left Arm", new Vector3(-0.42f, 0.86f, 0.08f), new Vector3(0.1f, 0.3f, 0.1f), jacketColor, parent).transform.localRotation = Quaternion.Euler(0f, 0f, -14f);
        CreateCapsule("Interviewer Right Arm", new Vector3(0.42f, 0.86f, 0.08f), new Vector3(0.1f, 0.3f, 0.1f), jacketColor, parent).transform.localRotation = Quaternion.Euler(0f, 0f, 14f);
        CreateCube("Interviewer Neck", new Vector3(0f, 1.31f, 0.03f), new Vector3(0.12f, 0.14f, 0.11f), skinColor, parent);
        headRenderer = CreateSphere("Interviewer Head", new Vector3(0f, 1.49f, 0.04f), new Vector3(0.25f, 0.29f, 0.24f), skinColor, parent).GetComponent<Renderer>();
        CreateSphere("Interviewer Hair", new Vector3(0f, 1.63f, 0.02f), new Vector3(0.26f, 0.11f, 0.24f), hairColor, parent);
        Color faceShadow = Color.Lerp(skinColor, new Color32(42, 48, 56, 255), 0.18f);
        CreateCube("Interviewer Nose Shadow", new Vector3(0f, 1.49f, 0.27f), new Vector3(0.022f, 0.07f, 0.012f), faceShadow, parent);
        return torso;
    }

    private static Renderer CreateInterviewerChairBack(Transform parent)
    {
        CreateCube("Interview Chair Seat Hint", new Vector3(0f, 0.36f, -0.04f), new Vector3(0.78f, 0.1f, 0.56f), new Color32(24, 30, 38, 255), parent);
        return CreateCube("Interview Chair Back", new Vector3(0f, 0.78f, -0.24f), new Vector3(0.74f, 0.66f, 0.1f), new Color32(24, 30, 38, 255), parent).GetComponent<Renderer>();
    }

    private static Color GetNameplateColor(int index)
    {
        return index switch
        {
            0 => new Color32(32, 42, 48, 255),
            1 => new Color32(30, 38, 54, 255),
            _ => new Color32(44, 38, 48, 255)
        };
    }

    private void ApplyAssignedInterviewerCharacterTint(GameObject instance, Color roleTint)
    {
        if (instance == null || assignedInterviewerCharacterTintStrength <= 0f)
        {
            return;
        }

        float strength = Mathf.Clamp01(assignedInterviewerCharacterTintStrength);
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.materials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                if (material == null)
                {
                    continue;
                }

                Color baseColor = GetMaterialColor(material);
                Color tintedColor = Color.Lerp(baseColor, roleTint, strength);
                if (replaceAssignedInterviewerTexturesWithTint)
                {
                    ClearImportedMaterialTextures(material);
                    tintedColor = Color.Lerp(tintedColor, roleTint, strength);
                }
                SetMaterialColor(material, tintedColor);
                SuppressMannequinMaterialShine(material);
            }
        }
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material.HasProperty("_Color"))
        {
            return material.GetColor("_Color");
        }

        return material.color;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static void SuppressMannequinMaterialShine(Material material)
    {
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", Color.black);
        }
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.18f);
        }
        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.18f);
        }
    }

    private static void ClearImportedMaterialTextures(Material material)
    {
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", null);
        }
        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", null);
        }
        if (material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", null);
        }
        if (material.HasProperty("_MetallicGlossMap"))
        {
            material.SetTexture("_MetallicGlossMap", null);
        }
    }

    private static void CreateInterviewerDeskProp(int index, Transform parent)
    {
        switch (index)
        {
            case 0:
                CreateCube("Hiring Manager Notebook", new Vector3(-0.22f, 0.48f, 0.56f), new Vector3(0.34f, 0.03f, 0.2f), new Color32(96, 128, 120, 255), parent);
                CreateCube("Hiring Manager Pen", new Vector3(0.06f, 0.505f, 0.54f), new Vector3(0.24f, 0.018f, 0.018f), new Color32(190, 198, 190, 255), parent);
                break;
            case 1:
                CreateCube("Architect Tablet", new Vector3(0f, 0.48f, 0.56f), new Vector3(0.4f, 0.03f, 0.24f), new Color32(22, 32, 44, 255), parent);
                CreateCube("Architect Tablet Glow", new Vector3(0f, 0.502f, 0.56f), new Vector3(0.3f, 0.01f, 0.16f), new Color32(68, 132, 180, 255), parent);
                break;
            default:
                CreateCube("Sales Director Folder", new Vector3(0.2f, 0.48f, 0.56f), new Vector3(0.38f, 0.03f, 0.23f), new Color32(126, 98, 74, 255), parent);
                CreateCube("Sales Director Card", new Vector3(-0.12f, 0.502f, 0.54f), new Vector3(0.2f, 0.01f, 0.12f), new Color32(204, 190, 158, 255), parent);
                break;
        }
    }

    private GameObject InstantiateInterviewerCharacter(GameObject prefab, string name, Vector3 positionOffset, Vector3 rotationOffset, Vector3 scaleMultiplier, Transform parent)
    {
        if (prefab == null)
        {
            return null;
        }

        Vector3 safeScale = new Vector3(
            Mathf.Max(0.01f, scaleMultiplier.x),
            Mathf.Max(0.01f, scaleMultiplier.y),
            Mathf.Max(0.01f, scaleMultiplier.z));
        GameObject instance = InstantiatePrefab(prefab, name, Vector3.zero, Quaternion.Euler(rotationOffset), safeScale, parent);
        if (instance == null)
        {
            return null;
        }

        if (autoFitInterviewerCharacterPrefabs)
        {
            FitInterviewerCharacterInstance(instance, parent);
        }

        ApplyInterviewerCharacterRuntimeDefaults(instance);
        instance.transform.localPosition += positionOffset;
        return instance;
    }

    private void ApplyInterviewerCharacterRuntimeDefaults(GameObject instance)
    {
        if (instance == null || !disableAssignedInterviewerAnimators)
        {
            return;
        }

        Animator[] animators = instance.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].enabled = false;
            }
        }

        Animation[] legacyAnimations = instance.GetComponentsInChildren<Animation>();
        for (int i = 0; i < legacyAnimations.Length; i++)
        {
            if (legacyAnimations[i] != null)
            {
                legacyAnimations[i].enabled = false;
            }
        }
    }

    private void FitInterviewerCharacterInstance(GameObject instance, Transform parent)
    {
        if (instance == null || parent == null || !TryGetRendererBounds(instance, out Bounds bounds))
        {
            return;
        }

        float fitScale = CalculateInterviewerCharacterFitScale(bounds.size);
        if (fitScale > 0f && !Mathf.Approximately(fitScale, 1f))
        {
            instance.transform.localScale *= fitScale;
            TryGetRendererBounds(instance, out bounds);
        }

        Vector3 targetCenter = parent.TransformPoint(interviewerCharacterTargetCenter);
        float targetFloorY = parent.TransformPoint(new Vector3(0f, interviewerCharacterFloorY, 0f)).y;
        Vector3 delta = new Vector3(
            targetCenter.x - bounds.center.x,
            targetFloorY - bounds.min.y,
            targetCenter.z - bounds.center.z);
        instance.transform.position += delta;
    }

    private float CalculateInterviewerCharacterFitScale(Vector3 currentSize)
    {
        if (interviewerCharacterFitByHeight && currentSize.y > 0.0001f)
        {
            return Mathf.Clamp(interviewerCharacterTargetHeight / currentSize.y, 0.001f, 100f);
        }

        return CalculateFitScale(currentSize, interviewerCharacterTargetBounds) * PrefabBoundsPadding;
    }

    private bool EnsureSingleActiveController()
    {
        TheRoomPrototypeController[] controllers = FindObjectsByType<TheRoomPrototypeController>();
        for (int i = 0; i < controllers.Length; i++)
        {
            TheRoomPrototypeController controller = controllers[i];
            if (controller != null && controller != this && controller.isActiveAndEnabled)
            {
                Debug.LogWarning($"Final Round RC16: Duplicate TheRoomPrototypeController on '{name}' disabled. Active controller is '{controller.name}'.");
                enabled = false;
                return false;
            }
        }

        return true;
    }

    private void LogRc16PrefabSlotStatus()
    {
        Debug.Log("Final Round RC16 prefab slots: "
            + FormatPrefabSlot("Meeting Table", meetingTablePrefab) + "; "
            + FormatPrefabSlot("Candidate Chair", candidateChairPrefab) + "; "
            + FormatPrefabSlot("Interviewer Panel", interviewerPanelPrefab) + "; "
            + FormatPrefabSlot("Laptop", laptopPrefab) + "; "
            + FormatPrefabSlot("Notepad", notepadPrefab) + "; "
            + FormatPrefabSlot("Water Glass", waterGlassPrefab) + "; "
            + FormatPrefabSlot("Wall Screen", wallScreenPrefab) + "; "
            + FormatPrefabSlot("Whiteboard", whiteboardPrefab) + "; "
            + FormatPrefabSlot("Room Sign", roomSignPrefab) + "; "
            + FormatPrefabSlot("Door/Doorframe", doorDoorframePrefab) + "; "
            + FormatPrefabSlot("Corner Prop/Plant", plantOrCornerPropPrefab) + "; "
            + FormatPrefabSlot("Ceiling Light", ceilingLightPrefab) + "; "
            + FormatPrefabSlot("Hiring Manager Character", hiringManagerPrefab) + "; "
            + FormatPrefabSlot("Principal Security Architect Character", principalSecurityArchitectPrefab) + "; "
            + FormatPrefabSlot("Sales Director Character", salesDirectorPrefab));
    }

    private static string FormatPrefabSlot(string label, GameObject prefab)
    {
        return prefab == null ? $"{label}=fallback" : $"{label}=assigned '{prefab.name}'";
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

    private static string GetInterviewerNameplate(int index)
    {
        return index switch
        {
            0 => "Hiring Manager\nPeople signal",
            1 => "Principal Security Architect\nTechnical signal",
            _ => "Sales Director\nCommercial signal"
        };
    }

    private static void CreateInterviewerLabel(string label, Vector3 position, Transform parent)
    {
        CreateWorldLabel(label, position, Quaternion.identity, 0.074f, 1.8f, new Color32(232, 238, 244, 255), parent);
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

        float panelWidth = 236f;
        float panelHeight = 46f;
        Rect panelRect = new Rect(Screen.width * 0.5f - panelWidth * 0.5f, Screen.height - 104f, panelWidth, panelHeight);
        Rect keyRect = new Rect(panelRect.x + 14f, panelRect.y + 9f, 32f, 28f);
        Rect labelRect = new Rect(panelRect.x + 56f, panelRect.y + 9f, panelRect.width - 70f, 28f);

        GUIStyle panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal =
            {
                background = Texture2D.whiteTexture,
                textColor = Color.clear
            }
        };
        Color previousColor = GUI.color;
        GUI.color = new Color32(9, 13, 18, 218);
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUI.color = new Color32(98, 197, 176, 255);
        GUI.Box(keyRect, GUIContent.none, panelStyle);

        GUI.color = previousColor;
        GUIStyle keyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };
        keyStyle.normal.textColor = new Color32(8, 18, 20, 255);
        GUI.Label(keyRect, "E", keyStyle);

        GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };
        promptStyle.normal.textColor = new Color32(231, 238, 244, 255);
        GUI.Label(labelRect, "Sit for interview", promptStyle);
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

    private static GameObject InstantiatePrefab(GameObject prefab, string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject instance = Instantiate(prefab, parent);
        instance.name = name;
        instance.transform.localPosition = position;
        instance.transform.localRotation = rotation;
        instance.transform.localScale = scale;
        return instance;
    }

    private static GameObject InstantiateFittedPrefab(GameObject prefab, string name, Vector3 anchorPosition, Quaternion rotation, Vector3 maxBoundsSize, PrefabAnchor anchor, Transform parent)
    {
        GameObject instance = InstantiatePrefab(prefab, name, anchorPosition, rotation, Vector3.one, parent);
        if (instance == null)
        {
            return null;
        }

        if (!TryGetRendererBounds(instance, out Bounds bounds))
        {
            return instance;
        }

        float uniformScale = CalculateFitScale(bounds.size, maxBoundsSize) * PrefabBoundsPadding;
        if (uniformScale > 0f && !Mathf.Approximately(uniformScale, 1f))
        {
            instance.transform.localScale *= uniformScale;
            TryGetRendererBounds(instance, out bounds);
        }

        Vector3 target = anchorPosition;
        Vector3 current = bounds.center;
        if (anchor == PrefabAnchor.BottomCenter)
        {
            current.y = bounds.min.y;
        }
        else if (anchor == PrefabAnchor.BackWallCenter)
        {
            current.z = bounds.max.z;
        }
        else if (anchor == PrefabAnchor.FrontWallCenter)
        {
            current.z = bounds.min.z;
        }

        instance.transform.position += target - current;
        return instance;
    }

    private static float CalculateFitScale(Vector3 currentSize, Vector3 maxSize)
    {
        float scale = float.PositiveInfinity;
        if (currentSize.x > 0.0001f && maxSize.x > 0f)
        {
            scale = Mathf.Min(scale, maxSize.x / currentSize.x);
        }
        if (currentSize.y > 0.0001f && maxSize.y > 0f)
        {
            scale = Mathf.Min(scale, maxSize.y / currentSize.y);
        }
        if (currentSize.z > 0.0001f && maxSize.z > 0f)
        {
            scale = Mathf.Min(scale, maxSize.z / currentSize.z);
        }

        return float.IsInfinity(scale) ? 1f : Mathf.Clamp(scale, 0.01f, 12f);
    }

    private static bool TryGetRendererBounds(GameObject instance, out Bounds bounds)
    {
        bounds = default;
        if (instance == null)
        {
            return false;
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }

    private static Renderer GetFirstRenderer(GameObject instance)
    {
        if (instance == null)
        {
            return null;
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        return renderers.Length > 0 ? renderers[0] : null;
    }

    private static void SnapRendererBoundsBottomToY(GameObject instance, float targetY)
    {
        if (instance == null)
        {
            return;
        }

        if (!TryGetRendererBounds(instance, out Bounds bounds))
        {
            return;
        }

        float deltaY = targetY - bounds.min.y;
        instance.transform.position += new Vector3(0f, deltaY, 0f);
    }

    private static void ApplyMaterialOverride(GameObject instance, Color color)
    {
        if (instance == null)
        {
            return;
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material = CreateMaterial(color);
            }
        }
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

    private static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.material.color = color;
        if (renderer.material.HasProperty("_BaseColor"))
        {
            renderer.material.SetColor("_BaseColor", color);
        }
        if (renderer.material.HasProperty("_Color"))
        {
            renderer.material.SetColor("_Color", color);
        }
    }

    private void ConfigureRoomHum()
    {
        if (roomHumClip == null || roomHumSource != null)
        {
            return;
        }

        roomHumSource = gameObject.AddComponent<AudioSource>();
        roomHumSource.clip = roomHumClip;
        roomHumSource.loop = true;
        roomHumSource.playOnAwake = false;
        roomHumSource.volume = roomHumVolume;
        roomHumSource.spatialBlend = 0f;
        roomHumSource.Play();
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
