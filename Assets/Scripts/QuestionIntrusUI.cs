using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class QuestionIntrusUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private FoodSelectableUI foodButtonPrefab;
    [SerializeField] private Button validateButtonUI;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<int> currentSelection = new List<int>();

    private QuestionData question;
    private Action<int, bool> onAnswered;
    private List<FoodSelectableUI> spawnedItems = new List<FoodSelectableUI>();

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;

        // Instancier les 4 aliments
        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];

            FoodSelectableUI item = Instantiate(foodButtonPrefab, contentPanel, false);
            item.gameObject.name = $"INTRUS_{f.Name}_{i}";
            item.Init(f, i, OnFoodClicked);

            spawnedItems.Add(item);

            // Par défaut → tout est "bon" (1)
            currentSelection.Add(1);
        }

        UpdateDebugText(); // ✅ init
    }

    private void OnFoodClicked(int index, bool isSelectedAsIntrus)
    {
        // Toggle entre 1 (bon) et 2 (intrus)
        currentSelection[index] = isSelectedAsIntrus ? 2 : 1;

        UpdateDebugText(); // ✅ maj debug
    }

    private void UpdateDebugText()
    {
        int encoded = EncodeSelection(currentSelection);
        debugText.text = $"{encoded}";
    }

    public void Btn_Validate()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        validateButtonUI.interactable = false;

        int encodedAnswer = EncodeSelection(currentSelection);

        // On envoie la réponse au manager
        onAnswered?.Invoke(encodedAnswer, false);

        Destroy(gameObject);
    }

    private int EncodeSelection(List<int> order)
    {
        string concat = string.Concat(order);
        return int.Parse(concat);
    }
}