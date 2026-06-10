using System.Text;
using UnityEngine;

public enum RoomIntroTone
{
    Neutral,
    Warm,
    LimitedSignal,
    DetailPressure,
    ClearMomentum
}

public enum ArchitectPressureLevel
{
    Neutral,
    Sharper
}

public readonly struct RoomModifierResult
{
    public int TechnicalStartModifier { get; }
    public int CommercialStartModifier { get; }
    public int RapportStartModifier { get; }
    public int EnergyStartModifier { get; }
    public int RecruiterWarmth { get; }
    public ArchitectPressureLevel ArchitectPressure { get; }
    public RoomIntroTone IntroTone { get; }
    public int ReactionWarmthModifier { get; }
    public string OutcomeEmailContextLine { get; }
    public string DebugSummary { get; }
    public bool IsNeutral =>
        TechnicalStartModifier == 0
        && CommercialStartModifier == 0
        && RapportStartModifier == 0
        && EnergyStartModifier == 0
        && RecruiterWarmth == 0
        && ArchitectPressure == ArchitectPressureLevel.Neutral
        && IntroTone == RoomIntroTone.Neutral
        && ReactionWarmthModifier == 0
        && string.IsNullOrWhiteSpace(OutcomeEmailContextLine);

    public RoomModifierResult(
        int technicalStartModifier,
        int commercialStartModifier,
        int rapportStartModifier,
        int energyStartModifier,
        int recruiterWarmth,
        ArchitectPressureLevel architectPressure,
        RoomIntroTone introTone,
        int reactionWarmthModifier,
        string outcomeEmailContextLine,
        string debugSummary)
    {
        TechnicalStartModifier = technicalStartModifier;
        CommercialStartModifier = commercialStartModifier;
        RapportStartModifier = rapportStartModifier;
        EnergyStartModifier = energyStartModifier;
        RecruiterWarmth = recruiterWarmth;
        ArchitectPressure = architectPressure;
        IntroTone = introTone;
        ReactionWarmthModifier = reactionWarmthModifier;
        OutcomeEmailContextLine = outcomeEmailContextLine;
        DebugSummary = debugSummary;
    }
}

public static class RoomModifierResolver
{
    public static RoomModifierResult Resolve(CandidateState state)
    {
        if (state == null)
        {
            return CreateNeutral("No active CandidateState.");
        }

        int technical = MapModifier(state.TechnicalReadiness);
        int energy = MapModifier(state.Energy);
        int rapport = Mathf.Clamp(MapModifier(state.RapportMomentum) + MapModifier(state.CandidateConfidence), -1, 1);
        int commercial = MapModifier(state.RoleFit);

        if (state.RoleFit >= 2 && state.RapportMomentum >= 1)
        {
            rapport = Mathf.Clamp(rapport + 1, -1, 2);
        }

        if (state.Energy <= -2 && state.CandidateConfidence <= -1)
        {
            energy = Mathf.Clamp(energy - 1, -2, 1);
        }

        if (state.TechnicalReadiness >= 2 && state.OverclaimRisk <= 0)
        {
            technical = Mathf.Clamp(technical + 1, -1, 2);
        }

        if (state.OverclaimRisk >= 2 && state.TechnicalReadiness <= -1)
        {
            technical = Mathf.Clamp(technical - 1, -2, 1);
        }

        int warmth = Mathf.Clamp(MapModifier(state.RecruiterTrust) + MapModifier(state.RapportMomentum), -2, 2);
        RoomIntroTone introTone = ResolveIntroTone(state);
        ArchitectPressureLevel architectPressure = state.OverclaimRisk >= 2
            ? ArchitectPressureLevel.Sharper
            : ArchitectPressureLevel.Neutral;
        int reactionWarmth = Mathf.Clamp(warmth, -1, 1);
        string contextLine = ResolveOutcomeEmailContextLine(state);

        technical = ClampStartModifier(technical);
        commercial = ClampStartModifier(commercial);
        rapport = ClampStartModifier(rapport);
        energy = ClampStartModifier(energy);

        return new RoomModifierResult(
            technical,
            commercial,
            rapport,
            energy,
            warmth,
            architectPressure,
            introTone,
            reactionWarmth,
            contextLine,
            BuildDebugSummary(state, technical, commercial, rapport, energy, warmth, architectPressure, introTone, reactionWarmth, contextLine));
    }

    private static RoomModifierResult CreateNeutral(string reason)
    {
        return new RoomModifierResult(0, 0, 0, 0, 0, ArchitectPressureLevel.Neutral, RoomIntroTone.Neutral, 0, string.Empty, reason);
    }

    private static int MapModifier(int value)
    {
        if (value >= 2)
        {
            return 1;
        }

        if (value <= -2)
        {
            return -1;
        }

        return 0;
    }

    private static int ClampStartModifier(int value)
    {
        return Mathf.Clamp(value, -2, 2);
    }

    private static RoomIntroTone ResolveIntroTone(CandidateState state)
    {
        if (state.OverclaimRisk >= 2)
        {
            return RoomIntroTone.DetailPressure;
        }

        if (state.RecruiterTrust <= -2)
        {
            return RoomIntroTone.LimitedSignal;
        }

        if (state.Energy >= 2 && state.CandidateConfidence >= 1)
        {
            return RoomIntroTone.ClearMomentum;
        }

        if (state.RecruiterTrust >= 2 || state.RapportMomentum >= 2)
        {
            return RoomIntroTone.Warm;
        }

        return RoomIntroTone.Neutral;
    }

    private static string ResolveOutcomeEmailContextLine(CandidateState state)
    {
        if (state.OverclaimRisk >= 2 && state.TechnicalReadiness <= 0)
        {
            return "The panel noted some gaps between early positioning and scenario depth.";
        }

        if (state.RecruiterTrust >= 2)
        {
            return "The early screen gave the panel a useful starting point.";
        }

        if (state.RoleFit >= 2)
        {
            return "The role alignment remained a positive signal throughout the process.";
        }

        if (state.Energy <= -2)
        {
            return "The panel felt the later stages lost some momentum.";
        }

        return string.Empty;
    }

    private static string BuildDebugSummary(
        CandidateState state,
        int technical,
        int commercial,
        int rapport,
        int energy,
        int warmth,
        ArchitectPressureLevel architectPressure,
        RoomIntroTone introTone,
        int reactionWarmth,
        string contextLine)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Start modifiers: Technical {FormatDelta(technical)}, Commercial {FormatDelta(commercial)}, Rapport {FormatDelta(rapport)}, Energy {FormatDelta(energy)}");
        builder.AppendLine($"Tone: intro {introTone}, architect pressure {architectPressure}, recruiter warmth {FormatDelta(warmth)}, reaction warmth {FormatDelta(reactionWarmth)}");
        builder.AppendLine($"Context line: {(string.IsNullOrWhiteSpace(contextLine) ? "none" : contextLine)}");
        builder.AppendLine($"Source: RoleFit {state.RoleFit}, RecruiterTrust {state.RecruiterTrust}, Confidence {state.CandidateConfidence}, Energy {state.Energy}, OverclaimRisk {state.OverclaimRisk}, TechnicalReadiness {state.TechnicalReadiness}, RapportMomentum {state.RapportMomentum}");
        return builder.ToString().TrimEnd();
    }

    private static string FormatDelta(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }
}
