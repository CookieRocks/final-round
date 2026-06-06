public class CompanyProfile
{
    public string CompanyName { get; }
    public string ProfileName { get; }
    public string Description { get; }
    public string StatModifierNotes { get; }
    public string PreferredStyle { get; }
    public float RandomEventChanceModifier { get; }
    public int EventEnergyLossModifier { get; }
    public int BetweenStageEnergyRecoveryModifier { get; }
    public int AiChaoticStyleBonus { get; }

    public CompanyProfile(
        string companyName,
        string profileName,
        string description,
        string statModifierNotes,
        string preferredStyle,
        float randomEventChanceModifier = 0f,
        int eventEnergyLossModifier = 0,
        int betweenStageEnergyRecoveryModifier = 0,
        int aiChaoticStyleBonus = 0)
    {
        CompanyName = companyName;
        ProfileName = profileName;
        Description = description;
        StatModifierNotes = statModifierNotes;
        PreferredStyle = preferredStyle;
        RandomEventChanceModifier = randomEventChanceModifier;
        EventEnergyLossModifier = eventEnergyLossModifier;
        BetweenStageEnergyRecoveryModifier = betweenStageEnergyRecoveryModifier;
        AiChaoticStyleBonus = aiChaoticStyleBonus;
    }
}
