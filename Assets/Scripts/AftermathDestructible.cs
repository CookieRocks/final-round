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

    public string ObjectId => objectId;
    public string DisplayLabel => displayLabel;
    public int CatharsisValue => catharsisValue;
    public bool HasBeenDestroyed => hasBeenDestroyed;

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

        SetRootActive(intactRoot, true);
        SetRootActive(brokenRoot, false);
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
        }
        else
        {
            transform.localScale *= 0.72f;
        }

        feedbackText = string.IsNullOrWhiteSpace(onDestroyedText)
            ? $"{displayLabel} is processed."
            : onDestroyedText;
        return true;
    }

    private static void SetRootActive(GameObject root, bool active)
    {
        if (root != null)
        {
            root.SetActive(active);
        }
    }
}
