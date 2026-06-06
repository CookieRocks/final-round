using TMPro;
using UnityEngine;

public class InterviewRoomBackdropController : MonoBehaviour
{
    public static readonly bool debugBackdropVisibility = false;

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
    private GameObject startupMessProps;
    private GameObject securityOpsProps;
    private GameObject aiHypeProps;
    private GameObject legacyEnterpriseProps;
    private GameObject saasVendorProps;
    private string activeCompanyProfileName = string.Empty;

    public RenderTexture ViewportTexture => viewportTexture;

    private void Awake()
    {
        gameObject.name = ControllerName;

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

    public void SetProcessAtmosphere(string companyProfileName, string stageName)
    {
        activeCompanyProfileName = companyProfileName ?? string.Empty;
        SetStageAtmosphere(stageName);
    }

    public void SetStageAtmosphere(string stageName)
    {
        if (mainLight == null || accentLight == null || rimLight == null || wallStageText == null)
        {
            return;
        }

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

        mainLight.color = mainColor;
        mainLight.intensity = debugBackdropVisibility ? mainIntensity + 0.75f : mainIntensity;
        accentLight.color = accentColor;
        accentLight.intensity = debugBackdropVisibility ? accentIntensity + 0.7f : accentIntensity;
        rimLight.color = Color.Lerp(accentColor, Color.white, 0.28f);
        rimLight.intensity = debugBackdropVisibility ? rimIntensity + 0.65f : rimIntensity;
        laptopLight.intensity = debugBackdropVisibility ? laptopIntensity + 0.65f : laptopIntensity;
        wallStageText.text = displayText;
        wallStageText.fontSize = debugBackdropVisibility ? 9f : 7f;

        if (wallPanelRenderer != null)
        {
            wallPanelRenderer.material.color = new Color(accentColor.r * 0.55f, accentColor.g * 0.55f, accentColor.b * 0.55f, 1f);
        }

        if (laptopScreenRenderer != null)
        {
            laptopScreenRenderer.material.color = new Color(accentColor.r * 0.9f, accentColor.g * 0.9f, accentColor.b * 0.9f, 1f);
        }

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

        if (roomCamera != null)
        {
            roomCamera.backgroundColor = cameraBackground;
        }
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
        CreateFurniture();
        CreateLaptop();
        CreateInterviewerSilhouette();
        CreateWallDisplay();
        CreateCompanyIdentityProps();
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
        cameraObject.transform.position = new Vector3(0f, 1.6f, -3.85f);
        cameraObject.transform.LookAt(new Vector3(0f, 1.38f, 1.18f));

        roomCamera = cameraObject.GetComponent<Camera>();
        roomCamera.enabled = true;
        roomCamera.clearFlags = CameraClearFlags.SolidColor;
        roomCamera.backgroundColor = debugBackdropVisibility ? new Color32(18, 21, 28, 255) : new Color32(12, 14, 19, 255);
        roomCamera.fieldOfView = debugBackdropVisibility ? 58f : 52f;
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
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        return material;
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
