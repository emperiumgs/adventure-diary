using System;

[Serializable]
public struct Question
{
    public string question,
        rightAnswer;

    public bool valid;

    public Question(string question, string rightAnswer, bool valid)
    {
        this.question = question;
        this.rightAnswer = rightAnswer;
        this.valid = valid;
    }
}