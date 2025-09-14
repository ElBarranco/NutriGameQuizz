using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class QuestionIntrusUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private List<GameObject> foodSlots; 
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

        // Reset
        currentSelection.Clear();
        CleanupSpawned();

        // ✅ Activer uniquement les slots nécessaires
        for (int i = 0; i < foodSlots.Count; i++)
        {
            bool active = i < q.Aliments.Count;
            foodSlots[i].SetActive(active);

            if (active)
            {
                FoodData f = q.Aliments[i];

                // Instancier un bouton dans ce slot
                FoodSelectableUI item = Instantiate(foodButtonPrefab, foodSlots[i].transform, false);
                item.gameObject.name = $"INTRUS_{f.Name}_{i}";
                item.Init(f, i, OnFoodClicked);

                spawnedItems.Add(item);

                currentSelection.Add(1); // par défaut "bon"
            }
        }

        UpdateDebugText();
    }

    private void CleanupSpawned()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();
    }

    private void OnFoodClicked(int index, bool isSelectedAsIntrus)
    {
        currentSelection[index] = isSelectedAsIntrus ? 2 : 1;
        UpdateDebugText();
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

        onAnswered?.Invoke(encodedAnswer, false);

        Destroy(gameObject);
    }

    private int EncodeSelection(List<int> order)
    {
        string concat = string.Concat(order);
        return int.Parse(concat);
    }
}