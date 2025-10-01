using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class QuestionValidateButtonUI : MonoBehaviour
{
    [BoxGroup("Références UI")]
    [SerializeField] private GameObject panel;

    public static QuestionValidateButtonUI Instance { get; private set; }

    [SerializeField] private Button button;
    [ReadOnly][SerializeField] private BaseQuestionUI currentQuestion;
    [ReadOnly][SerializeField] private bool shouldShow;

    private void Awake()
    {
        // Singleton classique
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void BindQuestion(BaseQuestionUI questionUI, QuestionType type)
    {
        currentQuestion = questionUI;
        button.interactable = true;
        UpdatePanelState(type);
    }

    public void Btn_OnClick()
    {
        if (currentQuestion == null) return;

        InteractionManager.Instance.TriggerMediumVibration();
        button.interactable = false;

        int answer = currentQuestion.GetAnswer();
        GameManager.Instance.OnQuestionAnswered(answer);

        currentQuestion.Close();
    }


    public void UpdatePanelState(QuestionType type)
    {
        shouldShow = ShouldShowValidateButton(type);

        if (shouldShow)
            OuvrirPanel();
        else
            FermerPanel();

         Debug.Log($"[Panel] todo :{shouldShow}");
    }

    public void DisableButton()
    {
        button.interactable = false;
    }
    public void EnableButton()
    {
        button.interactable = true;
    }

    public void OuvrirPanel()
    {
        panel.SetActive(true);
    }

    public void FermerPanel()
    {
        panel.SetActive(false);
    }

    public bool ShouldShowValidateButton(QuestionType type)
    {
        switch (type)
        {
            case QuestionType.CaloriesDual:
            case QuestionType.FunMeasure:
            case QuestionType.Sport:
            case QuestionType.Recycling:
            case QuestionType.NutritionTable:
                return false;

            case QuestionType.EstimateCalories:
            case QuestionType.MealComposition:
            case QuestionType.Sugar:
            case QuestionType.Intru:
            case QuestionType.Tri:
            case QuestionType.Subtraction:
                return true;

            default:
                return true;
        }
    }
}