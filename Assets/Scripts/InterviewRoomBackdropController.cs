using TMPro;
using UnityEngine;

public class InterviewRoomBackdropController : MonoBehaviour
{
    public static readonly bool debugBackdropVisibility = false;
    public static readonly bool enable3DViewportFallback = false;

    private const string ControllerName = "Final Round Interview Room Backdrop Controller";
    private const string RootName = "Final Round 3D Backdrop";

    private static InterviewRoomBackdropController activeController;
    private RenderTexture viewportTexture;

    private readonly Color wallColor = debugBackdropVisibility ? new Color32(54, 72, 96, 255) : new Color32(43, 50, 61, 255);
    private readonly Color floorColor = debugBackdropVisibility ? new Color32(46, 55, 68, 255) : new Color32(30, 34, 42, 255);
    private readonly Color deskColor = debugBackdropVisibility ? new Color32(116, 76, 42, 255) : new Color32(86, 58, 39, 255);
    private readonly Color chairColor = debugBackdropVisibility ? new Color32(55, 86, 122, 255) : new Color32(39, 47, 62, 255);
    private readonly Color silhouetteColor = debugBackdropVisibility ? new Color32(24, 28, 38, 255) : new Color32(24, 28, 36, 255);
    private readonly Color laptopBodyColor = debugBackdropVisibility ? new Color32(16, 24, 34, 255) : new Color32(15, 18, 24, 255);

    private GameObject roomRoot;
    private Camera mainDisplayCamera;
    private Light mainLight;
    private Light accentLight;
    private Light laptopLight;
    private Camera roomCamera;
    private Renderer floorRenderer;
    private Renderer backWallRenderer;
    private Renderer lowerWallPanelRenderer;
    private Renderer leftWallRenderer;
    private Renderer rightWallRenderer;
    private Renderer deskTopRenderer;
    private Renderer deskFrontRenderer;
    private Renderer deskEdgeRenderer;
    private Renderer[] chairRenderers;
    private Renderer[] silhouetteRenderers;
    private Renderer wallPanelRenderer;
    private Renderer laptopScreenRenderer;
    private TMP_Text wallStageText;
    private Light rimLight;
    private GameObject videoCallRoot;
    private Renderer callScreenRenderer;
    private Renderer callHeaderRenderer;
    private Renderer callGlowRenderer;
    private Renderer[] callBackgroundAccentRenderers;
    private Renderer statusCardRenderer;
    private TMP_Text callStageText;
    private TMP_Text callStatusText;
    private TMP_Text callStatusBodyText;
    private ParticipantTile[] participantTiles;
    private Color activeCallAccentColor = new Color32(92, 188, 164, 255);
    private Color inactiveTileColor = new Color32(28, 34, 46, 255);
    private Color activeTileColor = new Color32(43, 72, 82, 255);
    private string activeStageName = "Main Menu";
    private bool interviewerSpeaking = true;
    private GameObject startupMessProps;
    private GameObject securityOpsProps;
    private GameObject aiHypeProps;
    private GameObject legacyEnterpriseProps;
    private GameObject saasVendorProps;
    private string activeCompanyProfileName = string.Empty;
    private int activeInterviewPressure;

    public RenderTexture ViewportTexture => viewportTexture;

    private void Update()
    {
        AnimateVideoCall(Time.time);
    }

    private void Awake()
    {
        gameObject.name = ControllerName;

        if (!enable3DViewportFallback)
        {
            enabled = false;
            return;
        }

        if (activeController != null && activeController != this)
        {
            Destroy(gameObject);
            return;
        }

        activeController = this;
        CleanupLegacySceneObjects();
        EnsureMainDisplayCamera();
        BuildRoom();
        SetStageAtmosphere("Main Menu");

        if (debugBackdropVisibility)
        {
            LogBackdropState("created");
        }
    }

    private void OnDestroy()
    {
        if (activeController == this)
        {
            activeController = null;
        }

        if (viewportTexture != null)
        {
            viewportTexture.Release();
            Destroy(viewportTexture);
            viewportTexture = null;
        }
    }

    public void SetProcessAtmosphere(string companyProfileName, string stageName, int interviewPressure)
    {
        activeCompanyProfileName = companyProfileName ?? string.Empty;
        activeInterviewPressure = Mathf.Clamp(interviewPressure, 0, 100);
        interviewerSpeaking = IsInterviewStage(stageName);
        SetStageAtmosphere(stageName);
    }

    public void SetInterviewerSpeaking(bool isSpeaking)
    {
        interviewerSpeaking = isSpeaking;

        if (callStatusText != null && IsInterviewStage(activeStageName))
        {
            callStatusText.text = interviewerSpeaking ? "LIVE PANEL" : "PANEL MUTED";
        }
    }

    public void SetStageAtmosphere(string stageName)
    {
        if (mainLight == null || accentLight == null || rimLight == null)
        {
            return;
        }

        activeStageName = string.IsNullOrEmpty(stageName) ? "Main Menu" : stageName;
        Color mainColor = new Color32(255, 242, 220, 255);
        Color accentColor = new Color32(74, 143, 166, 255);
        float mainIntensity = 1.55f;
        float accentIntensity = 1.15f;
        float rimIntensity = 1.2f;
        float laptopIntensity = 0.8f;
        string displayText = string.IsNullOrEmpty(stageName) ? "FINAL ROUND" : stageName.ToUpperInvariant();

        switch (stageName)
        {
            case "Recruiter Screen":
                mainColor = new Color32(255, 232, 205, 255);
                accentColor = new Color32(96, 180, 168, 255);
                mainIntensity = 1.65f;
                accentIntensity = 0.85f;
                rimIntensity = 0.9f;
                laptopIntensity = 1.25f;
                break;
            case "Hiring Manager":
                mainColor = new Color32(255, 244, 224, 255);
                accentColor = new Color32(95, 135, 190, 255);
                mainIntensity = 1.55f;
                accentIntensity = 1.0f;
                rimIntensity = 1.1f;
                laptopIntensity = 0.75f;
                break;
            case "Technical Panel":
                mainColor = new Color32(226, 238, 255, 255);
                accentColor = new Color32(89, 162, 255, 255);
                mainIntensity = 1.35f;
                accentIntensity = 1.45f;
                rimIntensity = 1.35f;
                laptopIntensity = 0.9f;
                break;
            case "VP Round":
                mainColor = new Color32(255, 222, 188, 255);
                accentColor = new Color32(184, 103, 255, 255);
                mainIntensity = 1.25f;
                accentIntensity = 1.65f;
                rimIntensity = 1.6f;
                laptopIntensity = 0.55f;
                break;
            case "Final Outcome":
                mainColor = new Color32(244, 240, 230, 255);
                accentColor = new Color32(218, 218, 206, 255);
                mainIntensity = 1.65f;
                accentIntensity = 0.8f;
                rimIntensity = 1.35f;
                laptopIntensity = 0.6f;
                displayText = "FINAL DECISION";
                break;
            case "Between Rounds":
                mainColor = new Color32(230, 236, 255, 255);
                accentColor = new Color32(123, 154, 220, 255);
                mainIntensity = 1.35f;
                accentIntensity = 1.15f;
                rimIntensity = 1.0f;
                laptopIntensity = 0.8f;
                break;
            default:
                displayText = "FINAL ROUND";
                break;
        }

        ApplyCompanyAtmosphere(
            activeCompanyProfileName,
            stageName,
            ref mainColor,
            ref accentColor,
            ref mainIntensity,
            ref accentIntensity,
            ref rimIntensity,
            ref laptopIntensity,
            ref displayText);
        ApplyPressureAtmosphere(
            activeInterviewPressure,
            ref mainColor,
            ref accentColor,
            ref mainIntensity,
            ref accentIntensity,
            ref rimIntensity,
            ref laptopIntensity);

        mainLight.color = mainColor;
        mainLight.intensity = debugBackdropVisibility ? mainIntensity + 0.75f : mainIntensity;
        accentLight.color = accentColor;
        accentLight.intensity = debugBackdropVisibility ? accentIntensity + 0.7f : accentIntensity;
        rimLight.color = Color.Lerp(accentColor, Color.white, 0.28f);
        rimLight.intensity = debugBackdropVisibility ? rimIntensity + 0.65f : rimIntensity;
        laptopLight.intensity = debugBackdropVisibility ? laptopIntensity + 0.65f : laptopIntensity;
        if (wallStageText != null)
        {
            wallStageText.text = displayText;
            wallStageText.fontSize = debugBackdropVisibility ? 9f : 7f;
        }
        ConfigureParticipantTiles(stageName, displayText, accentColor);

        if (wallPanelRenderer != null)
        {
            wallPanelRenderer.material.color = new Color(accentColor.r * 0.55f, accentColor.g * 0.55f, accentColor.b * 0.55f, 1f);
        }

        if (laptopScreenRenderer != null)
        {
            laptopScreenRenderer.material.color = new Color(accentColor.r * 0.9f, accentColor.g * 0.9f, accentColor.b * 0.9f, 1f);
        }

        ApplyCallTheme(stageName, accentColor);

        if (debugBackdropVisibility)
        {
            Debug.Log(
                "Final Round backdrop atmosphere applied\n" +
                $"Stage: {displayText}\n" +
                $"Debug visibility: {debugBackdropVisibility}");
        }
    }

