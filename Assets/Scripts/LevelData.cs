using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "NewLevel", menuName = "NutritionQuiz/Level")]
public class LevelData : ScriptableObject
{
    public LevelType TypeDeNiveau = LevelType.Normal;
    public DifficultyLevel Difficulty = DifficultyLevel.Medium;
    public List<QuestionData> Questions = new List<QuestionData>();
}