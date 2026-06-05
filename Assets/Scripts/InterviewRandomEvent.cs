public class RandomInterviewEvent
{
    public string EventTitle { get; }
    public string EventDescription { get; }
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

    public RandomInterviewEvent(
        string eventTitle,
        string eventDescription,
        int confidenceChange = 0,
        int energyChange = 0,
        int technicalCredibilityChange = 0,
        int commercialAlignmentChange = 0,
        int diplomaticStyleChange = 0,
        int bluntStyleChange = 0,
        int commercialStyleChange = 0,
        int technicalStyleChange = 0,
        int chaoticStyleChange = 0,
        int burnedOutStyleChange = 0)
    {
        EventTitle = eventTitle;
        EventDescription = eventDescription;
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