    private void ApplyCompanyAtmosphere(
        string companyProfileName,
        string stageName,
        ref Color mainColor,
        ref Color accentColor,
        ref float mainIntensity,
        ref float accentIntensity,
        ref float rimIntensity,
        ref float laptopIntensity,
        ref string displayText)
    {
        SetCompanyPropsActive(companyProfileName);

        Color wall = wallColor;
        Color floor = floorColor;
        Color lowerPanel = new Color32(34, 40, 50, 255);
        Color desk = deskColor;
        Color deskFront = new Color32(58, 40, 30, 255);
        Color chair = chairColor;
        Color silhouette = silhouetteColor;
        Color cameraBackground = debugBackdropVisibility ? new Color32(18, 21, 28, 255) : new Color32(12, 14, 19, 255);

        switch (companyProfileName)
        {
            case "Big SaaS Vendor":
                wall = new Color32(48, 56, 68, 255);
                floor = new Color32(38, 43, 51, 255);
                lowerPanel = new Color32(42, 50, 62, 255);
                desk = new Color32(72, 78, 86, 255);
                deskFront = new Color32(54, 60, 70, 255);
                chair = new Color32(38, 47, 62, 255);
                silhouette = new Color32(22, 27, 36, 255);
                mainColor = Color.Lerp(mainColor, new Color32(218, 235, 255, 255), 0.55f);
                accentColor = new Color32(88, 150, 210, 255);
                mainIntensity += 0.1f;
                accentIntensity -= 0.1f;
                laptopIntensity -= 0.1f;
                displayText = "QBR / PANEL";
                break;
            case "Startup Rocketship":
                wall = new Color32(62, 48, 43, 255);
                floor = new Color32(41, 35, 35, 255);
                lowerPanel = new Color32(66, 48, 40, 255);
                desk = new Color32(104, 66, 42, 255);
                deskFront = new Color32(75, 44, 32, 255);
                chair = new Color32(72, 54, 60, 255);
                silhouette = new Color32(27, 24, 28, 255);
                mainColor = Color.Lerp(mainColor, new Color32(255, 204, 164, 255), 0.62f);
                accentColor = new Color32(255, 110, 72, 255);
                mainIntensity += 0.2f;
                accentIntensity += 0.45f;
                laptopIntensity += 0.55f;
                displayText = "RUNWAY / URGENCY";
                break;
            case "Security Vendor":
                wall = new Color32(28, 39, 46, 255);
                floor = new Color32(20, 27, 34, 255);
                lowerPanel = new Color32(22, 48, 52, 255);
                desk = new Color32(38, 48, 54, 255);
                deskFront = new Color32(24, 32, 38, 255);
                chair = new Color32(28, 39, 48, 255);
                silhouette = new Color32(14, 18, 24, 255);
                mainColor = Color.Lerp(mainColor, new Color32(170, 220, 255, 255), 0.55f);
                accentColor = new Color32(64, 220, 166, 255);
                mainIntensity -= 0.25f;
                accentIntensity += 0.5f;
                rimIntensity += 0.45f;
                displayText = "RISK REVIEW";
                break;
            case "AI Hype Company":
                wall = new Color32(36, 30, 52, 255);
                floor = new Color32(25, 24, 38, 255);
                lowerPanel = new Color32(46, 32, 70, 255);
                desk = new Color32(52, 44, 72, 255);
                deskFront = new Color32(34, 28, 48, 255);
                chair = new Color32(42, 39, 66, 255);
                silhouette = new Color32(18, 18, 30, 255);
                mainColor = Color.Lerp(mainColor, new Color32(204, 186, 255, 255), 0.58f);
                accentColor = new Color32(116, 236, 255, 255);
                accentIntensity += 0.7f;
                rimIntensity += 0.5f;
                laptopIntensity += 0.8f;
                displayText = "AI STRATEGY";
                break;
            case "Legacy Enterprise":
                wall = new Color32(80, 78, 72, 255);
                floor = new Color32(58, 57, 54, 255);
                lowerPanel = new Color32(70, 68, 62, 255);
                desk = new Color32(82, 76, 66, 255);
                deskFront = new Color32(62, 57, 50, 255);
                chair = new Color32(62, 62, 60, 255);
                silhouette = new Color32(30, 30, 30, 255);
                cameraBackground = new Color32(32, 32, 31, 255);
                mainColor = Color.Lerp(mainColor, new Color32(230, 224, 208, 255), 0.68f);
                accentColor = new Color32(158, 154, 142, 255);
                mainIntensity -= 0.05f;
                accentIntensity -= 0.35f;
                rimIntensity -= 0.2f;
                laptopIntensity -= 0.25f;
                displayText = "STEERING COMMITTEE";
                break;
        }

        if (stageName == "Final Outcome")
        {
            displayText = "FINAL DECISION";
        }

        ApplyRendererColor(floorRenderer, floor);
        ApplyRendererColor(backWallRenderer, wall);
        ApplyRendererColor(lowerWallPanelRenderer, lowerPanel);
        ApplyRendererColor(leftWallRenderer, lowerPanel);
        ApplyRendererColor(rightWallRenderer, lowerPanel);
        ApplyRendererColor(deskTopRenderer, desk);
        ApplyRendererColor(deskFrontRenderer, deskFront);
        ApplyRendererColor(deskEdgeRenderer, Color.Lerp(desk, Color.white, 0.18f));
        ApplyRendererColors(chairRenderers, chair);
        ApplyRendererColors(silhouetteRenderers, silhouette);
        ApplyRendererColor(callScreenRenderer, Color.Lerp(wall, Color.black, 0.36f));

        if (roomCamera != null)
        {
            roomCamera.backgroundColor = cameraBackground;
        }
    }

