using UnityEngine;

public class QuestionsArray
{
    public Question[] questions;

    public QuestionsArray(Question[] questions)
    {
        this.questions = questions;
    }
}
public class MatchQuestions : Singleton<MatchQuestions>
{
    static QuestionsArray[] questionArrays;

    public TextAsset[] questionsJsons;

	void Awake()
    {
        if (questionArrays == null)
        {
            questionArrays = new QuestionsArray[questionsJsons.Length];
            for (int i = 0; i < questionsJsons.Length; i++)
                questionArrays[i] = new QuestionsArray(JsonUtilityExtended.FromJsonArray<Question>(questionsJsons[i].text));
        }
    }

    public Question GetQuestion(int pageNum)
    {
        if (questionArrays == null)
            Awake();
        Question q = questionArrays[pageNum].questions[Random.Range(0, questionArrays.Length)];
        return q;
    }
}