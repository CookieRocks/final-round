using UnityEngine;

[CreateAssetMenu(fileName = "AnswerOption", menuName = "Final Round/Answer Option")]
public sealed class AnswerOptionData : ScriptableObject
{
    [SerializeField] private string answerText;
    [SerializeField] private int technicalDelta;
    [SerializeField] private int commercialDelta;
    [SerializeField] private int rapportDelta;
    [SerializeField] private int energyDelta;

    public string AnswerText => answerText;
    public int TechnicalDelta => technicalDelta;
    public int CommercialDelta => commercialDelta;
    public int RapportDelta => rapportDelta;
    public int EnergyDelta => energyDelta;

    public bool IsValid(out string validationError)
    {
        if (string.IsNullOrWhiteSpace(answerText))
        {
            validationError = "answer text is empty";
            return false;
        }

        validationError = string.Empty;
        return true;
    }
}
