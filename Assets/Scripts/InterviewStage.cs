public class InterviewStage
{
    public string StageName { get; }
    public string StageIntroText { get; }
    public string StageCompleteText { get; }
    public InterviewQuestion[] QuestionPool { get; }
    public InterviewQuestion[] Questions => QuestionPool;
    public int QuestionsToAskThisRun { get; }

    public InterviewStage(string stageName, string stageIntroText, string stageCompleteText, InterviewQuestion[] questionPool, int questionsToAskThisRun)
    {
        StageName = stageName;
        StageIntroText = stageIntroText;
        StageCompleteText = stageCompleteText;
        QuestionPool = questionPool;
        QuestionsToAskThisRun = questionsToAskThisRun;
    }
}
