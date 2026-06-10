using UnityEngine;

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
