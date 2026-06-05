public class InterviewQuestion
{
    public string QuestionText { get; }
    public AnswerOption[] Answers { get; }

    public InterviewQuestion(string questionText, AnswerOption[] answers)
    {
        QuestionText = questionText;
        Answers = answers;
    }
}
