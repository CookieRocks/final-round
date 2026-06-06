public class CompanyProfile
{
    public string CompanyName { get; }
    public string ProfileName { get; }
    public string Description { get; }
    public string StatModifierNotes { get; }
    public string PreferredStyle { get; }
    public string RuleName { get; }
    public string RuleDescription { get; }
    public string RuleHint { get; }
    public float RandomEventChanceModifier { get; }
    public int EventEnergyLossModifier { get; }
    public int BetweenStageEnergyRecoveryModifier { get; }
    public int AiChaoticStyleBonus { get; }
    public int OfferRecommendedThresholdModifier { get; }
    public int RecoveryPositiveEffectBonus { get; }
    public int ChaoticStyleForgiveness { get; }
    public int FirstChaoticAnswerConfidenceBonus { get; }
    public bool RequiresTechnicalAndCommercialOfferGate { get; }

    public CompanyProfile(
        string companyName,
        string profileName,
        string description,
        string statModifierNotes,
        string preferredStyle,
        string ruleName,
        string ruleDescription,
        string ruleHint,
        float randomEventChanceModifier = 0f,
        int eventEnergyLossModifier = 0,
        int betweenStageEnergyRecoveryModifier = 0,
        int aiChaoticStyleBonus = 0,
        int offerRecommendedThresholdModifier = 0,
        int recoveryPositiveEffectBonus = 0,
        int chaoticStyleForgiveness = 0,
        int firstChaoticAnswerConfidenceBonus = 0,
        bool requiresTechnicalAndCommercialOfferGate = false)
    {
        CompanyName = companyName;
        ProfileName = profileName;
        Description = description;
        StatModifierNotes = statModifierNotes;
        PreferredStyle = preferredStyle;
        RuleName = ruleName;
        RuleDescription = ruleDescription;
        RuleHint = ruleHint;
        RandomEventChanceModifier = randomEventChanceModifier;
        EventEnergyLossModifier = eventEnergyLossModifier;
        BetweenStageEnergyRecoveryModifier = betweenStageEnergyRecoveryModifier;
        AiChaoticStyleBonus = aiChaoticStyleBonus;
        OfferRecommendedThresholdModifier = offerRecommendedThresholdModifier;
        RecoveryPositiveEffectBonus = recoveryPositiveEffectBonus;
        ChaoticStyleForgiveness = chaoticStyleForgiveness;
        FirstChaoticAnswerConfidenceBonus = firstChaoticAnswerConfidenceBonus;
        RequiresTechnicalAndCommercialOfferGate = requiresTechnicalAndCommercialOfferGate;
    }

    public string GetRuleModifierSummary()
    {
        return
            $"eventChance {RandomEventChanceModifier:+0.00;-0.00;0.00}, " +
            $"eventEnergyLoss {EventEnergyLossModifier:+0;-0;0}, " +
            $"betweenStageEnergy {BetweenStageEnergyRecoveryModifier:+0;-0;0}, " +
            $"offerThreshold {OfferRecommendedThresholdModifier:+0;-0;0}, " +
            $"recoveryPositiveBonus {RecoveryPositiveEffectBonus:+0;-0;0}, " +
            $"chaosForgiveness {ChaoticStyleForgiveness}, " +
            $"firstChaosConfidence {FirstChaoticAnswerConfidenceBonus:+0;-0;0}, " +
            $"techCommercialGate {RequiresTechnicalAndCommercialOfferGate}";
    }
}
