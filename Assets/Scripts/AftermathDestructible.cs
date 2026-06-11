using System.Collections;
using UnityEngine;

public sealed class AftermathDestructible : MonoBehaviour
{
    [SerializeField] private string objectId;
    [SerializeField] private string displayLabel;
    [SerializeField] private int hitPoints = 1;
    [SerializeField] private int catharsisValue = 10;
    [SerializeField] private GameObject intactRoot;
    [SerializeField] private GameObject brokenRoot;
    [SerializeField] private string onHitText;
    [SerializeField] private string onDestroyedText;
    [SerializeField] private bool canBeDestroyed = true;
    [SerializeField] private bool hasBeenDestroyed;

    private bool highlighted;
    private float punchMultiplier = 1f;
    private Vector3 baseLocalScale = Vector3.one;
    private Coroutine punchRoutine;

    public string ObjectId => objectId;
    public string DisplayLabel => displayLabel;
    public int CatharsisValue => catharsisValue;
    public bool HasBeenDestroyed => hasBeenDestroyed;
    public Vector3 EffectPosition => transform.position;

    public void Configure(
        string id,
        string label,
        int startingHitPoints,
        int value,
        GameObject intact,
        GameObject broken,
        string hitText,
        string destroyedText,
        bool destroyable = true)
    {
        objectId = id;
        displayLabel = label;
        hitPoints = Mathf.Max(1, startingHitPoints);
        catharsisValue = Mathf.Max(0, value);
        intactRoot = intact;
        brokenRoot = broken;
        onHitText = hitText;
        onDestroyedText = destroyedText;
        canBeDestroyed = destroyable;
        hasBeenDestroyed = false;
        highlighted = false;
        punchMultiplier = 1f;
        baseLocalScale = transform.localScale;

        SetRootActive(intactRoot, true);
        SetRootActive(brokenRoot, false);
        ApplyVisualScale();
    }

    public bool TryProcessHit(out string feedbackText, out bool destroyedThisHit)
    {
        destroyedThisHit = false;
        feedbackText = string.Empty;

        if (!canBeDestroyed || hasBeenDestroyed)
        {
            return false;
        }

        hitPoints = Mathf.Max(0, hitPoints - 1);
        if (hitPoints > 0)
        {
            PlayHitReaction(1.08f, 0.16f);
            feedbackText = string.IsNullOrWhiteSpace(onHitText)
                ? $"{displayLabel} shifts."
                : onHitText;
            return true;
        }

        hasBeenDestroyed = true;
        destroyedThisHit = true;
        SetRootActive(intactRoot, false);

        if (brokenRoot != null)
        {
            SetRootActive(brokenRoot, true);
            brokenRoot.transform.localRotation = Quaternion.Euler(0f, 0f, GetProcessedTilt());
        }
        else
        {
            transform.localScale *= 0.72f;
        }

        highlighted = false;
        PlayHitReaction(1.14f, 0.2f);
        feedbackText = string.IsNullOrWhiteSpace(onDestroyedText)
            ? $"{displayLabel} is processed."
            : onDestroyedText;
        return true;
    }

    public void SetHighlighted(bool value)
    {
        if (hasBeenDestroyed || highlighted == value)
        {
            return;
        }

        highlighted = value;
        ApplyVisualScale();
    }

    public string GetProcessLabel()
    {
        return $"Process: {displayLabel}";
    }

    private void PlayHitReaction(float scaleMultiplier, float duration)
    {
        if (punchRoutine != null)
        {
            StopCoroutine(punchRoutine);
        }

        punchRoutine = StartCoroutine(PunchRoutine(scaleMultiplier, duration));
    }

    private IEnumerator PunchRoutine(float scaleMultiplier, float duration)
    {
        punchMultiplier = scaleMultiplier;
        ApplyVisualScale();
        yield return new WaitForSeconds(duration);

        punchMultiplier = 1f;
        ApplyVisualScale();
        punchRoutine = null;
    }

    private void ApplyVisualScale()
    {
        float highlightMultiplier = highlighted ? 1.045f : 1f;
        transform.localScale = baseLocalScale * highlightMultiplier * punchMultiplier;
    }

    private float GetProcessedTilt()
    {
        int hash = string.IsNullOrEmpty(objectId) ? displayLabel.GetHashCode() : objectId.GetHashCode();
        return hash % 2 == 0 ? -9f : 9f;
    }

    private static void SetRootActive(GameObject root, bool active)
    {
        if (root != null)
        {
            root.SetActive(active);
        }
    }
}
