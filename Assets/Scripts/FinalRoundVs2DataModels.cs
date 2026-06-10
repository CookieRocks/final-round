using UnityEngine;

[CreateAssetMenu(fileName = "JobListingData", menuName = "Final Round/VS2/Job Listing")]
public sealed class JobListingData : ScriptableObject
{
    public string jobId;
    public string companyName;
    public string roleTitle;
    [TextArea(3, 8)] public string summary;
    [TextArea(2, 8)] public string[] responsibilities;
    [TextArea(2, 8)] public string[] requirements;
    [TextArea(2, 8)] public string[] niceToHaves;
    public string salaryRange;
    [TextArea(2, 6)] public string processNotes;
    [TextArea(2, 6)] public string[] redFlags;
    [TextArea(2, 6)] public string[] greenFlags;
    public string defaultRoomProfileId;
}

[CreateAssetMenu(fileName = "RoomModifierData", menuName = "Final Round/VS2/Room Modifier")]
public sealed class RoomModifierData : ScriptableObject
{
    public int technicalStartModifier;
    public int commercialStartModifier;
    public int rapportStartModifier;
    public int energyStartModifier;
    public string stageIntroVariant;
    public string architectPressureVariant;
    public string outcomeEmailVariant;
    public int reactionWarmthModifier;
    [TextArea(2, 8)] public string debugSummary;
}
