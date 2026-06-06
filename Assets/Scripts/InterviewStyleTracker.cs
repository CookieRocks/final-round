public class InterviewStyleTracker
{
    private int diplomaticStyle;
    private int bluntStyle;
    private int commercialStyle;
    private int technicalStyle;
    private int chaoticStyle;
    private int burnedOutStyle;

    public int ChaoticStyle => chaoticStyle;

    public void Reset()
    {
        diplomaticStyle = 0;
        bluntStyle = 0;
        commercialStyle = 0;
        technicalStyle = 0;
        chaoticStyle = 0;
        burnedOutStyle = 0;
    }

    public void Apply(AnswerOption answer)
    {
        diplomaticStyle += answer.DiplomaticStyleChange;
        bluntStyle += answer.BluntStyleChange;
        commercialStyle += answer.CommercialStyleChange;
        technicalStyle += answer.TechnicalStyleChange;
        chaoticStyle += answer.ChaoticStyleChange;
        burnedOutStyle += answer.BurnedOutStyleChange;
    }

    public void Apply(RandomInterviewEvent interviewEvent)
    {
        diplomaticStyle += interviewEvent.DiplomaticStyleChange;
        bluntStyle += interviewEvent.BluntStyleChange;
        commercialStyle += interviewEvent.CommercialStyleChange;
        technicalStyle += interviewEvent.TechnicalStyleChange;
        chaoticStyle += interviewEvent.ChaoticStyleChange;
        burnedOutStyle += interviewEvent.BurnedOutStyleChange;
    }

    public void AddChaoticStyle(int change)
    {
        chaoticStyle += change;
    }

    public void ApplyStyleChanges(
        int diplomaticChange,
        int bluntChange,
        int commercialChange,
        int technicalChange,
        int chaoticChange,
        int burnedOutChange)
    {
        diplomaticStyle += diplomaticChange;
        bluntStyle += bluntChange;
        commercialStyle += commercialChange;
        technicalStyle += technicalChange;
        chaoticStyle += chaoticChange;
        burnedOutStyle += burnedOutChange;
    }

    public string GetDebugSummary()
    {
        return
            "Style Counters:\n" +
            $"Diplomatic: {diplomaticStyle}\n" +
            $"Blunt: {bluntStyle}\n" +
            $"Commercial: {commercialStyle}\n" +
            $"Technical: {technicalStyle}\n" +
            $"Chaotic: {chaoticStyle}\n" +
            $"BurnedOut: {burnedOutStyle}";
    }

    public InterviewStyleResult DetermineDominantStyle(PlayerStats stats)
    {
        if (burnedOutStyle >= 4 || burnedOutStyle + chaoticStyle >= 8)
        {
            return new InterviewStyleResult(
                "Burnout Goblin",
                "Dominant style: Burnout Goblin. You kept showing up, but the process had the energy of answering Slack messages from inside a cupboard. Funny, human, and occasionally effective, but the panel could feel the battery warning.");
        }

        if (technicalStyle >= 5 && chaoticStyle >= 3)
        {
            return new InterviewStyleResult(
                "Demo Gremlin",
                "Dominant style: Demo Gremlin. You can absolutely find the technical truth, though sometimes by sprinting through the product with all the tabs open. The panel saw capability, plus a faint risk of live-demo weather.");
        }

        if (commercialStyle >= 5 && diplomaticStyle >= 4)
        {
            return new InterviewStyleResult(
                "Boardroom Translator",
                "Dominant style: Boardroom Translator. You made technical ideas sound like business decisions without sanding off the useful detail. This is the version of you that gets quoted in the debrief.");
        }

        if (technicalStyle >= 5 && commercialStyle <= 2)
        {
            return new InterviewStyleResult(
                "Technical Purist",
                "Dominant style: Technical Purist. You protected accuracy like it owed you money. The panel trusted your depth, though a few answers needed a clearer bridge back to why the buyer should care.");
        }

        if (commercialStyle >= 5 && stats.Confidence >= 75)
        {
            return new InterviewStyleResult(
                "AE Whisperer",
                "Dominant style: AE Whisperer. You kept translating messy deal energy into next steps, mutual value, and fewer surprise explosions. Somewhere, an account executive just relaxed their shoulders.");
        }

        if (bluntStyle >= 5)
        {
            return new InterviewStyleResult(
                "Direct But Spiky",
                "Dominant style: Direct But Spiky. Nobody left the process wondering what you meant, which is useful. A few answers just needed a little more padding before they hit the table.");
        }

        if (stats.IsBalanced() && chaoticStyle <= 2)
        {
            return new InterviewStyleResult(
                "Safe Pair of Hands",
                "Dominant style: Safe Pair of Hands. Not every answer tried to win the internet, and that is a strength. You came across steady, credible, and unlikely to turn a customer call into a group therapy session.");
        }

        if (technicalStyle >= commercialStyle)
        {
            return new InterviewStyleResult(
                "Technical Purist",
                "Dominant style: Technical Purist. Your strongest signal was technical credibility. With a little more commercial framing, this turns from solid interview into very hard-to-ignore candidate.");
        }

        return new InterviewStyleResult(
            "Safe Pair of Hands",
            "Dominant style: Safe Pair of Hands. Your run was measured and broadly credible. The panel may not be writing poetry about it, which is fine, because hiring debriefs should ideally contain fewer poems.");
    }
}
