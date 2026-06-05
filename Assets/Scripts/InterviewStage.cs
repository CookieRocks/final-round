public class InterviewStage
{
    public string StageName { get; }
    public string StageIntroText { get; }
    public string StageCompleteText { get; }
    public InterviewQuestion[] Questions { get; }

    public InterviewStage(string stageName, string stageIntroText, string stageCompleteText, InterviewQuestion[] questions)
    {
        StageName = stageName;
        StageIntroText = stageIntroText;
        StageCompleteText = stageCompleteText;
        Questions = questions;
    }
}
