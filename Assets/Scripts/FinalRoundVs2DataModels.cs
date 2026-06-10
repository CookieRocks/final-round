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

[CreateAssetMenu(fileName = "ApplicationChoiceData", menuName = "Final Round/VS2/Application Choice")]
public sealed class ApplicationChoiceData : ScriptableObject
{
    public string choiceId;
    public string label;
    [TextArea(3, 8)] public string bodyText;
    [TextArea(2, 6)] public string feedbackText;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int roleFitDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int recruiterTrustDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int candidateConfidenceDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int energyDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int overclaimRiskDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int technicalReadinessDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int rapportMomentumDelta;
}

[System.Serializable]
public sealed class RecruiterResponseChoice
{
    public string choiceId;
    public string label;
    [TextArea(2, 6)] public string responseText;
    public string nextMessageId;
}

[CreateAssetMenu(fileName = "RecruiterMessageData", menuName = "Final Round/VS2/Recruiter Message")]
public sealed class RecruiterMessageData : ScriptableObject
{
    public string messageId;
    public string senderName;
    public string subject;
    [TextArea(4, 12)] public string messageText;
    public RecruiterResponseChoice[] responseChoices;
    public string nextMessageId;
}

[System.Serializable]
public sealed class RecruiterScreenResponseChoice
{
    public string choiceId;
    public string label;
    [TextArea(2, 6)] public string responseText;
}

[CreateAssetMenu(fileName = "RecruiterScreenQuestionData", menuName = "Final Round/VS2/Recruiter Screen Question")]
public sealed class RecruiterScreenQuestionData : ScriptableObject
{
    public string questionId;
    [TextArea(3, 8)] public string prompt;
    public RecruiterScreenResponseChoice[] responseChoices;
    [TextArea(2, 6)] public string successText;
    [TextArea(2, 6)] public string concernText;
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
