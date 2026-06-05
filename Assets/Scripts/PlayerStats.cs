using UnityEngine;

public class PlayerStats
{
    public int Confidence { get; private set; }
    public int Energy { get; private set; }
    public int TechnicalCredibility { get; private set; }
    public int CommercialAlignment { get; private set; }
    public int TotalScore => Confidence + Energy + TechnicalCredibility + CommercialAlignment;
    public bool HasCriticalWeakness => Confidence < 20 || Energy < 20 || TechnicalCredibility < 20 || CommercialAlignment < 20;

    public void Reset(int confidence, int energy, int technicalCredibility, int commercialAlignment)
    {
        Confidence = Mathf.Clamp(confidence, 0, 100);
        Energy = Mathf.Clamp(energy, 0, 100);
        TechnicalCredibility = Mathf.Clamp(technicalCredibility, 0, 100);
        CommercialAlignment = Mathf.Clamp(commercialAlignment, 0, 100);
    }

    public PlayerStats Copy()
    {
        PlayerStats copy = new PlayerStats();
        copy.Reset(Confidence, Energy, TechnicalCredibility, CommercialAlignment);
        return copy;
    }

    public void Apply(AnswerOption answer)
    {
        ApplyChanges(answer.ConfidenceChange, answer.EnergyChange, answer.TechnicalCredibilityChange, answer.CommercialAlignmentChange);
    }

    public void Apply(RandomInterviewEvent interviewEvent)
    {
        ApplyChanges(interviewEvent.ConfidenceChange, interviewEvent.EnergyChange, interviewEvent.TechnicalCredibilityChange, interviewEvent.CommercialAlignmentChange);
    }

    public void RecoverBetweenStages(bool stageWasStrong)
    {
        Energy = Mathf.Clamp(Energy + 10, 0, 100);

        if (stageWasStrong)
        {
            Confidence = Mathf.Clamp(Confidence + 5, 0, 100);
        }
    }

    public string GetSummary()
    {
        return
            $"Confidence: {Confidence}/100\n" +
            $"Energy: {Energy}/100\n" +
            $"Technical Credibility: {TechnicalCredibility}/100\n" +
            $"Commercial Alignment: {CommercialAlignment}/100";
    }

    public bool IsBalanced()
    {
        int highestStat = Mathf.Max(Confidence, Energy, TechnicalCredibility, CommercialAlignment);
        int lowestStat = Mathf.Min(Confidence, Energy, TechnicalCredibility, CommercialAlignment);
        return highestStat - lowestStat <= 25;
    }

    private void ApplyChanges(int confidenceChange, int energyChange, int technicalCredibilityChange, int commercialAlignmentChange)
    {
        Confidence = Mathf.Clamp(Confidence + confidenceChange, 0, 100);
        Energy = Mathf.Clamp(Energy + energyChange, 0, 100);
        TechnicalCredibility = Mathf.Clamp(TechnicalCredibility + technicalCredibilityChange, 0, 100);
        CommercialAlignment = Mathf.Clamp(CommercialAlignment + commercialAlignmentChange, 0, 100);
    }
}
