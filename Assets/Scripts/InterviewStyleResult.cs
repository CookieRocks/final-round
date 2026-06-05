public class InterviewStyleResult
{
    public string StyleName { get; }
    public string SummaryText { get; }

    public InterviewStyleResult(string styleName, string summaryText)
    {
        StyleName = styleName;
        SummaryText = summaryText;
    }
}
