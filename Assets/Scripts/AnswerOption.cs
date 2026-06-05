public class AnswerOption
{
    public string AnswerText { get; }
    public string ConsequenceText { get; }
    public int ConfidenceChange { get; }
    public int EnergyChange { get; }
    public int TechnicalCredibilityChange { get; }
    public int CommercialAlignmentChange { get; }
    public int DiplomaticStyleChange { get; }
    public int BluntStyleChange { get; }
    public int CommercialStyleChange { get; }
    public int TechnicalStyleChange { get; }
    public int ChaoticStyleChange { get; }
    public int BurnedOutStyleChange { get; }

    public AnswerOption(
        string answerText,
        string consequenceText,
        int confidenceChange,
        int energyChange,
        int technicalCredibilityChange,
        int commercialAlignmentChange,
        int diplomaticStyleChange = 0,
        int bluntStyleChange = 0,
        int commercialStyleChange = 0,
        int technicalStyleChange = 0,
        int chaoticStyleChange = 0,
        int burnedOutStyleChange = 0)
    {
        AnswerText = answerText;
        ConsequenceText = consequenceText;
        ConfidenceChange = confidenceChange;
        EnergyChange = energyChange;
        TechnicalCredibilityChange = technicalCredibilityChange;
        CommercialAlignmentChange = commercialAlignmentChange;
        DiplomaticStyleChange = diplomaticStyleChange;
        BluntStyleChange = bluntStyleChange;
        CommercialStyleChange = commercialStyleChange;
        TechnicalStyleChange = technicalStyleChange;
        ChaoticStyleChange = chaoticStyleChange;
        BurnedOutStyleChange = burnedOutStyleChange;
    }
}
