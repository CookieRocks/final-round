using UnityEngine;

[CreateAssetMenu(fileName = "InterviewQuestion", menuName = "Final Round/Interview Question")]
public sealed class InterviewQuestionData : ScriptableObject
{
    [SerializeField] private string questionId;
    [SerializeField] private QuestionCategory category;
    [SerializeField] private string speakerName;
    [TextArea(3, 8)]
    [SerializeField] private string questionText;
    [SerializeField] private AnswerOptionData[] answerOptions = new AnswerOptionData[4];

    public string QuestionId => questionId;
    public QuestionCategory Category => category;
    public string SpeakerName => speakerName;
    public string QuestionText => questionText;
    public AnswerOptionData[] AnswerOptions => answerOptions;

    public bool IsValid(out string validationError)
    {
        if (string.IsNullOrWhiteSpace(questionId))
        {
            validationError = "question ID is empty";
            return false;
        }

        if (string.IsNullOrWhiteSpace(speakerName))
        {
            validationError = $"question '{questionId}' has no speaker name";
            return false;
        }

        if (string.IsNullOrWhiteSpace(questionText))
        {
            validationError = $"question '{questionId}' has no question text";
            return false;
        }

        if (answerOptions == null || answerOptions.Length != 4)
        {
            validationError = $"question '{questionId}' must have exactly 4 answer options";
            return false;
        }

        for (int i = 0; i < answerOptions.Length; i++)
        {
            AnswerOptionData answer = answerOptions[i];
            if (answer == null)
            {
                validationError = $"question '{questionId}' answer {i + 1} is not assigned";
                return false;
            }

            if (!answer.IsValid(out string answerError))
            {
                validationError = $"question '{questionId}' answer {i + 1} is invalid: {answerError}";
                return false;
            }
        }

        validationError = string.Empty;
        return true;
    }
}
