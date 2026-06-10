using UnityEngine;

public enum InterviewerReaction
{
    Neutral,
    Listening,
    Positive,
    Awkward,
    Concerned
}

public sealed class InterviewerPlaceholder : MonoBehaviour
{
    [SerializeField] private Renderer panelRenderer;
    [SerializeField] private Renderer avatarRenderer;
    [SerializeField] private Renderer reactionRenderer;
    [SerializeField] private Transform emphasisRoot;
    [SerializeField] private Renderer nameplateRenderer;
    [SerializeField] private Light emphasisLight;

    private Color neutralColor = new Color32(44, 50, 62, 255);
    private Color listeningColor = new Color32(54, 75, 86, 255);
    private Color positiveColor = new Color32(54, 96, 74, 255);
    private Color awkwardColor = new Color32(82, 78, 64, 255);
    private Color concernedColor = new Color32(100, 64, 70, 255);
    private Vector3 baseAvatarScale = Vector3.one;
    private Quaternion baseAvatarRotation = Quaternion.identity;
    private Vector3 targetAvatarScale = Vector3.one;
    private Quaternion targetAvatarRotation = Quaternion.identity;
    private Color baseNameplateColor = new Color32(28, 34, 42, 255);
    private float baseEmphasisLightIntensity;
    private float targetEmphasisLightIntensity;
    private InterviewerReaction currentReaction = InterviewerReaction.Neutral;
    private float idleSeed;

    private void Awake()
    {
        idleSeed = Random.Range(0f, 100f);
        CaptureBaseAvatarTransform();
    }

    private void Update()
    {
        Transform animatedTransform = GetAnimatedTransform();
        if (animatedTransform == null)
        {
            return;
        }

        if (currentReaction == InterviewerReaction.Neutral)
        {
            float idleTilt = Mathf.Sin(Time.time * 0.72f + idleSeed) * 0.45f;
            targetAvatarRotation = baseAvatarRotation * Quaternion.Euler(idleTilt, 0f, idleTilt * 0.2f);
        }
        else if (currentReaction == InterviewerReaction.Listening)
        {
            float nod = Mathf.Sin(Time.time * 2.1f + idleSeed) * 1.4f;
            float sideTilt = Mathf.Sin(Time.time * 0.9f + idleSeed) * 0.35f;
            targetAvatarRotation = baseAvatarRotation * Quaternion.Euler(nod, 0f, sideTilt);
        }

        animatedTransform.localScale = Vector3.Lerp(animatedTransform.localScale, targetAvatarScale, Time.deltaTime * 8f);
        animatedTransform.localRotation = Quaternion.Slerp(animatedTransform.localRotation, targetAvatarRotation, Time.deltaTime * 8f);

        if (emphasisLight != null)
        {
            emphasisLight.intensity = Mathf.Lerp(emphasisLight.intensity, targetEmphasisLightIntensity, Time.deltaTime * 8f);
        }
    }

    public void Configure(Renderer panel, Renderer avatar, Renderer reaction)
    {
        Configure(panel, avatar, reaction, null, null, null);
    }

    public void Configure(Renderer panel, Renderer avatar, Renderer reaction, Transform characterRoot, Renderer nameplate, Light light)
    {
        panelRenderer = panel;
        avatarRenderer = avatar;
        reactionRenderer = reaction;
        emphasisRoot = characterRoot;
        nameplateRenderer = nameplate;
        emphasisLight = light;
        CaptureBaseAvatarTransform();
        if (nameplateRenderer != null)
        {
            baseNameplateColor = nameplateRenderer.material.color;
        }
        if (emphasisLight != null)
        {
            baseEmphasisLightIntensity = emphasisLight.intensity;
            targetEmphasisLightIntensity = emphasisLight.intensity;
        }
        SetReaction(InterviewerReaction.Neutral);
    }

    public void SetReaction(InterviewerReaction reaction)
    {
        currentReaction = reaction;
        Color color = reaction switch
        {
            InterviewerReaction.Listening => listeningColor,
            InterviewerReaction.Positive => positiveColor,
            InterviewerReaction.Awkward => awkwardColor,
            InterviewerReaction.Concerned => concernedColor,
            _ => neutralColor
        };

        targetAvatarScale = reaction == InterviewerReaction.Positive
            ? baseAvatarScale * 1.06f
            : reaction == InterviewerReaction.Concerned
                ? new Vector3(baseAvatarScale.x * 0.97f, baseAvatarScale.y * 0.95f, baseAvatarScale.z * 0.97f)
                : baseAvatarScale;
        targetAvatarRotation = reaction switch
        {
            InterviewerReaction.Positive => baseAvatarRotation * Quaternion.Euler(2.25f, 0f, 0f),
            InterviewerReaction.Awkward => baseAvatarRotation * Quaternion.Euler(-1.75f, 0f, 0f),
            InterviewerReaction.Concerned => baseAvatarRotation * Quaternion.Euler(-3.5f, 0f, 0f),
            _ => baseAvatarRotation
        };

        ApplyColor(panelRenderer, color);
        ApplyColor(reactionRenderer, Color.Lerp(color, Color.white, 0.35f));
        ApplyColor(nameplateRenderer, Color.Lerp(baseNameplateColor, color, reaction == InterviewerReaction.Neutral ? 0.12f : 0.45f));
        targetEmphasisLightIntensity = reaction == InterviewerReaction.Neutral
            ? baseEmphasisLightIntensity
            : reaction == InterviewerReaction.Listening
                ? baseEmphasisLightIntensity + 0.12f
                : baseEmphasisLightIntensity + 0.35f;
    }

    private void CaptureBaseAvatarTransform()
    {
        Transform animatedTransform = GetAnimatedTransform();
        if (animatedTransform == null)
        {
            return;
        }

        baseAvatarScale = animatedTransform.localScale;
        baseAvatarRotation = animatedTransform.localRotation;
        targetAvatarScale = baseAvatarScale;
        targetAvatarRotation = baseAvatarRotation;
    }

    private Transform GetAnimatedTransform()
    {
        if (emphasisRoot != null)
        {
            return emphasisRoot;
        }

        return avatarRenderer == null ? null : avatarRenderer.transform;
    }

    private static void ApplyColor(Renderer renderer, Color color)
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
    }
}
