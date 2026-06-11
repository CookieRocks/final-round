using UnityEngine;

[CreateAssetMenu(fileName = "JobListingData", menuName = "Final Round/VS2/Job Listing")]
public sealed class JobListingData : ScriptableObject
{
    public string jobId;
    public string companyName;
    public string roleTitle;
    [TextArea(2, 5)] public string jobCardSummary;
    public string difficultyProfile;
    public string recruiterName;
    public string recruiterTitle;
    public string recruiterCompany;
    [TextArea(3, 8)] public string summary;
    [TextArea(2, 8)] public string[] responsibilities;
    [TextArea(2, 8)] public string[] requirements;
    [TextArea(2, 8)] public string[] niceToHaves;
    public string salaryRange;
    [TextArea(2, 6)] public string processNotes;
    [TextArea(2, 6)] public string[] redFlags;
    [TextArea(2, 6)] public string[] greenFlags;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultRoleFitDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultRecruiterTrustDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultCandidateConfidenceDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultEnergyDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultOverclaimRiskDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultTechnicalReadinessDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int defaultRapportMomentumDelta;
    public string defaultRoomProfileId;
    [TextArea(2, 5)] public string roomContextLine;
    [TextArea(2, 5)] public string outcomeContextLine;
    [TextArea(2, 5)] public string processSummaryNote;
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
