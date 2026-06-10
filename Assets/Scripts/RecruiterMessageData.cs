using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RecruiterMessageData", menuName = "Final Round/VS2/Recruiter Message")]
public sealed class RecruiterMessageData : ScriptableObject
{
    public string messageId;
    public string senderName;
    public string subject;
    [TextArea(4, 12)] public string messageText;
    public RecruiterResponseChoice[] responseChoices;
    public string nextMessageId;
}

[Serializable]
public sealed class RecruiterResponseChoice
{
    public string choiceId;
    public string label;
    [TextArea(2, 6)] public string responseText;
    public string nextMessageId;
}
