using UnityEngine;

public enum InterviewOutcomeType
{
    StrongPass,
    Pass,
    Hold,
    Reject
}

public enum OutcomeScoreDimension
{
    Technical,
    Commercial,
    Rapport,
    Energy
}

public readonly struct OutcomeScoreSnapshot
{
    public int Technical { get; }
    public int Commercial { get; }
    public int Rapport { get; }
    public int Energy { get; }
    public int Total => Technical + Commercial + Rapport + Energy;

    public OutcomeScoreSnapshot(int technical, int commercial, int rapport, int energy)
    {
        Technical = technical;
        Commercial = commercial;
        Rapport = rapport;
        Energy = energy;
    }
}

public readonly struct OutcomeEmail
{
    public string FromLine { get; }
    public string SubjectLine { get; }
    public string OpeningLine { get; }
    public string OutcomeParagraph { get; }
    public string FeedbackParagraph { get; }

    public OutcomeEmail(
        string fromLine,
        string subjectLine,
        string openingLine,
        string outcomeParagraph,
        string feedbackParagraph)
    {
        FromLine = fromLine;
        SubjectLine = subjectLine;
        OpeningLine = openingLine;
        OutcomeParagraph = outcomeParagraph;
        FeedbackParagraph = feedbackParagraph;
    }
}

public static class OutcomeEmailGenerator
{
    public static OutcomeEmail Generate(InterviewOutcomeType outcome, OutcomeScoreSnapshot score)
    {
        string subject = outcome switch
        {
            InterviewOutcomeType.StrongPass => "Final Round Feedback",
            InterviewOutcomeType.Pass => "Interview Follow-Up",
            InterviewOutcomeType.Hold => "Final Round Update",
            _ => "Final Round Outcome"
        };

        string opening = outcome switch
        {
            InterviewOutcomeType.Reject => "Hi,\n\nThank you again for taking the time to meet with the team.",
            InterviewOutcomeType.Hold => "Hi,\n\nThank you for the conversation today. We appreciate the time and preparation.",
            _ => "Hi,\n\nThank you for speaking with the panel today."
        };

        string outcomeParagraph = outcome switch
        {
            InterviewOutcomeType.StrongPass =>
                "The panel was aligned in debrief. You handled both the technical pressure and the commercial context well, and the team felt there was a clear path to putting you in front of customers.",
            InterviewOutcomeType.Pass =>
                "The feedback was positive overall. There are a few areas the team would want to calibrate, but the signal from the final round was strong enough to continue the process.",
            InterviewOutcomeType.Hold =>
                "We are still aligning internally and will come back to you once we have completed the process. At this stage, feedback is not negative, but it is not fully settled.",
            _ =>
                "After debrief, we have decided not to move forward for this role. The team appreciated the discussion, but felt the final round did not give enough consistent signal for this particular position."
        };

        return new OutcomeEmail(
            "From: recruitment@northbridge-cyber.example",
            $"Subject: {subject}",
            opening,
            outcomeParagraph,
            BuildFeedbackParagraph(score));
    }

    public static OutcomeScoreDimension GetWeakestDimension(OutcomeScoreSnapshot score)
    {
        OutcomeScoreDimension weakest = OutcomeScoreDimension.Technical;
        int weakestValue = score.Technical;

        PickLower(score.Commercial, OutcomeScoreDimension.Commercial, ref weakestValue, ref weakest);
        PickLower(score.Rapport, OutcomeScoreDimension.Rapport, ref weakestValue, ref weakest);
        PickLower(score.Energy, OutcomeScoreDimension.Energy, ref weakestValue, ref weakest);
        return weakest;
    }

    public static bool HasAllStrongScores(OutcomeScoreSnapshot score)
    {
        return score.Technical >= 7
            && score.Commercial >= 7
            && score.Rapport >= 7
            && score.Energy >= 7;
    }

    private static string BuildFeedbackParagraph(OutcomeScoreSnapshot score)
    {
        if (HasAllStrongScores(score))
        {
            return "Feedback was balanced across the scorecard. The panel noted credible technical depth, clear business framing, and a calm presence in the room.";
        }

        return GetWeakestDimension(score) switch
        {
            OutcomeScoreDimension.Technical =>
                "The main reservation was technical depth. The panel wanted stronger evidence that you could handle a skeptical security audience without leaning too heavily on general positioning.",
            OutcomeScoreDimension.Commercial =>
                "The main reservation was commercial impact. The panel wanted a clearer line from security capability to business risk, buying urgency, and executive value.",
            OutcomeScoreDimension.Rapport =>
                "The main reservation was stakeholder engagement. The panel saw credible moments, but wanted more confidence that tense customer rooms would feel managed rather than merely answered.",
            _ =>
                "The main reservation was confidence and presence. The panel wanted more energy in the delivery, especially when the conversation became ambiguous."
        };
    }

    private static void PickLower(
        int candidateValue,
        OutcomeScoreDimension candidate,
        ref int weakestValue,
        ref OutcomeScoreDimension weakest)
    {
        if (candidateValue < weakestValue)
        {
            weakestValue = candidateValue;
            weakest = candidate;
        }
    }
}
