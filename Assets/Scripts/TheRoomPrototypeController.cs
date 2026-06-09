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
        return gameManager != null && gameManager.IsRoomUiFocusActive();
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

        if (gameManager != null)
        {
            gameManager.SetRoomObjectiveText("Interview in progress.");
        }

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
        playerController.SetMovementEnabled(true);
    }

    private void UpdateChairHighlight(bool active)
    {
        if (chairHighlightRenderer == null)
        {
            return;
        }

        Color color = active
            ? new Color32(96, 220, 190, 255)
            : new Color32(62, 130, 118, 150);
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

        standingStartPoint = CreateMarker("Standing Start Point", new Vector3(0f, 1.05f, -6.2f), Quaternion.identity, root);
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
        CreateCube("Hall Floor", new Vector3(0f, -0.05f, -4.4f), new Vector3(4.8f, 0.1f, 5.4f), new Color32(56, 58, 64, 255), root);
        CreateCube("Room Floor", new Vector3(0f, -0.05f, 0.7f), new Vector3(6.8f, 0.1f, 6.0f), new Color32(44, 47, 54, 255), root);
        CreateCube("Back Wall", new Vector3(0f, 1.5f, 3.65f), new Vector3(6.8f, 3f, 0.12f), new Color32(70, 74, 82, 255), root);
        CreateCube("Left Wall", new Vector3(-3.4f, 1.5f, 0.7f), new Vector3(0.12f, 3f, 6.0f), new Color32(61, 65, 73, 255), root);
        CreateCube("Right Wall", new Vector3(3.4f, 1.5f, 0.7f), new Vector3(0.12f, 3f, 6.0f), new Color32(61, 65, 73, 255), root);
        CreateCube("Front Left Wall", new Vector3(-2.25f, 1.5f, -2.25f), new Vector3(2.3f, 3f, 0.12f), new Color32(65, 68, 76, 255), root);
        CreateCube("Front Right Wall", new Vector3(2.25f, 1.5f, -2.25f), new Vector3(2.3f, 3f, 0.12f), new Color32(65, 68, 76, 255), root);
        CreateCube("Door Header", new Vector3(0f, 2.45f, -2.25f), new Vector3(2.2f, 1.1f, 0.12f), new Color32(65, 68, 76, 255), root);
        CreateCube("Door Frame Left", new Vector3(-1.12f, 1.15f, -2.36f), new Vector3(0.08f, 2.3f, 0.12f), new Color32(112, 126, 142, 255), root);
        CreateCube("Door Frame Right", new Vector3(1.12f, 1.15f, -2.36f), new Vector3(0.08f, 2.3f, 0.12f), new Color32(112, 126, 142, 255), root);
        CreateCube("Ceiling Soft Panel", new Vector3(0f, 3f, 0.6f), new Vector3(6.8f, 0.08f, 6.0f), new Color32(52, 55, 62, 255), root);

        CreatePointLight("Hall Guide Light", new Vector3(0f, 2.25f, -5.1f), 5.5f, 3.5f, new Color32(226, 238, 255, 255), root);
        CreatePointLight("Doorway Guide Light", new Vector3(0f, 2.35f, -2.1f), 5.2f, 3.2f, new Color32(255, 242, 220, 255), root);
        CreatePointLight("Room Key Light", new Vector3(0f, 2.55f, 0.1f), 8f, 4.4f, new Color32(255, 238, 210, 255), root);
        CreatePointLight("Panel Fill Light", new Vector3(0f, 2.1f, 2.45f), 5f, 2.6f, new Color32(176, 218, 255, 255), root);
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
        CreateCube("Interview Table", new Vector3(0f, 0.72f, 0.95f), new Vector3(4.4f, 0.18f, 1.65f), new Color32(86, 70, 54, 255), root);
        CreateCube("Table Front Modesty Panel", new Vector3(0f, 0.38f, 0.25f), new Vector3(4.2f, 0.62f, 0.12f), new Color32(66, 54, 44, 255), root);
        CreateCube("Candidate Chair", new Vector3(0f, 0.42f, -1.35f), new Vector3(0.72f, 0.18f, 0.72f), new Color32(54, 76, 92, 255), root);
        CreateCube("Candidate Chair Back", new Vector3(0f, 0.92f, -1.68f), new Vector3(0.78f, 0.9f, 0.12f), new Color32(54, 76, 92, 255), root);
        chairHighlightRenderer = CreateCube("Interview Chair Floor Marker", new Vector3(0f, 0.012f, -1.35f), new Vector3(1.18f, 0.025f, 1.0f), new Color32(62, 130, 118, 150), root).GetComponent<Renderer>();

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
            new Vector3(-1.45f, 1.22f, 2.03f),
            new Vector3(0f, 1.24f, 2.13f),
            new Vector3(1.45f, 1.22f, 2.03f)
        };

        for (int i = 0; i < placeholders.Length; i++)
        {
            GameObject placeholder = new GameObject($"Interviewer Placeholder {i + 1}");
            placeholder.transform.SetParent(root, false);
            placeholder.transform.localPosition = positions[i];
            placeholder.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Renderer panel = CreateCube("Panel", Vector3.zero, new Vector3(0.86f, 1.22f, 0.08f), new Color32(44, 50, 62, 255), placeholder.transform).GetComponent<Renderer>();
            Renderer head = CreateSphere("Avatar Head", new Vector3(0f, 0.16f, -0.18f), new Vector3(0.28f, 0.32f, 0.28f), new Color32(168, 174, 184, 255), placeholder.transform).GetComponent<Renderer>();
            CreateCapsule("Avatar Body", new Vector3(0f, -0.28f, -0.18f), new Vector3(0.34f, 0.34f, 0.18f), new Color32(94, 103, 118, 255), placeholder.transform);
            Renderer reaction = CreateCube("Reaction Hook Strip", new Vector3(0f, -0.55f, -0.22f), new Vector3(0.68f, 0.06f, 0.04f), new Color32(72, 82, 96, 255), placeholder.transform).GetComponent<Renderer>();
            CreateInterviewerLabel(GetInterviewerLabel(i), new Vector3(0f, 0.78f, -0.24f), placeholder.transform);

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
        GameObject labelObject = new GameObject(label + " Label", typeof(TextMeshPro));
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = position;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one * 0.09f;

        TextMeshPro text = labelObject.GetComponent<TextMeshPro>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 2.4f;
        text.color = new Color32(222, 230, 238, 255);
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
