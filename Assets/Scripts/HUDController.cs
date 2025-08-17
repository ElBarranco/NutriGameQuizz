using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class HUDController : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TextMeshProUGUI questionCounterText;
    [SerializeField] private RectTransform nextButtonContainer;
    [SerializeField] private TextMeshProUGUI questionTitleText;

    [SerializeField] private Vector2 shownPosition;
    [SerializeField] private Vector2 hiddenPosition;

    [SerializeField] private GameObject hudInGameUI;
    [SerializeField] private GameObject finalScreenUI;


    [Header("Buttons")]
    [SerializeField] private GameObject correctButtonGO;
    [SerializeField] private GameObject wrongButtonGO;

    [Header("More Info Panels")]
    [SerializeField] private GameObject moreInfoDualPrefab;
    [SerializeField] private GameObject moreInfoEstimatePrefab;
    [SerializeField] private GameObject moreInfoFunMeasurePrefab;
    [SerializeField] private Transform moreInfoPanelParent;

    private GameObject currentMoreInfoPanel;

    [Header("Icônes par sous-type")]
    [SerializeField] private Image nutrientIcon;

    [SerializeField] private Sprite caloriesIcon;
    [SerializeField] private Sprite proteinsIcon;
    [SerializeField] private Sprite sugarsIcon;
    [SerializeField] private Sprite fatsIcon;
    [SerializeField] private Sprite fibersIcon;

    [SerializeField] private ProgressBar progressBar;

    private void Start()
    {
        float height = nextButtonContainer.sizeDelta.y;
        shownPosition = Vector2.zero;
        hiddenPosition = new Vector2(0f, -height);

        hudInGameUI.SetActive(false);
        finalScreenUI.SetActive(false);
    }
    public void InitGame(int totalQuestions)
    {
        hudInGameUI.SetActive(true);
        UpdateQuestionNumber(0);
        progressBar.SetTotalQuestions(totalQuestions);
        //SetNextButtonVisible(false);
    }
    public void UpdateQuestionNumber(int currentIndex)
    {
        questionCounterText.text = $"{currentIndex + 1}";
    }

    public void SetNextButtonVisible(bool visible, bool isCorrect = true)
    {
        correctButtonGO.SetActive(false);
        wrongButtonGO.SetActive(false);

        if (visible)
        {
            if (isCorrect)
                correctButtonGO.SetActive(true);
            else
                wrongButtonGO.SetActive(true);

            nextButtonContainer.DOAnchorPos(shownPosition, 0.4f).SetEase(Ease.OutBack);
        }
        else
        {
            nextButtonContainer.DOAnchorPos(hiddenPosition, 0.3f).SetEase(Ease.InBack);
        }
    }

    public void UpdateHUDForNewQuestion(int questionIndex, QuestionData data)
    {
        // 1. Numéro de question
        UpdateQuestionNumber(questionIndex);

        // 2. Titre de la question
        SetQuestionTitle(data.Type, data.SousType);

        // 3. Icône selon le sous-type
        SetNutrientIcon(data.SousType);

        // 4. Cacher le bouton de validation
        SetNextButtonVisible(false);

        // 5. Supprimer les panneaux d'info précédents
        HideMoreInfo();
        progressBar.SetCurrentQuestionIndex(questionIndex);
    }

    public void ShowMoreInfo(QuestionData data, int userAnswer = -1)
    {
        if (currentMoreInfoPanel != null)
        {
            HideMoreInfo();
        }

        // On crée le panneau d'information selon le type de question
        switch (data.Type)
        {
            case QuestionType.EstimateCalories:
                currentMoreInfoPanel = Instantiate(moreInfoEstimatePrefab, moreInfoPanelParent);
                MoreInfoEstimatePanelUI estimatePanel = currentMoreInfoPanel.GetComponent<MoreInfoEstimatePanelUI>();
                estimatePanel.Show(data.Aliments[0], userAnswer, data.SousType);
                break;

            case QuestionType.FunMeasure:
                currentMoreInfoPanel = Instantiate(moreInfoFunMeasurePrefab, moreInfoPanelParent);
                MoreInfoFunMeasurePanelUI funPanel = currentMoreInfoPanel.GetComponent<MoreInfoFunMeasurePanelUI>();
                funPanel.Show(data.Aliments, data.SpecialMeasures, data.IndexBonneRéponse);
                break;

            case QuestionType.CaloriesDual:
            default:
                currentMoreInfoPanel = Instantiate(moreInfoDualPrefab, moreInfoPanelParent);
                MoreInfoDualPanelUI dualPanel = currentMoreInfoPanel.GetComponent<MoreInfoDualPanelUI>();
                dualPanel.Show(
                    data.Aliments[0],
                    data.Aliments[1],
                    data.PortionSelections[0],
                    data.PortionSelections[1],
                    data.IndexBonneRéponse,
                    data.SousType,
                    userAnswer
                );
                break;
        }

    }


    public void HideMoreInfo()
    {
        if (currentMoreInfoPanel == null)
            return;

        var basePanel = currentMoreInfoPanel.GetComponent<MoreInfoPanelBase>();

        switch (basePanel)
        {
            case MoreInfoDualPanelUI dual:
                dual.Hide();
                break;

            case MoreInfoEstimatePanelUI estimate:
                estimate.Hide();
                break;

            /*case MoreInfoFunMeasurePanelUI fun:
                fun.Hide();
                break;*/

            default:
                Destroy(currentMoreInfoPanel);
                break;
        }

        currentMoreInfoPanel = null;
    }

    public void Btn_OnNextClicked()
    {
        InteractionManager.Instance.TriggerMediumVibration();

        SetNextButtonVisible(false);
        HideMoreInfo();
        GameManager.Instance.TriggerNextStep();
    }

    private void Update()
    {
        if (GameManager.Instance.EnableMoreInfo && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Btn_OnNextClicked();
        }
    }

    private void SetNutrientIcon(QuestionSubType subType)
    {
        Sprite icon = subType switch
        {
            QuestionSubType.Proteine => proteinsIcon,
            QuestionSubType.Glucide => sugarsIcon,
            QuestionSubType.Lipide => fatsIcon,
            QuestionSubType.Fibres => fibersIcon,
            _ => caloriesIcon
        };

        if (nutrientIcon != null)
            nutrientIcon.sprite = icon;
    }

    public void SetQuestionTitle(QuestionType type, QuestionSubType subType)
    {
        string title = type switch
        {
            QuestionType.EstimateCalories => subType switch
            {
                QuestionSubType.Proteine => "Estime la quantité de protéines !",
                QuestionSubType.Glucide => "Estime la quantité de glucides !",
                QuestionSubType.Lipide => "Estime la quantité de lipides !",
                QuestionSubType.Fibres => "Estime la quantité de fibres !",
                _ => "Devine les calories !"
            },

            QuestionType.CaloriesDual => subType switch
            {
                QuestionSubType.Proteine => "Quel aliment contient le plus de protéines ?",
                QuestionSubType.Glucide => "Quel aliment contient le plus de glucides ?",
                QuestionSubType.Lipide => "Quel aliment est le plus gras ?",
                QuestionSubType.Fibres => "Quel aliment contient le plus de fibres ?",
                _ => "Quel aliment est le plus calorique ?"
            },

            QuestionType.FunMeasure => "Tu vas être surpris...",
            _ => "Question nutritionnelle"
        };

        questionTitleText.text = title;
    }

    public void ShowEndGameUI()
    {
        hudInGameUI.SetActive(false);
        finalScreenUI.SetActive(true);
        Debug.Log("🎉 Fin du jeu ! Affichage de l'écran de fin.");
    }

    public void Btn_OnMenuClicked()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        hudInGameUI.SetActive(true);
        finalScreenUI.SetActive(false);
        uiManager.ShowMenu();
    }
}