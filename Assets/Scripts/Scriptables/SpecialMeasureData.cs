using UnityEngine;

[CreateAssetMenu(fileName = "SpecialMeasureData", menuName = "Quiz/FunMeasure")]
public class SpecialMeasureData : ScriptableObject
{
    public SpecialMeasureType Type;
    public long VolumeLitres;
}