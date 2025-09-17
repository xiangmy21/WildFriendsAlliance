using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
    public string animal;
    public string question;
    public string[] options;
    public string correct_answer;
    public string explanation;
    public int difficulty;
}

[System.Serializable]
public class QuestionList
{
    public List<QuestionData> questions;
}

[System.Serializable]
public class AnimalFriendshipData
{
    public string animalName;
    public int friendshipLevel;
    public float battleBonus; // 战斗中的奖励百分比

    public AnimalFriendshipData(string name)
    {
        animalName = name;
        friendshipLevel = 0;
        battleBonus = 0f;
    }
}

[System.Serializable]
public class QuizCardData
{
    public string animalName;
    public int difficulty;
    public QuestionData question;

    public QuizCardData(string animal, int diff, QuestionData q)
    {
        animalName = animal;
        difficulty = diff;
        question = q;
    }
}