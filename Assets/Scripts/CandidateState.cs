using System;
using UnityEngine;

[Serializable]
public sealed class CandidateState
{
    public const int MinPrototypeModifier = -3;
    public const int MaxPrototypeModifier = 3;

    [SerializeField] private string selectedJobId;
    [SerializeField] private string jobDefaultDeltasAppliedJobId;
    [SerializeField] private string applicationChoiceId;
    [SerializeField] private string recruiterPathId;
    [SerializeField] private string recruiterResponseIds;
    [SerializeField] private string roomOutcome;
    [SerializeField] private string roomModifierSummary;
    [SerializeField] private bool hasActiveDeskRun;
    [SerializeField] private bool aftermathAvailable;
    [SerializeField] private bool aftermathCompleted;
    [SerializeField] private int roleFit;
    [SerializeField] private int recruiterTrust;
    [SerializeField] private int candidateConfidence;
    [SerializeField] private int energy;
    [SerializeField] private int overclaimRisk;
    [SerializeField] private int technicalReadiness;
    [SerializeField] private int rapportMomentum;

    public string SelectedJobId
    {
        get => selectedJobId;
        set => selectedJobId = value;
    }

    public string JobDefaultDeltasAppliedJobId
    {
        get => jobDefaultDeltasAppliedJobId;
        set => jobDefaultDeltasAppliedJobId = value;
    }

    public string ApplicationChoiceId
    {
        get => applicationChoiceId;
        set => applicationChoiceId = value;
    }

    public string RecruiterPathId
    {
        get => recruiterPathId;
        set => recruiterPathId = value;
    }

    public string RecruiterResponseIds
    {
        get => recruiterResponseIds;
        set => recruiterResponseIds = value;
    }

    public string RoomOutcome
    {
        get => roomOutcome;
        set => roomOutcome = value;
    }

    public string RoomModifierSummary
    {
        get => roomModifierSummary;
        set => roomModifierSummary = value;
    }

    public bool HasActiveDeskRun
    {
        get => hasActiveDeskRun;
        set => hasActiveDeskRun = value;
    }

    public bool AftermathAvailable
    {
        get => aftermathAvailable;
        set => aftermathAvailable = value;
    }

    public bool AftermathCompleted
    {
        get => aftermathCompleted;
        set => aftermathCompleted = value;
    }

    public int RoleFit
    {
        get => roleFit;
        set => roleFit = ClampModifier(value);
    }

    public int RecruiterTrust
    {
        get => recruiterTrust;
        set => recruiterTrust = ClampModifier(value);
    }

    public int CandidateConfidence
    {
        get => candidateConfidence;
        set => candidateConfidence = ClampModifier(value);
    }

    public int Energy
    {
        get => energy;
        set => energy = ClampModifier(value);
    }

    public int OverclaimRisk
    {
        get => overclaimRisk;
        set => overclaimRisk = ClampModifier(value);
    }

    public int TechnicalReadiness
    {
        get => technicalReadiness;
        set => technicalReadiness = ClampModifier(value);
    }

    public int RapportMomentum
    {
        get => rapportMomentum;
        set => rapportMomentum = ClampModifier(value);
    }

    public static CandidateState CreateNeutral(bool activeDeskRun)
    {
        return new CandidateState
        {
            hasActiveDeskRun = activeDeskRun,
            selectedJobId = string.Empty,
            jobDefaultDeltasAppliedJobId = string.Empty,
            applicationChoiceId = string.Empty,
            recruiterPathId = string.Empty,
            recruiterResponseIds = string.Empty,
            roomOutcome = string.Empty,
            roomModifierSummary = string.Empty,
            aftermathAvailable = false,
            aftermathCompleted = false
        };
    }

    public void ApplyDeltas(
        int roleFitDelta,
        int recruiterTrustDelta,
        int candidateConfidenceDelta,
        int energyDelta,
        int overclaimRiskDelta,
        int technicalReadinessDelta,
        int rapportMomentumDelta)
    {
        RoleFit += roleFitDelta;
        RecruiterTrust += recruiterTrustDelta;
        CandidateConfidence += candidateConfidenceDelta;
        Energy += energyDelta;
        OverclaimRisk += overclaimRiskDelta;
        TechnicalReadiness += technicalReadinessDelta;
        RapportMomentum += rapportMomentumDelta;
    }

    public string BuildDebugSummary()
    {
        return
            $"Active Desk Run: {hasActiveDeskRun}\n" +
            $"Selected Job: {FormatId(selectedJobId)}\n" +
            $"Job Defaults Applied: {FormatId(jobDefaultDeltasAppliedJobId)}\n" +
            $"Application Choice: {FormatId(applicationChoiceId)}\n" +
            $"Recruiter Path: {FormatId(recruiterPathId)}\n" +
            $"Recruiter Responses: {FormatId(recruiterResponseIds)}\n" +
            $"Room Outcome: {FormatId(roomOutcome)}\n" +
            $"Room Modifiers: {FormatId(roomModifierSummary)}\n" +
            $"Aftermath Available: {aftermathAvailable}, Aftermath Completed: {aftermathCompleted}\n" +
            $"Role Fit: {roleFit}, Recruiter Trust: {recruiterTrust}, Candidate Confidence: {candidateConfidence}, Energy: {energy}\n" +
            $"Overclaim Risk: {overclaimRisk}, Technical Readiness: {technicalReadiness}, Rapport Momentum: {rapportMomentum}";
    }

    private static int ClampModifier(int value)
    {
        return Mathf.Clamp(value, MinPrototypeModifier, MaxPrototypeModifier);
    }

    private static string FormatId(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "none" : value;
    }
}
