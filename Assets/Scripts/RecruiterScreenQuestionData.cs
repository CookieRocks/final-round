using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RecruiterScreenQuestionData", menuName = "Final Round/VS2/Recruiter Screen Question")]
public sealed class RecruiterScreenQuestionData : ScriptableObject
{
    public string questionId;
    [TextArea(3, 8)] public string prompt;
    public RecruiterScreenResponseChoice[] responseChoices;
    [TextArea(2, 6)] public string successText;
    [TextArea(2, 6)] public string concernText;
}

[Serializable]
public sealed class RecruiterScreenResponseChoice
{
    public string choiceId;
    public string label;
    [TextArea(2, 6)] public string responseText;
    [TextArea(2, 6)] public string feedbackText;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int roleFitDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int recruiterTrustDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int candidateConfidenceDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int energyDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int overclaimRiskDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int technicalReadinessDelta;
    [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int rapportMomentumDelta;
}
