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

    private Color neutralColor = new Color32(44, 50, 62, 255);
    private Color listeningColor = new Color32(54, 75, 86, 255);
    private Color positiveColor = new Color32(54, 96, 74, 255);
    private Color awkwardColor = new Color32(82, 78, 64, 255);
    private Color concernedColor = new Color32(100, 64, 70, 255);
    private Vector3 baseAvatarScale = Vector3.one;
    private Quaternion baseAvatarRotation = Quaternion.identity;
    private Vector3 targetAvatarScale = Vector3.one;
    private Quaternion targetAvatarRotation = Quaternion.identity;
    private InterviewerReaction currentReaction = InterviewerReaction.Neutral;

    private void Awake()
    {
        CaptureBaseAvatarTransform();
    }

    private void Update()
    {
        if (avatarRenderer == null)
        {
            return;
        }

        if (currentReaction == InterviewerReaction.Listening)
        {
            float nod = Mathf.Sin(Time.time * 2.1f + transform.GetSiblingIndex()) * 1.4f;
            targetAvatarRotation = Quaternion.Euler(nod, 0f, 0f);
        }

        avatarRenderer.transform.localScale = Vector3.Lerp(avatarRenderer.transform.localScale, targetAvatarScale, Time.deltaTime * 8f);
        avatarRenderer.transform.localRotation = Quaternion.Slerp(avatarRenderer.transform.localRotation, targetAvatarRotation, Time.deltaTime * 8f);
    }

    public void Configure(Renderer panel, Renderer avatar, Renderer reaction)
    {
        panelRenderer = panel;
        avatarRenderer = avatar;
        reactionRenderer = reaction;
        CaptureBaseAvatarTransform();
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
            ? baseAvatarScale * 1.04f
            : reaction == InterviewerReaction.Concerned
                ? new Vector3(baseAvatarScale.x * 0.98f, baseAvatarScale.y * 0.96f, baseAvatarScale.z * 0.98f)
                : baseAvatarScale;
        targetAvatarRotation = reaction switch
        {
            InterviewerReaction.Positive => Quaternion.Euler(2.5f, 0f, 0f),
            InterviewerReaction.Awkward => Quaternion.Euler(0f, 0f, 2.5f),
            InterviewerReaction.Concerned => Quaternion.Euler(-4f, 0f, 0f),
            _ => baseAvatarRotation
        };

        ApplyColor(panelRenderer, color);
        ApplyColor(reactionRenderer, Color.Lerp(color, Color.white, 0.35f));
    }

    private void CaptureBaseAvatarTransform()
    {
        if (avatarRenderer == null)
        {
            return;
        }

        baseAvatarScale = avatarRenderer.transform.localScale;
        baseAvatarRotation = avatarRenderer.transform.localRotation;
        targetAvatarScale = baseAvatarScale;
        targetAvatarRotation = baseAvatarRotation;
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
