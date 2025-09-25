using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class HUDController : MonoBehaviour
{


    [SerializeField] private UIManager uiManager;
    [SerializeField] private QuestionTitleManager questionTitleManager;
    [SerializeField] private TextMeshProUGUI questionCounterText;
    [SerializeField] private RectTransform nextButtonContainer;
    [SerializeField] private PowerUpUIManager powerUpUIManager;
    [SerializeField] private QuestionValidateButtonUI validateButtonUI;

    [SerializeField] private Vector2 shownPosition;
    [SerializeField] private Vector2 hiddenPosition;

    [Header("UI")]
    [SerializeField] private GameObject hudInGameUI;
    [SerializeField] private FinalUIController finalUI;


    [Header("Buttons")]
    [SerializeField] private GameObject correctButtonGO;
    [SerializeField] private GameObject wrongButtonGO;

    [Header("More Info Panels")]
    [SerializeField] private GameObject moreInfoSubtractionPrefab;
    [SerializeField] private GameObject moreInfoDualPrefab;
    [SerializeField] private GameObject moreInfoEstimatePrefab;
    [SerializeField] private GameObject moreInfoFunMeasurePrefab;
    [SerializeField] private GameObject moreInfoMealPrefab;
    [SerializeField] private GameObject moreInfoSportPrefab;
    [SerializeField] private GameObject moreInfoTriPrefab;
    [SerializeField] private GameObject moreInfoIntrusPrefab;
    [SerializeField] private GameObject moreInfoSugarPrefab;
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
        finalUI.gameObject.SetActive(false);
    }
    public void InitGame(int totalQuestions)
    {
        hudInGameUI.SetActive(true);
        UpdateQuestionNumber(0);
        progressBar.SetTotalQuestions(totalQuestions);
        //SetNextButtonVisible(false);
        powerUpUIManager.OuvrirPanel();
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
            powerUpUIManager.FermerPanel();
            validateButtonUI.FermerPanel();
        }
        else
        {
            nextButtonContainer.DOAnchorPos(hiddenPosition, 0.3f).SetEase(Ease.InBack);
            powerUpUIManager.OuvrirPanel();

        }
    }

    public void UpdateHUDForNewQuestion(int questionIndex, QuestionData data)
    {
        // 1. Numéro de question
        UpdateQuestionNumber(questionIndex);

        // 2. Titre de la question
        questionTitleManager.SetQuestionTitle(data.Type, data.SousType, data.Aliments, data.PortionSelections, data.ValeursComparees[0]);

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

        switch (data.Type)
        {
            case QuestionType.EstimateCalories:
                currentMoreInfoPanel = Instantiate(moreInfoEstimatePrefab, moreInfoPanelParent);
                MoreInfoEstimatePanelUI estimatePanel = currentMoreInfoPanel.GetComponent<MoreInfoEstimatePanelUI>();
                estimatePanel.Show(data.Aliments[0], data.PortionSelections[0], userAnswer, data.SousType);
                break;

            case QuestionType.Sport:
                currentMoreInfoPanel = Instantiate(moreInfoSportPrefab, moreInfoPanelParent);
                MoreInfoSportPanelUI sportPanel = currentMoreInfoPanel.GetComponent<MoreInfoSportPanelUI>();
                sportPanel.Show(
                    Mathf.RoundToInt(data.ValeursComparees[0]),
                    data.Aliments[0],
                    data.SportChoices[0],
                    data.SportChoices[1],
                    data.IndexBonneRéponse,
                    userAnswer
                );
                break;

            case QuestionType.FunMeasure:
                currentMoreInfoPanel = Instantiate(moreInfoFunMeasurePrefab, moreInfoPanelParent);
                MoreInfoFunMeasurePanelUI funPanel = currentMoreInfoPanel.GetComponent<MoreInfoFunMeasurePanelUI>();
                funPanel.Show(data.Aliments, data.SpecialMeasures, data.IndexBonneRéponse);
                break;

            case QuestionType.MealComposition:
                currentMoreInfoPanel = Instantiate(moreInfoMealPrefab, moreInfoPanelParent);
                MoreInfoMealPanelUI mealPanel = currentMoreInfoPanel.GetComponent<MoreInfoMealPanelUI>();
                mealPanel.Show(data.Aliments, data.PortionSelections, data.ValeursComparees[0], userAnswer, data.Solutions);
                break;

            case QuestionType.Tri:
                currentMoreInfoPanel = Instantiate(moreInfoTriPrefab, moreInfoPanelParent);
                MoreInfoSortPanelUI triPanel = currentMoreInfoPanel.GetComponent<MoreInfoSortPanelUI>();
                triPanel.Show(data, userAnswer);
                break;

            case QuestionType.Intru:
                currentMoreInfoPanel = Instantiate(moreInfoIntrusPrefab, moreInfoPanelParent);
                MoreInfoIntrusPanelUI intrusPanel = currentMoreInfoPanel.GetComponent<MoreInfoIntrusPanelUI>();
                intrusPanel.Show(data, userAnswer);
                break;

            case QuestionType.Sugar:
                currentMoreInfoPanel = Instantiate(moreInfoSugarPrefab, moreInfoPanelParent);
                MoreInfoSugarPanelUI sugarPanel = currentMoreInfoPanel.GetComponent<MoreInfoSugarPanelUI>();
                sugarPanel.Show(data, userAnswer);
                break;

            case QuestionType.Subtraction:
                currentMoreInfoPanel = Instantiate(moreInfoSubtractionPrefab, moreInfoPanelParent);
                MoreInfoSubtractionPanelUI subtractionPanel = currentMoreInfoPanel.GetComponent<MoreInfoSubtractionPanelUI>();
                subtractionPanel.Show(data, userAnswer);
                break;

            case QuestionType.Recycling:
                GameManager.Instance.TriggerNextStep();
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

            case MoreInfoSubtractionPanelUI subtraction: // ✅ nouveau
                subtraction.Hide();
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

    public void Btn_Close()
    {
        InteractionManager.Instance.TriggerMediumVibration();

        GameManager.Instance.QuitGame();
        uiManager.ShowMenu();
        hudInGameUI.SetActive(false);
        if (currentMoreInfoPanel != null)
        {
            Destroy(currentMoreInfoPanel);
        }

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

    public void ShowEndGameUI()
    {
        hudInGameUI.SetActive(false);
        finalUI.gameObject.SetActive(true);
        finalUI.InitFinalUI(ScoreManager.Instance.GetScore(), GameManager.Instance.GetTotalQuestionsCount(), ScoreManager.Instance.GetCorrectAnswers());
        Debug.Log("🎉 Fin du jeu ! Affichage de l'écran de fin.");
    }

    public void Btn_OnMenuClicked()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        hudInGameUI.SetActive(true);
        finalUI.gameObject.SetActive(false);
        uiManager.ShowMenu();
    }




}