    private void ApplyPressureAtmosphere(
        int interviewPressure,
        ref Color mainColor,
        ref Color accentColor,
        ref float mainIntensity,
        ref float accentIntensity,
        ref float rimIntensity,
        ref float laptopIntensity)
    {
        if (interviewPressure < 50)
        {
            return;
        }

        if (interviewPressure < 75)
        {
            mainColor = Color.Lerp(mainColor, new Color32(194, 214, 238, 255), 0.18f);
            mainIntensity -= 0.08f;
            accentIntensity += 0.12f;
            return;
        }

        if (interviewPressure < 90)
        {
            mainColor = Color.Lerp(mainColor, new Color32(178, 198, 230, 255), 0.26f);
            accentColor = Color.Lerp(accentColor, new Color32(176, 82, 170, 255), 0.25f);
            mainIntensity -= 0.14f;
            accentIntensity += 0.34f;
            rimIntensity += 0.22f;
            laptopIntensity += 0.12f;
            return;
        }

        mainColor = Color.Lerp(mainColor, new Color32(154, 158, 210, 255), 0.34f);
        accentColor = Color.Lerp(accentColor, new Color32(224, 66, 120, 255), 0.36f);
        mainIntensity -= 0.2f;
        accentIntensity += 0.55f;
        rimIntensity += 0.4f;
        laptopIntensity += 0.18f;
    }

    private void ApplyRendererColor(Renderer renderer, Color color)
    {
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private void ApplyRendererColors(Renderer[] renderers, Color color)
    {
        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            ApplyRendererColor(renderers[i], color);
        }
    }

    private void SetCompanyPropsActive(string companyProfileName)
    {
        SetActiveIfExists(saasVendorProps, companyProfileName == "Big SaaS Vendor");
        SetActiveIfExists(startupMessProps, companyProfileName == "Startup Rocketship");
        SetActiveIfExists(securityOpsProps, companyProfileName == "Security Vendor");
        SetActiveIfExists(aiHypeProps, companyProfileName == "AI Hype Company");
        SetActiveIfExists(legacyEnterpriseProps, companyProfileName == "Legacy Enterprise");
    }

    private void SetActiveIfExists(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private void SetCallDecorationsVisible(bool visible)
    {
        if (callBackgroundAccentRenderers == null)
        {
            return;
        }

        for (int i = 0; i < callBackgroundAccentRenderers.Length; i++)
        {
            if (callBackgroundAccentRenderers[i] != null)
            {
                callBackgroundAccentRenderers[i].gameObject.SetActive(visible);
            }
        }
    }

    private void BuildRoom()
    {
        RemoveExistingBackdropRoots();

        roomRoot = new GameObject(RootName);
        roomRoot.transform.SetParent(transform, false);
        roomRoot.transform.position = Vector3.zero;
        roomRoot.transform.rotation = Quaternion.identity;
        roomRoot.transform.localScale = Vector3.one;

        CreateCamera();
        CreateLighting();
        CreateRoomShell();
        CreateVideoCallScene();
    }

    private void CleanupLegacySceneObjects()
    {
        string[] legacyObjectNames =
        {
            "Geometry",
            "BackgroundMesh",
            "Platform",
            "UnityMaterialBall_Gold",
            "SamplesSpotlight",
            "SamplesSpotlightModel",
            "SamplesFloorSpotlight",
            "SamplesFixture",
            "StaticLightingSky",
            "Adaptive Probe Volume",
            "ProbeVolumePerSceneData",
            "Reflection Probe",
            "Lighting"
        };

        for (int i = 0; i < legacyObjectNames.Length; i++)
        {
            DestroyObjectsNamed(legacyObjectNames[i]);
        }
    }

    private void RemoveExistingBackdropRoots()
    {
        string[] generatedRootNames =
        {
            RootName,
            "Interview Room Backdrop",
            "Final Round Generated Interview Room",
            "Final Round Backdrop Camera",
            "Backdrop Camera"
        };

        for (int i = 0; i < generatedRootNames.Length; i++)
        {
            DestroyObjectsNamed(generatedRootNames[i]);
        }
    }

    private void DestroyObjectsNamed(string objectName)
    {
        GameObject[] sceneObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);

        for (int i = 0; i < sceneObjects.Length; i++)
        {
            GameObject target = sceneObjects[i];

            if (target.name != objectName)
            {
                continue;
            }

            if (target != gameObject)
            {
                Destroy(target);
            }
        }
    }

    private void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Backdrop Camera", typeof(Camera));
        cameraObject.transform.SetParent(roomRoot.transform, false);
        cameraObject.tag = "Untagged";
        cameraObject.transform.position = new Vector3(0f, 1.55f, -1.28f);
        cameraObject.transform.LookAt(new Vector3(0f, 1.55f, 1.78f));

