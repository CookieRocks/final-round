using UnityEngine;

public enum InterviewerReaction
{
    Neutral,
    Listening,
    Positive,
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
    private Color concernedColor = new Color32(100, 64, 70, 255);
    private InterviewerReaction currentReaction = InterviewerReaction.Neutral;

    private void Update()
    {
        if (currentReaction != InterviewerReaction.Listening || avatarRenderer == null)
        {
            return;
        }

        float nod = Mathf.Sin(Time.time * 2.1f + transform.GetSiblingIndex()) * 1.4f;
        avatarRenderer.transform.localRotation = Quaternion.Euler(nod, 0f, 0f);
    }

    public void Configure(Renderer panel, Renderer avatar, Renderer reaction)
    {
        panelRenderer = panel;
        avatarRenderer = avatar;
        reactionRenderer = reaction;
        SetReaction(InterviewerReaction.Neutral);
    }

    public void SetReaction(InterviewerReaction reaction)
    {
        currentReaction = reaction;
        Color color = reaction switch
        {
            InterviewerReaction.Listening => listeningColor,
            InterviewerReaction.Positive => positiveColor,
            InterviewerReaction.Concerned => concernedColor,
            _ => neutralColor
        };

        ApplyColor(panelRenderer, color);
        ApplyColor(reactionRenderer, Color.Lerp(color, Color.white, 0.35f));
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
