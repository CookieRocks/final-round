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
    private Renderer wallPanelRenderer;
    private Renderer laptopScreenRenderer;
    private TMP_Text wallStageText;
    private Light rimLight;

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

    public void SetStageAtmosphere(string stageName)
    {
        if (mainLight == null || accentLight == null || rimLight == null || wallStageText == null)
        {
            return;
        }

        Color mainColor = new Color32(255, 242, 220, 255);
        Color accentColor = new Color32(74, 143, 166, 255);
        float mainIntensity = 1.35f;
        float accentIntensity = 1.05f;
        float rimIntensity = 1.1f;
        string displayText = string.IsNullOrEmpty(stageName) ? "FINAL ROUND" : stageName.ToUpperInvariant();

        switch (stageName)
        {
            case "Recruiter Screen":
                mainColor = new Color32(255, 232, 205, 255);
                accentColor = new Color32(96, 180, 168, 255);
                mainIntensity = 1.35f;
                accentIntensity = 0.95f;
                rimIntensity = 0.9f;
                break;
            case "Hiring Manager":
                mainColor = new Color32(255, 244, 224, 255);
                accentColor = new Color32(95, 135, 190, 255);
                mainIntensity = 1.4f;
                accentIntensity = 1.0f;
                rimIntensity = 1.0f;
                break;
            case "Technical Panel":
                mainColor = new Color32(226, 238, 255, 255);
                accentColor = new Color32(89, 162, 255, 255);
                mainIntensity = 1.2f;
                accentIntensity = 1.35f;
                rimIntensity = 1.25f;
                break;
            case "VP Round":
                mainColor = new Color32(255, 222, 188, 255);
                accentColor = new Color32(184, 103, 255, 255);
                mainIntensity = 1.15f;
                accentIntensity = 1.55f;
                rimIntensity = 1.45f;
                break;
            case "Final Outcome":
                mainColor = new Color32(244, 240, 230, 255);
                accentColor = new Color32(218, 218, 206, 255);
                mainIntensity = 1.45f;
                accentIntensity = 0.8f;
                rimIntensity = 1.2f;
                displayText = "FINAL DECISION";
                break;
            case "Between Rounds":
                mainColor = new Color32(230, 236, 255, 255);
                accentColor = new Color32(123, 154, 220, 255);
                mainIntensity = 1.2f;
                accentIntensity = 1.15f;
                rimIntensity = 1.0f;
                break;
            default:
                displayText = "FINAL ROUND";
                break;
        }

        mainLight.color = mainColor;
        mainLight.intensity = debugBackdropVisibility ? mainIntensity + 0.75f : mainIntensity;
        accentLight.color = accentColor;
        accentLight.intensity = debugBackdropVisibility ? accentIntensity + 0.7f : accentIntensity;
        rimLight.color = Color.Lerp(accentColor, Color.white, 0.28f);
        rimLight.intensity = debugBackdropVisibility ? rimIntensity + 0.65f : rimIntensity;
        wallStageText.text = displayText;
        wallStageText.fontSize = debugBackdropVisibility ? 9f : 7f;

        if (wallPanelRenderer != null)
        {
            wallPanelRenderer.material.color = new Color(accentColor.r * 0.55f, accentColor.g * 0.55f, accentColor.b * 0.55f, 1f);
        }

        if (laptopScreenRenderer != null)
        {
            laptopScreenRenderer.material.color = new Color(accentColor.r * 0.75f, accentColor.g * 0.75f, accentColor.b * 0.75f, 1f);
        }

        if (debugBackdropVisibility)
        {
            Debug.Log(
                "Final Round backdrop atmosphere applied\n" +
                $"Stage: {displayText}\n" +
                $"Debug visibility: {debugBackdropVisibility}");
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
        cameraObject.transform.position = new Vector3(0f, 1.58f, -3.85f);
        cameraObject.transform.LookAt(new Vector3(0f, 1.32f, 1.08f));

        roomCamera = cameraObject.GetComponent<Camera>();
        roomCamera.enabled = true;
        roomCamera.clearFlags = CameraClearFlags.SolidColor;
        roomCamera.backgroundColor = debugBackdropVisibility ? new Color32(18, 21, 28, 255) : new Color32(12, 14, 19, 255);
        roomCamera.fieldOfView = debugBackdropVisibility ? 58f : 50f;
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
        CreateCube("Floor", new Vector3(0f, -0.04f, 0.2f), new Vector3(7f, 0.08f, 6.4f), floorColor);
        CreateCube("Back Wall", new Vector3(0f, 1.65f, 2.55f), new Vector3(7.5f, 3.4f, 0.12f), wallColor);
        CreateCube("Left Wall", new Vector3(-3.45f, 1.65f, 0.2f), new Vector3(0.12f, 3.4f, 5.0f), new Color32(34, 39, 49, 255));
        CreateCube("Right Wall", new Vector3(3.45f, 1.65f, 0.2f), new Vector3(0.12f, 3.4f, 5.0f), new Color32(34, 39, 49, 255));
    }

    private void CreateFurniture()
    {
        CreateCube("Interview Desk Top", new Vector3(0f, 0.82f, 0.6f), new Vector3(3.05f, 0.16f, 1.0f), deskColor);
        CreateCube("Interview Desk Front", new Vector3(0f, 0.46f, 1.02f), new Vector3(3.05f, 0.72f, 0.1f), new Color32(58, 40, 30, 255));
        CreateCube("Desk Left Leg", new Vector3(-1.28f, 0.42f, 0.25f), new Vector3(0.16f, 0.84f, 0.16f), deskColor);
        CreateCube("Desk Right Leg", new Vector3(1.28f, 0.42f, 0.25f), new Vector3(0.16f, 0.84f, 0.16f), deskColor);
        CreateCube("Desk Front Edge", new Vector3(0f, 0.93f, 0.08f), new Vector3(3.12f, 0.08f, 0.16f), new Color32(117, 77, 48, 255));

        CreateChair("Candidate Chair", new Vector3(-1.4f, 0f, -1.25f), 155f);
        CreateChair("Interviewer Chair", new Vector3(0.15f, 0f, 1.55f), 0f);
    }

    private void CreateChair(string name, Vector3 basePosition, float yaw)
    {
        Transform chairRoot = new GameObject(name).transform;
        chairRoot.SetParent(roomRoot.transform, false);
        chairRoot.position = basePosition;
        chairRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

        CreateCube(name + " Seat", new Vector3(0f, 0.46f, 0f), new Vector3(0.85f, 0.15f, 0.72f), chairColor, chairRoot);
        CreateCube(name + " Back", new Vector3(0f, 0.98f, 0.32f), new Vector3(0.85f, 0.95f, 0.14f), chairColor, chairRoot);
        CreateCube(name + " Left Leg", new Vector3(-0.32f, 0.22f, -0.22f), new Vector3(0.1f, 0.46f, 0.1f), chairColor, chairRoot);
        CreateCube(name + " Right Leg", new Vector3(0.32f, 0.22f, -0.22f), new Vector3(0.1f, 0.46f, 0.1f), chairColor, chairRoot);
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

        CreateSphere("Interviewer Head", new Vector3(0.18f, 1.84f, 1.58f), new Vector3(0.42f, 0.42f, 0.42f), silhouetteColor);
        CreateCube("Interviewer Left Arm", new Vector3(-0.34f, 1.15f, 1.2f), new Vector3(0.16f, 0.58f, 0.18f), silhouetteColor);
        CreateCube("Interviewer Right Arm", new Vector3(0.7f, 1.15f, 1.2f), new Vector3(0.16f, 0.58f, 0.18f), silhouetteColor);
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

    private GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(roomRoot.transform, false);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = scale;
        sphere.GetComponent<Renderer>().material = CreateMaterial(name + " Material", color);
        RemoveCollider(sphere);
        return sphere;
    }

    private GameObject CreateCapsule(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = name;
        capsule.transform.SetParent(roomRoot.transform, false);
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
