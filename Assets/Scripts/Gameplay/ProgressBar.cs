using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private int totalQuestions = 1;


    public void SetTotalQuestions(int total)
    {
        totalQuestions = Mathf.Max(1, total); // évite division par zéro
        ResetProgress();
    }


    public void SetCurrentQuestionIndex(int index)
    {
        float progress = (float)(index) / totalQuestions;
        SetProgress(progress);
    }


    public void SetProgress(float progress)
    {


        fillImage.fillAmount = Mathf.Clamp01(progress);
    }

    public void ResetProgress()
    {
        SetProgress(0f);
    }
}