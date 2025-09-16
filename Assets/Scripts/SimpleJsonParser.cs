using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单的JSON解析器，用于解析问题数据
/// 避免依赖外部JSON库
/// </summary>
public static class SimpleJsonParser
{
    public static List<QuestionData> ParseQuestionFile(string jsonContent)
    {
        List<QuestionData> questions = new List<QuestionData>();

        try
        {
            // 使用Unity的JsonUtility解析
            // 需要包装成一个对象，因为JsonUtility不能直接解析数组
            string wrappedJson = "{\"questions\":" + jsonContent + "}";
            QuestionWrapper wrapper = JsonUtility.FromJson<QuestionWrapper>(wrappedJson);

            if (wrapper?.questions != null)
            {
                questions = new List<QuestionData>(wrapper.questions);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON解析失败: {e.Message}");
        }

        return questions;
    }

    [System.Serializable]
    private class QuestionWrapper
    {
        public QuestionData[] questions;
    }
}