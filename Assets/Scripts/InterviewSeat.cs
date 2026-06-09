using UnityEngine;

public sealed class InterviewSeat : MonoBehaviour
{
    [SerializeField] private Transform seatedCameraPoint;
    [SerializeField] private float interactionDistance = 2.2f;
    [SerializeField] private float maxLookAngle = 34f;

    public Transform SeatedCameraPoint => seatedCameraPoint;
    public float InteractionDistance => interactionDistance;

    public void Configure(Transform cameraPoint, float distance, float lookAngle)
    {
        seatedCameraPoint = cameraPoint;
        interactionDistance = distance;
        maxLookAngle = lookAngle;
    }

    public bool CanInteract(Transform player)
    {
        return CanInteract(player, player);
    }

    public bool CanInteract(Transform player, Transform view)
    {
        return player != null
            && IsCloseEnough(player)
            && IsLookingAtChair(view);
    }

    private bool IsCloseEnough(Transform player)
    {
        return Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    private bool IsLookingAtChair(Transform view)
    {
        if (view == null)
        {
            return true;
        }

        Vector3 toChair = transform.position - view.position;
        if (toChair.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        return Vector3.Angle(view.forward, toChair.normalized) <= maxLookAngle;
    }
}