        roomCamera = cameraObject.GetComponent<Camera>();
        roomCamera.enabled = true;
        roomCamera.clearFlags = CameraClearFlags.SolidColor;
        roomCamera.backgroundColor = debugBackdropVisibility ? new Color32(18, 21, 28, 255) : new Color32(12, 14, 19, 255);
        roomCamera.orthographic = true;
        roomCamera.orthographicSize = debugBackdropVisibility ? 1.68f : 1.52f;
        roomCamera.nearClipPlane = 0.05f;
        roomCamera.farClipPlane = 50f;
        roomCamera.depth = 0f;
        roomCamera.cullingMask = ~0;
        roomCamera.targetTexture = CreateViewportTexture();
    }

    private void EnsureMainDisplayCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include);

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].targetTexture == null && cameras[i].gameObject.name == "Main Camera")
            {
                mainDisplayCamera = cameras[i];
                break;
            }
        }

        if (mainDisplayCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
            mainDisplayCamera = cameraObject.GetComponent<Camera>();
        }

        mainDisplayCamera.gameObject.name = "Main Camera";
        mainDisplayCamera.gameObject.tag = "MainCamera";
        mainDisplayCamera.enabled = true;
        mainDisplayCamera.targetTexture = null;
        mainDisplayCamera.targetDisplay = 0;
        mainDisplayCamera.clearFlags = CameraClearFlags.SolidColor;
        mainDisplayCamera.backgroundColor = new Color32(8, 10, 15, 255);
        mainDisplayCamera.fieldOfView = 60f;
        mainDisplayCamera.nearClipPlane = 0.1f;
        mainDisplayCamera.farClipPlane = 100f;
        mainDisplayCamera.depth = -10f;
        mainDisplayCamera.cullingMask = 0;
        mainDisplayCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainDisplayCamera.transform.rotation = Quaternion.identity;

        DisableExtraDisplayCameras(mainDisplayCamera);
    }

    private void DisableExtraDisplayCameras(Camera cameraToKeep)
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include);

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];

            if (candidate == cameraToKeep || candidate.targetTexture != null)
            {
                continue;
            }

            candidate.enabled = false;
            candidate.gameObject.tag = "Untagged";
        }
    }

    private RenderTexture CreateViewportTexture()
    {
        if (viewportTexture != null)
        {
            viewportTexture.Release();
            Destroy(viewportTexture);
        }

        viewportTexture = new RenderTexture(960, 540, 16, RenderTextureFormat.ARGB32)
        {
            name = "Final Round Backdrop Viewport Texture",
            antiAliasing = 2,
            useMipMap = false,
            autoGenerateMips = false
        };
        viewportTexture.Create();
        return viewportTexture;
    }

    private void CreateLighting()
    {
        GameObject mainLightObject = new GameObject("Final Round Soft Key Light", typeof(Light));
        mainLightObject.transform.SetParent(roomRoot.transform, false);
        mainLightObject.transform.position = new Vector3(-1.7f, 3.25f, -1.8f);
        mainLightObject.transform.rotation = Quaternion.Euler(55f, 25f, 0f);

        mainLight = mainLightObject.GetComponent<Light>();
        mainLight.type = LightType.Spot;
        mainLight.range = 10f;
        mainLight.spotAngle = debugBackdropVisibility ? 82f : 72f;
        mainLight.shadows = LightShadows.Soft;

        GameObject accentLightObject = new GameObject("Final Round Accent Light", typeof(Light));
        accentLightObject.transform.SetParent(roomRoot.transform, false);
        accentLightObject.transform.position = new Vector3(2.65f, 2.05f, 1.25f);

        accentLight = accentLightObject.GetComponent<Light>();
        accentLight.type = LightType.Point;
        accentLight.range = 7f;

        GameObject rimLightObject = new GameObject("Final Round Interviewer Rim Light", typeof(Light));
        rimLightObject.transform.SetParent(roomRoot.transform, false);
        rimLightObject.transform.position = new Vector3(0f, 1.9f, 2.25f);

        rimLight = rimLightObject.GetComponent<Light>();
        rimLight.type = LightType.Point;
        rimLight.range = 3.2f;

        GameObject laptopLightObject = new GameObject("Final Round Laptop Glow", typeof(Light));
        laptopLightObject.transform.SetParent(roomRoot.transform, false);
        laptopLightObject.transform.position = new Vector3(0f, 1.12f, -0.2f);

        laptopLight = laptopLightObject.GetComponent<Light>();
        laptopLight.type = LightType.Point;
        laptopLight.range = 2.1f;
        laptopLight.intensity = debugBackdropVisibility ? 1.35f : 0.65f;
        laptopLight.color = new Color32(140, 210, 255, 255);
    }

    private void CreateRoomShell()
    {
        floorRenderer = CreateCube("Floor", new Vector3(0f, -0.04f, 0.2f), new Vector3(7f, 0.08f, 6.4f), floorColor).GetComponent<Renderer>();
        backWallRenderer = CreateCube("Back Wall", new Vector3(0f, 1.65f, 2.55f), new Vector3(7.5f, 3.4f, 0.12f), wallColor).GetComponent<Renderer>();
        lowerWallPanelRenderer = CreateCube("Back Wall Lower Panel", new Vector3(0f, 0.68f, 2.48f), new Vector3(7.1f, 0.7f, 0.05f), new Color32(34, 40, 50, 255)).GetComponent<Renderer>();
        leftWallRenderer = CreateCube("Left Wall", new Vector3(-3.45f, 1.65f, 0.2f), new Vector3(0.12f, 3.4f, 5.0f), new Color32(34, 39, 49, 255)).GetComponent<Renderer>();
        rightWallRenderer = CreateCube("Right Wall", new Vector3(3.45f, 1.65f, 0.2f), new Vector3(0.12f, 3.4f, 5.0f), new Color32(34, 39, 49, 255)).GetComponent<Renderer>();
    }

    private void CreateFurniture()
    {
        deskTopRenderer = CreateCube("Interview Desk Top", new Vector3(0f, 0.82f, 0.6f), new Vector3(3.05f, 0.16f, 1.0f), deskColor).GetComponent<Renderer>();
        deskFrontRenderer = CreateCube("Interview Desk Front", new Vector3(0f, 0.46f, 1.02f), new Vector3(3.05f, 0.72f, 0.1f), new Color32(58, 40, 30, 255)).GetComponent<Renderer>();
        CreateCube("Desk Left Leg", new Vector3(-1.28f, 0.42f, 0.25f), new Vector3(0.16f, 0.84f, 0.16f), deskColor);
        CreateCube("Desk Right Leg", new Vector3(1.28f, 0.42f, 0.25f), new Vector3(0.16f, 0.84f, 0.16f), deskColor);
        deskEdgeRenderer = CreateCube("Desk Front Edge", new Vector3(0f, 0.93f, 0.08f), new Vector3(3.12f, 0.08f, 0.16f), new Color32(117, 77, 48, 255)).GetComponent<Renderer>();

        Renderer[] candidateChairRenderers = CreateChair("Candidate Chair", new Vector3(-1.4f, 0f, -1.25f), 155f);
        Renderer[] interviewerChairRenderers = CreateChair("Interviewer Chair", new Vector3(0.15f, 0f, 1.55f), 0f);
        chairRenderers = new Renderer[candidateChairRenderers.Length + interviewerChairRenderers.Length];
        candidateChairRenderers.CopyTo(chairRenderers, 0);
        interviewerChairRenderers.CopyTo(chairRenderers, candidateChairRenderers.Length);
    }

    private Renderer[] CreateChair(string name, Vector3 basePosition, float yaw)
    {
        Transform chairRoot = new GameObject(name).transform;
        chairRoot.SetParent(roomRoot.transform, false);
        chairRoot.position = basePosition;
        chairRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

        return new Renderer[]
        {
            CreateCube(name + " Seat", new Vector3(0f, 0.46f, 0f), new Vector3(0.85f, 0.15f, 0.72f), chairColor, chairRoot).GetComponent<Renderer>(),
            CreateCube(name + " Back", new Vector3(0f, 0.98f, 0.32f), new Vector3(0.85f, 0.95f, 0.14f), chairColor, chairRoot).GetComponent<Renderer>(),
            CreateCube(name + " Left Leg", new Vector3(-0.32f, 0.22f, -0.22f), new Vector3(0.1f, 0.46f, 0.1f), chairColor, chairRoot).GetComponent<Renderer>(),
            CreateCube(name + " Right Leg", new Vector3(0.32f, 0.22f, -0.22f), new Vector3(0.1f, 0.46f, 0.1f), chairColor, chairRoot).GetComponent<Renderer>()
        };
    }

    private void CreateLaptop()
    {
        CreateCube("Laptop Base", new Vector3(-0.52f, 0.94f, 0.38f), new Vector3(0.9f, 0.06f, 0.5f), laptopBodyColor);

        GameObject screen = CreateCube("Laptop Screen", new Vector3(-0.52f, 1.16f, 0.62f), new Vector3(0.9f, 0.48f, 0.05f), new Color32(58, 116, 148, 255));
        screen.transform.rotation = Quaternion.Euler(-12f, 0f, 0f);
        laptopScreenRenderer = screen.GetComponent<Renderer>();
    }

    private void CreateInterviewerSilhouette()
    {
        GameObject torso = CreateCapsule("Interviewer Torso", new Vector3(0.18f, 1.25f, 1.72f), new Vector3(0.58f, 0.78f, 0.42f), silhouetteColor);
        torso.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        silhouetteRenderers = new Renderer[]
        {
            torso.GetComponent<Renderer>(),
            CreateSphere("Interviewer Head", new Vector3(0.18f, 1.84f, 1.58f), new Vector3(0.42f, 0.42f, 0.42f), silhouetteColor).GetComponent<Renderer>(),
            CreateCube("Interviewer Left Arm", new Vector3(-0.34f, 1.15f, 1.2f), new Vector3(0.16f, 0.58f, 0.18f), silhouetteColor).GetComponent<Renderer>(),
            CreateCube("Interviewer Right Arm", new Vector3(0.7f, 1.15f, 1.2f), new Vector3(0.16f, 0.58f, 0.18f), silhouetteColor).GetComponent<Renderer>()
        };

        for (int i = 0; i < silhouetteRenderers.Length; i++)
        {
            silhouetteRenderers[i].gameObject.SetActive(false);
        }
    }

    private void CreateWallDisplay()
    {
        GameObject panel = CreateCube("Stage Wall Display", new Vector3(0f, 1.9f, 2.47f), new Vector3(3.2f, 0.82f, 0.04f), new Color32(38, 86, 98, 255));
        wallPanelRenderer = panel.GetComponent<Renderer>();

        GameObject textObject = new GameObject("Stage Wall Display Text", typeof(TextMeshPro));
        textObject.transform.SetParent(roomRoot.transform, false);
        textObject.transform.position = new Vector3(0f, 1.91f, 2.42f);
        textObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        textObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        wallStageText = textObject.GetComponent<TMP_Text>();
        wallStageText.text = "FINAL ROUND";
        wallStageText.fontSize = debugBackdropVisibility ? 9f : 7f;
        wallStageText.fontStyle = FontStyles.Bold;
        wallStageText.alignment = TextAlignmentOptions.Center;
        wallStageText.color = new Color32(232, 244, 250, 255);
        wallStageText.rectTransform.sizeDelta = new Vector2(62f, 14f);
    }

    private void CreateVideoCallScene()
    {
        videoCallRoot = new GameObject("Animated Video Call Viewport");
        videoCallRoot.transform.SetParent(roomRoot.transform, false);

        callGlowRenderer = CreateCallCube("Video Call Outer Frame", new Vector3(0f, 1.55f, 1.95f), new Vector3(5.04f, 2.68f, 0.035f), new Color32(45, 70, 82, 255), videoCallRoot.transform).GetComponent<Renderer>();
        callScreenRenderer = CreateCallCube("Video Call Screen", new Vector3(0f, 1.55f, 1.9f), new Vector3(4.88f, 2.52f, 0.05f), new Color32(22, 38, 58, 255), videoCallRoot.transform).GetComponent<Renderer>();
        callHeaderRenderer = CreateCallCube("Video Call Top Bar", new Vector3(0f, 2.64f, 1.34f), new Vector3(4.42f, 0.22f, 0.04f), new Color32(36, 48, 64, 255), videoCallRoot.transform).GetComponent<Renderer>();

        callStageText = CreateWorldText(
            "Video Call Stage Label",
            new Vector3(-2.02f, 2.64f, 1.12f),
            new Vector2(30f, 4f),
            "FINAL ROUND",
            3.6f,
            TextAlignmentOptions.Left,
            videoCallRoot.transform);

        callStatusText = CreateWorldText(
            "Video Call Status Label",
            new Vector3(2.02f, 2.64f, 1.12f),
            new Vector2(22f, 4f),
            "LIVE CALL",
            2.9f,
            TextAlignmentOptions.Right,
            videoCallRoot.transform);
        callStatusText.color = new Color32(172, 224, 214, 255);

        callBackgroundAccentRenderers = new Renderer[]
        {
            CreateCallCube("Call Background Accent Left", new Vector3(-2.34f, 1.47f, 1.62f), new Vector3(0.07f, 1.86f, 0.04f), new Color32(92, 188, 164, 255), videoCallRoot.transform).GetComponent<Renderer>(),
            CreateCallCube("Call Background Accent Right", new Vector3(2.34f, 1.47f, 1.62f), new Vector3(0.07f, 1.86f, 0.04f), new Color32(92, 188, 164, 255), videoCallRoot.transform).GetComponent<Renderer>(),
            CreateCallCube("Call Background Signal Bar", new Vector3(0f, 0.32f, 1.62f), new Vector3(4.24f, 0.06f, 0.04f), new Color32(92, 188, 164, 255), videoCallRoot.transform).GetComponent<Renderer>()
        };
        SetCallDecorationsVisible(false);

        statusCardRenderer = CreateCallCube("Call Standby Status Card", new Vector3(0f, 1.42f, 1.2f), new Vector3(3.5f, 1.25f, 0.035f), new Color32(68, 82, 104, 255), videoCallRoot.transform).GetComponent<Renderer>();
        callStatusBodyText = CreateWorldText(
            "Call Standby Status Text",
            new Vector3(0f, 1.42f, 0.98f),
            new Vector2(44f, 10f),
            "BETWEEN ROUNDS",
            4.8f,
            TextAlignmentOptions.Center,
            videoCallRoot.transform);

        participantTiles = new ParticipantTile[3];
        participantTiles[0] = CreateParticipantTile(0);
        participantTiles[1] = CreateParticipantTile(1);
        participantTiles[2] = CreateParticipantTile(2);
        ConfigureParticipantTiles(activeStageName, "FINAL ROUND", activeCallAccentColor);
    }

    private ParticipantTile CreateParticipantTile(int index)
    {
        GameObject tileRoot = new GameObject($"Video Call Participant {index + 1}");
        tileRoot.transform.SetParent(videoCallRoot.transform, false);

        ParticipantTile tile = new ParticipantTile
        {
            Root = tileRoot.transform,
            TileRenderer = CreateCallCube($"Participant {index + 1} Tile", Vector3.zero, new Vector3(1f, 0.78f, 0.035f), new Color32(84, 102, 128, 255), tileRoot.transform).GetComponent<Renderer>(),
            InnerRenderer = CreateCallCube($"Participant {index + 1} Inner Panel", new Vector3(0f, 0.02f, -0.035f), new Vector3(0.88f, 0.58f, 0.025f), new Color32(40, 52, 68, 255), tileRoot.transform).GetComponent<Renderer>(),
            LabelPlateRenderer = CreateCallCube($"Participant {index + 1} Label Plate", new Vector3(0f, -0.31f, -0.3f), new Vector3(0.72f, 0.14f, 0.025f), new Color32(12, 18, 28, 255), tileRoot.transform).GetComponent<Renderer>(),
            AvatarRoot = new GameObject($"Participant {index + 1} Avatar").transform,
            Label = CreateWorldText(
                $"Participant {index + 1} Label",
                new Vector3(0f, -0.31f, -0.44f),
                new Vector2(48f, 8f),
                "Interviewer",
                8f,
                TextAlignmentOptions.Center,
                tileRoot.transform),
            StatusDotRenderer = CreateCallSphere($"Participant {index + 1} Camera Status Dot", new Vector3(0.42f, 0.31f, -0.24f), new Vector3(0.055f, 0.055f, 0.055f), new Color32(122, 224, 159, 255), tileRoot.transform).GetComponent<Renderer>()
        };
        tile.TileIndex = index;

        tile.BorderRenderers = new Renderer[]
        {
            CreateCallCube($"Participant {index + 1} Top Active Border", new Vector3(0f, 0.41f, -0.24f), new Vector3(1.04f, 0.035f, 0.025f), activeCallAccentColor, tileRoot.transform).GetComponent<Renderer>(),
            CreateCallCube($"Participant {index + 1} Bottom Active Border", new Vector3(0f, -0.41f, -0.24f), new Vector3(1.04f, 0.035f, 0.025f), activeCallAccentColor, tileRoot.transform).GetComponent<Renderer>(),
            CreateCallCube($"Participant {index + 1} Left Active Border", new Vector3(-0.52f, 0f, -0.24f), new Vector3(0.035f, 0.82f, 0.025f), activeCallAccentColor, tileRoot.transform).GetComponent<Renderer>(),
            CreateCallCube($"Participant {index + 1} Right Active Border", new Vector3(0.52f, 0f, -0.24f), new Vector3(0.035f, 0.82f, 0.025f), activeCallAccentColor, tileRoot.transform).GetComponent<Renderer>()
        };

        tile.AvatarRoot.SetParent(tileRoot.transform, false);
        tile.HeadRenderer = CreateCallSphere($"Participant {index + 1} Head", new Vector3(0f, 0.13f, -0.26f), new Vector3(0.24f, 0.24f, 0.24f), new Color32(232, 238, 246, 255), tile.AvatarRoot).GetComponent<Renderer>();
        tile.BodyRenderer = CreateCallCapsule($"Participant {index + 1} Shoulders", new Vector3(0f, -0.15f, -0.26f), new Vector3(0.42f, 0.34f, 0.24f), new Color32(104, 138, 176, 255), tile.AvatarRoot).GetComponent<Renderer>();
        tile.InnerRenderer.gameObject.SetActive(false);
        tile.StatusDotRenderer.gameObject.SetActive(false);

        return tile;
    }

    private TMP_Text CreateWorldText(string name, Vector3 localPosition, Vector2 sizeDelta, string text, float fontSize, TextAlignmentOptions alignment, Transform parent)
    {
        GameObject textObject = new GameObject(name, typeof(TextMeshPro));
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = alignment;
        label.color = new Color32(232, 244, 250, 255);
        label.rectTransform.sizeDelta = sizeDelta;
        return label;
    }

    private void ConfigureParticipantTiles(string stageName, string displayText, Color accentColor)
    {
        if (participantTiles == null)
        {
            return;
        }

        int participantCount = GetParticipantCount(stageName);
        string[] labels = GetParticipantLabels(stageName);
        Vector3[] positions = GetParticipantPositions(participantCount);
        Vector3[] scales = GetParticipantScales(participantCount);
        bool showParticipants = participantCount > 0;
        activeCallAccentColor = accentColor;
        activeTileColor = Color.Lerp(new Color32(35, 46, 62, 255), accentColor, 0.32f);

        if (statusCardRenderer != null)
        {
            statusCardRenderer.gameObject.SetActive(!showParticipants);
            statusCardRenderer.material.color = Color.Lerp(new Color32(30, 38, 50, 255), accentColor, 0.16f);
        }

        if (callStatusBodyText != null)
        {
            callStatusBodyText.gameObject.SetActive(!showParticipants);
            callStatusBodyText.text = stageName == "Final Outcome" ? "FINAL DECISION" : "BETWEEN ROUNDS";
            callStatusBodyText.color = Color.Lerp(new Color32(232, 244, 250, 255), accentColor, 0.18f);
        }

        for (int i = 0; i < participantTiles.Length; i++)
        {
            ParticipantTile tile = participantTiles[i];
            bool isVisible = showParticipants && i < participantCount;
            tile.Root.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            tile.Root.localPosition = positions[i];
            tile.Root.localScale = Vector3.one;
            ApplyParticipantTileLayout(tile, scales[i]);
            tile.Label.text = labels[i];
            tile.TileRenderer.material.color = Color.Lerp(GetParticipantTileColor(i), accentColor, 0.08f);
            tile.LabelPlateRenderer.material.color = Color.Lerp(new Color32(10, 15, 24, 255), accentColor, 0.08f);
            ApplyRendererColors(tile.BorderRenderers, accentColor);
            SetBorderActive(tile, i == 0 && interviewerSpeaking);
            tile.BodyRenderer.material.color = Color.Lerp(GetParticipantBodyColor(i), accentColor, 0.24f + i * 0.05f);
            tile.HeadRenderer.material.color = GetParticipantHeadColor(i);
        }

        if (callStageText != null)
        {
            callStageText.text = GetCallTitle(stageName);
        }

        if (callStatusText != null)
        {
            callStatusText.text = showParticipants ? GetParticipantStatusText(participantCount) : "STANDBY";
        }
    }

    private int GetParticipantCount(string stageName)
    {
        switch (stageName)
        {
            case "Technical Panel":
                return 3;
            case "VP Round":
                return 2;
            case "Recruiter Screen":
            case "Hiring Manager":
                return 1;
            default:
                return 0;
        }
    }

    private string[] GetParticipantLabels(string stageName)
    {
        switch (stageName)
        {
            case "Recruiter Screen":
                return new[] { "Recruiter", string.Empty, string.Empty };
            case "Hiring Manager":
                return new[] { "Hiring Manager", string.Empty, string.Empty };
            case "Technical Panel":
                return new[] { "Solutions Lead", "Security Architect", "SE Manager" };
            case "VP Round":
                return new[] { "VP Sales Engineering", "Regional Director", string.Empty };
            case "Final Outcome":
                return new[] { "Decision Room", string.Empty, string.Empty };
            default:
                return new[] { "Waiting Room", string.Empty, string.Empty };
        }
    }

    private string GetCallTitle(string stageName)
    {
        switch (stageName)
        {
            case "Recruiter Screen":
                return "ONE-TO-ONE SCREEN";
            case "Hiring Manager":
                return "HIRING MANAGER CALL";
            case "Technical Panel":
                return "TECHNICAL PANEL";
            case "VP Round":
                return "FINAL LEADERSHIP CALL";
            case "Final Outcome":
                return "FINAL DECISION";
            case "Between Rounds":
                return "BETWEEN ROUNDS";
            default:
                return "LIVE INTERVIEW";
        }
    }

    private string GetParticipantStatusText(int participantCount)
    {
        if (participantCount <= 1)
        {
                return "LIVE CALL";
        }

        return $"LIVE CALL  {participantCount}";
    }

    private Color GetParticipantTileColor(int participantIndex)
    {
        switch (participantIndex)
        {
            case 1:
                return new Color32(62, 76, 96, 255);
            case 2:
                return new Color32(55, 70, 78, 255);
            default:
                return new Color32(70, 86, 108, 255);
        }
    }

    private Color GetParticipantBodyColor(int participantIndex)
    {
        switch (participantIndex)
        {
            case 1:
                return new Color32(90, 116, 150, 255);
            case 2:
                return new Color32(100, 126, 116, 255);
            default:
                return new Color32(108, 136, 172, 255);
        }
    }

    private Color GetParticipantHeadColor(int participantIndex)
    {
        switch (participantIndex)
        {
            case 1:
                return new Color32(226, 232, 236, 255);
            case 2:
                return new Color32(218, 228, 220, 255);
            default:
                return new Color32(232, 238, 246, 255);
        }
    }

    private Vector3[] GetParticipantPositions(int participantCount)
    {
        if (participantCount == 1)
        {
            return new[]
            {
                new Vector3(0f, 1.50f, 1.74f),
                Vector3.zero,
                Vector3.zero
            };
        }

        if (participantCount == 2)
        {
            return new[]
            {
                new Vector3(-1.16f, 1.48f, 1.74f),
                new Vector3(1.16f, 1.48f, 1.74f),
                Vector3.zero
            };
        }

        return new[]
        {
            new Vector3(-1.58f, 1.50f, 1.74f),
            new Vector3(0f, 1.50f, 1.74f),
            new Vector3(1.58f, 1.50f, 1.74f)
        };
    }

    private Vector3[] GetParticipantScales(int participantCount)
    {
        if (participantCount == 1)
        {
            return new[]
            {
                new Vector3(4.5f, 2.64f, 1f),
                Vector3.one,
                Vector3.one
            };
        }

        if (participantCount == 2)
        {
            return new[]
            {
                new Vector3(2.16f, 2.34f, 1f),
                new Vector3(2.16f, 2.34f, 1f),
                Vector3.one
            };
        }

        return new[]
        {
            new Vector3(1.42f, 2.22f, 1f),
            new Vector3(1.42f, 2.22f, 1f),
            new Vector3(1.42f, 2.22f, 1f)
        };
    }

    private void ApplyCallTheme(string stageName, Color accentColor)
    {
        Color baseScreen = Color.Lerp(new Color32(18, 23, 32, 255), accentColor, 0.09f);
        Color header = Color.Lerp(new Color32(29, 37, 49, 255), accentColor, 0.16f);

        switch (activeCompanyProfileName)
        {
            case "Big SaaS Vendor":
                baseScreen = Color.Lerp(new Color32(31, 38, 48, 255), new Color32(114, 158, 202, 255), 0.16f);
                header = new Color32(45, 56, 70, 255);
                break;
            case "Startup Rocketship":
                baseScreen = Color.Lerp(new Color32(48, 35, 34, 255), new Color32(255, 116, 72, 255), 0.15f);
                header = new Color32(72, 48, 42, 255);
                break;
            case "Security Vendor":
                baseScreen = Color.Lerp(new Color32(12, 24, 30, 255), new Color32(58, 220, 172, 255), 0.1f);
                header = new Color32(22, 44, 52, 255);
                break;
            case "AI Hype Company":
                baseScreen = Color.Lerp(new Color32(28, 22, 45, 255), new Color32(116, 236, 255, 255), 0.15f);
                header = new Color32(42, 30, 68, 255);
                break;
            case "Legacy Enterprise":
                baseScreen = new Color32(54, 53, 49, 255);
                header = new Color32(68, 66, 60, 255);
                break;
        }

        float pressureTension = Mathf.InverseLerp(50f, 100f, activeInterviewPressure);
        baseScreen = Color.Lerp(baseScreen, new Color32(46, 34, 46, 255), pressureTension * 0.12f);
        header = Color.Lerp(header, accentColor, 0.12f + pressureTension * 0.16f);

        ApplyRendererColor(callScreenRenderer, baseScreen);
        ApplyRendererColor(callHeaderRenderer, header);
        ApplyRendererColor(callGlowRenderer, Color.Lerp(accentColor, new Color32(18, 22, 30, 255), 0.62f));
        ApplyRendererColors(callBackgroundAccentRenderers, accentColor);

        if (callStageText != null)
        {
            callStageText.color = Color.Lerp(new Color32(232, 244, 250, 255), accentColor, 0.18f);
        }

        if (callStatusText != null)
        {
            callStatusText.color = IsInterviewStage(stageName)
                ? Color.Lerp(new Color32(178, 234, 220, 255), accentColor, 0.38f)
                : new Color32(178, 186, 198, 255);
        }
    }

    private void AnimateVideoCall(float time)
    {
        if (participantTiles == null)
        {
            return;
        }

        float pressureTension = Mathf.InverseLerp(35f, 100f, activeInterviewPressure);
        float idleSpeed = Mathf.Lerp(0.8f, 1.9f, pressureTension);
        float idleAmount = Mathf.Lerp(0.012f, 0.038f, pressureTension);
        int activeSpeakerIndex = GetActiveSpeakerIndex(time);

        for (int i = 0; i < participantTiles.Length; i++)
        {
            ParticipantTile tile = participantTiles[i];
            if (tile == null || tile.Root == null || !tile.Root.gameObject.activeSelf)
            {
                continue;
            }

            bool activeSpeaker = interviewerSpeaking && i == activeSpeakerIndex && IsInterviewStage(activeStageName);
            float bob = Mathf.Sin(time * idleSpeed + i * 1.7f) * idleAmount;
            float nod = activeSpeaker ? Mathf.Sin(time * (1.4f + pressureTension) + i) * Mathf.Lerp(0.35f, 0.9f, pressureTension) : 0f;
            tile.AvatarRoot.localPosition = new Vector3(0f, bob, -0.1f);
            tile.AvatarRoot.localRotation = Quaternion.Euler(nod, 0f, 0f);

            float pulse = activeSpeaker ? 0.55f + Mathf.Sin(time * Mathf.Lerp(2.8f, 5.5f, pressureTension)) * 0.22f : 0f;
            Color baseTileColor = GetParticipantTileColor(tile.TileIndex);
            tile.TileRenderer.material.color = activeSpeaker
                ? Color.Lerp(baseTileColor, activeCallAccentColor, 0.16f + Mathf.Clamp01(pulse) * Mathf.Lerp(0.03f, 0.1f, pressureTension))
                : Color.Lerp(Color.Lerp(baseTileColor, Color.black, 0.22f), activeCallAccentColor, pressureTension * 0.035f);
            SetBorderActive(tile, activeSpeaker);
            SetBorderPulse(tile, 1f + pulse * Mathf.Lerp(0.025f, 0.055f, pressureTension));
        }

        if (callGlowRenderer != null)
        {
            float glowPulse = 0.5f + Mathf.Sin(time * Mathf.Lerp(1.0f, 3.6f, pressureTension)) * 0.5f;
            callGlowRenderer.material.color = Color.Lerp(
                Color.Lerp(activeCallAccentColor, Color.black, 0.76f),
                Color.Lerp(new Color32(224, 66, 120, 255), activeCallAccentColor, 0.35f),
                pressureTension * glowPulse * 0.36f);
        }

        SetCallDecorationsVisible(false);
    }

    private int GetActiveSpeakerIndex(float time)
    {
        int participantCount = GetParticipantCount(activeStageName);
        if (participantCount <= 1)
        {
            return 0;
        }

        float interval = Mathf.Lerp(4.5f, 2.4f, Mathf.InverseLerp(50f, 100f, activeInterviewPressure));
        return Mathf.FloorToInt(time / interval) % participantCount;
    }

    private bool IsInterviewStage(string stageName)
    {
        return stageName == "Recruiter Screen"
            || stageName == "Hiring Manager"
            || stageName == "Technical Panel"
            || stageName == "VP Round";
    }

    private void SetBorderActive(ParticipantTile tile, bool active)
    {
        if (tile == null || tile.BorderRenderers == null)
        {
            return;
        }

        for (int i = 0; i < tile.BorderRenderers.Length; i++)
        {
            if (tile.BorderRenderers[i] != null)
            {
                tile.BorderRenderers[i].gameObject.SetActive(active && i == 0);
            }
        }
    }

    private void ApplyParticipantTileLayout(ParticipantTile tile, Vector3 size)
    {
        if (tile == null)
        {
            return;
        }

        float width = size.x;
        float height = size.y;
        float avatarVariant = tile.TileIndex == 1 ? 0.92f : tile.TileIndex == 2 ? 1.06f : 1f;
        float avatarSize = Mathf.Min(width, height) * 0.22f * avatarVariant;
        float shoulderWidth = Mathf.Min(width, height) * (tile.TileIndex == 1 ? 0.38f : tile.TileIndex == 2 ? 0.46f : 0.42f);
        float borderThickness = 0.085f;

        tile.TileRenderer.transform.localPosition = Vector3.zero;
        tile.TileRenderer.transform.localScale = new Vector3(width, height, 0.035f);
        tile.HeadRenderer.transform.localPosition = new Vector3(0f, height * 0.14f, -0.72f);
        tile.HeadRenderer.transform.localScale = new Vector3(avatarSize * (tile.TileIndex == 2 ? 0.92f : 1f), avatarSize, avatarSize);
        tile.BodyRenderer.transform.localPosition = new Vector3(0f, -height * 0.08f, -0.72f);
        tile.BodyRenderer.transform.localScale = new Vector3(shoulderWidth, height * (tile.TileIndex == 1 ? 0.21f : 0.24f), avatarSize);
        tile.LabelPlateRenderer.transform.localPosition = new Vector3(0f, -height * 0.35f, -1.02f);
        tile.LabelPlateRenderer.transform.localScale = new Vector3(width * 0.92f, height * 0.24f, 0.025f);
        tile.Label.transform.localPosition = new Vector3(0f, -height * 0.355f, -1.18f);
        tile.Label.fontSize = height >= 2.3f ? 9.6f : 6.2f;
        tile.Label.rectTransform.sizeDelta = new Vector2(width * 12f, 7f);

        if (tile.BorderRenderers == null || tile.BorderRenderers.Length < 4)
        {
            return;
        }

        tile.BorderRenderers[0].transform.localPosition = new Vector3(0f, height * 0.5f, -0.86f);
        tile.BorderRenderers[0].transform.localScale = new Vector3(width + borderThickness, borderThickness, 0.025f);
        tile.BorderRenderers[1].transform.localPosition = new Vector3(0f, -height * 0.5f, -0.86f);
        tile.BorderRenderers[1].transform.localScale = new Vector3(width + borderThickness, borderThickness, 0.025f);
        tile.BorderRenderers[2].transform.localPosition = new Vector3(-width * 0.5f, 0f, -0.86f);
        tile.BorderRenderers[2].transform.localScale = new Vector3(borderThickness, height + borderThickness, 0.025f);
        tile.BorderRenderers[3].transform.localPosition = new Vector3(width * 0.5f, 0f, -0.86f);
        tile.BorderRenderers[3].transform.localScale = new Vector3(borderThickness, height + borderThickness, 0.025f);
    }

    private void SetBorderPulse(ParticipantTile tile, float scale)
    {
        if (tile == null || tile.BorderRenderers == null)
        {
            return;
        }

        for (int i = 0; i < tile.BorderRenderers.Length; i++)
        {
            if (tile.BorderRenderers[i] != null)
            {
                tile.BorderRenderers[i].transform.localScale = new Vector3(
                    tile.BorderRenderers[i].transform.localScale.x,
                    tile.BorderRenderers[i].transform.localScale.y,
                    0.025f * scale);
            }
        }
    }

    private void CreateCompanyIdentityProps()
    {
        saasVendorProps = CreatePropRoot("Big SaaS Vendor Props");
        CreateCube("Polished Table Strip", new Vector3(0f, 0.955f, -0.04f), new Vector3(2.65f, 0.025f, 0.04f), new Color32(142, 170, 194, 255), saasVendorProps.transform);
        CreateCube("Boardroom Frosted Panel", new Vector3(-2.55f, 1.76f, 2.38f), new Vector3(0.62f, 1.0f, 0.035f), new Color32(82, 108, 132, 255), saasVendorProps.transform);
        CreateCube("Boardroom Frosted Panel 2", new Vector3(2.55f, 1.76f, 2.38f), new Vector3(0.62f, 1.0f, 0.035f), new Color32(82, 108, 132, 255), saasVendorProps.transform);

        startupMessProps = CreatePropRoot("Startup Rocketship Props");
        CreateCube("Sticky Note Stack", new Vector3(0.95f, 1.02f, 0.22f), new Vector3(0.25f, 0.025f, 0.18f), new Color32(255, 204, 74, 255), startupMessProps.transform);
        CreateCube("Second Laptop Glow Block", new Vector3(0.62f, 1.04f, 0.35f), new Vector3(0.45f, 0.04f, 0.28f), new Color32(255, 88, 66, 255), startupMessProps.transform);
        CreateCube("Whiteboard Side Quest", new Vector3(-2.7f, 1.72f, 2.4f), new Vector3(0.72f, 0.82f, 0.04f), new Color32(116, 82, 72, 255), startupMessProps.transform);

        securityOpsProps = CreatePropRoot("Security Vendor Props");
        CreateCube("Security Ops Left Rail", new Vector3(-1.9f, 1.9f, 2.39f), new Vector3(0.06f, 0.88f, 0.04f), new Color32(42, 190, 146, 255), securityOpsProps.transform);
        CreateCube("Security Ops Right Rail", new Vector3(1.9f, 1.9f, 2.39f), new Vector3(0.06f, 0.88f, 0.04f), new Color32(42, 190, 146, 255), securityOpsProps.transform);
        CreateCube("Formal Nameplate", new Vector3(0.2f, 1.0f, 0.02f), new Vector3(0.62f, 0.08f, 0.05f), new Color32(44, 76, 86, 255), securityOpsProps.transform);

        aiHypeProps = CreatePropRoot("AI Hype Company Props");
        CreateCube("Neon Cyan Rail", new Vector3(-2.35f, 1.1f, 2.38f), new Vector3(0.08f, 1.5f, 0.04f), new Color32(70, 236, 255, 255), aiHypeProps.transform);
        CreateCube("Neon Purple Rail", new Vector3(2.35f, 1.1f, 2.38f), new Vector3(0.08f, 1.5f, 0.04f), new Color32(184, 94, 255, 255), aiHypeProps.transform);
        CreateSphere("Overdramatic Demo Orb", new Vector3(0.9f, 1.25f, 0.32f), new Vector3(0.22f, 0.22f, 0.22f), new Color32(120, 235, 255, 255), aiHypeProps.transform);

        legacyEnterpriseProps = CreatePropRoot("Legacy Enterprise Props");
        CreateCube("Conservative Table Blotter", new Vector3(0.38f, 0.985f, 0.16f), new Vector3(0.72f, 0.025f, 0.42f), new Color32(74, 86, 74, 255), legacyEnterpriseProps.transform);
        CreateCube("Committee Name Plate", new Vector3(-0.35f, 1.0f, 0.03f), new Vector3(0.76f, 0.08f, 0.05f), new Color32(118, 112, 96, 255), legacyEnterpriseProps.transform);
        CreateCube("Muted Wall Plaque", new Vector3(2.55f, 1.75f, 2.4f), new Vector3(0.58f, 0.44f, 0.04f), new Color32(102, 98, 88, 255), legacyEnterpriseProps.transform);

        SetCompanyPropsActive(activeCompanyProfileName);
    }

    private GameObject CreatePropRoot(string rootName)
    {
        GameObject propRoot = new GameObject(rootName);
        propRoot.transform.SetParent(roomRoot.transform, false);
        propRoot.transform.localPosition = Vector3.zero;
        propRoot.transform.localRotation = Quaternion.identity;
        propRoot.transform.localScale = Vector3.one;
        return propRoot;
    }

    private GameObject CreateCallCube(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().material = CreateFlatMaterial(name + " Material", color);
        RemoveCollider(cube);
        return cube;
    }

    private GameObject CreateCallSphere(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent, false);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = scale;
        sphere.GetComponent<Renderer>().material = CreateFlatMaterial(name + " Material", color);
        RemoveCollider(sphere);
        return sphere;
    }

    private GameObject CreateCallCapsule(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = name;
        capsule.transform.SetParent(parent, false);
        capsule.transform.localPosition = position;
        capsule.transform.localScale = scale;
        capsule.GetComponent<Renderer>().material = CreateFlatMaterial(name + " Material", color);
        RemoveCollider(capsule);
        return capsule;
    }

    private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Color color, Transform parent = null)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent == null ? roomRoot.transform : parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().material = CreateMaterial(name + " Material", color);
        RemoveCollider(cube);
        return cube;
    }

    private GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Color color, Transform parent = null)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent == null ? roomRoot.transform : parent, false);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = scale;
        sphere.GetComponent<Renderer>().material = CreateMaterial(name + " Material", color);
        RemoveCollider(sphere);
        return sphere;
    }

    private GameObject CreateCapsule(string name, Vector3 position, Vector3 scale, Color color, Transform parent = null)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = name;
        capsule.transform.SetParent(parent == null ? roomRoot.transform : parent, false);
        capsule.transform.localPosition = position;
        capsule.transform.localScale = scale;
        capsule.GetComponent<Renderer>().material = CreateMaterial(name + " Material", color);
        RemoveCollider(capsule);
        return capsule;
    }

    private void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Sprites/Default")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = name;
        ApplyMaterialColor(material, color);

        return material;
    }

    private Material CreateFlatMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Sprites/Default")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = name;
        ApplyMaterialColor(material, color);
        return material;
    }

    private static void ApplyMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

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

    private sealed class ParticipantTile
    {
        public int TileIndex;
        public Transform Root;
        public Transform AvatarRoot;
        public Renderer TileRenderer;
        public Renderer InnerRenderer;
        public Renderer LabelPlateRenderer;
        public Renderer[] BorderRenderers;
        public Renderer StatusDotRenderer;
        public Renderer HeadRenderer;
        public Renderer BodyRenderer;
        public TMP_Text Label;
    }

    private void LogBackdropState(string action)
    {
        string cameraName = roomCamera == null ? "none" : roomCamera.name;
        string cameraPosition = roomCamera == null ? "n/a" : roomCamera.transform.position.ToString("F2");
        string cameraRotation = roomCamera == null ? "n/a" : roomCamera.transform.rotation.eulerAngles.ToString("F1");
        string rootPosition = roomRoot == null ? "n/a" : roomRoot.transform.position.ToString("F2");

        Debug.Log(
            $"Final Round 3D backdrop {action}\n" +
            $"Root exists: {roomRoot != null}\n" +
            $"Root position: {rootPosition}\n" +
            $"Active camera: {cameraName}\n" +
            $"Camera position: {cameraPosition}\n" +
            $"Camera rotation: {cameraRotation}\n" +
            $"Camera tag: {(roomCamera == null ? "n/a" : roomCamera.tag)}\n" +
            $"Debug visibility: {debugBackdropVisibility}");
    }
}
