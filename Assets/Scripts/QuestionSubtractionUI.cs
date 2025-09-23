using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class QuestionSubtractionUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private List<GameObject> foodSlots;
    [SerializeField] private FoodSelectableSubtractionUI foodButtonPrefab;
    [SerializeField] private Button validateButtonUI;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Debug")]
    [SerializeField, ReadOnly] private int currentSelection = -1;
    [SerializeField, ReadOnly] private int encodedAnswer = -1;

    private QuestionData question;
    private Action<int, bool> onAnswered;
    private List<FoodSelectableSubtractionUI> spawnedItems = new List<FoodSelectableSubtractionUI>();

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;

        // Reset
        currentSelection = -1;
        encodedAnswer = -1;
        CleanupSpawned();

        // ✅ Activer uniquement les slots nécessaires
        for (int i = 0; i < foodSlots.Count; i++)
        {
            bool active = i < q.Aliments.Count;
            foodSlots[i].SetActive(active);

            if (active)
            {
                FoodData f = q.Aliments[i];
                PortionSelection sel = q.PortionSelections[i]; 

                // Instancier un bouton dans ce slot
                FoodSelectableSubtractionUI item = Instantiate(foodButtonPrefab, foodSlots[i].transform, false);
                item.gameObject.name = $"SUBTRACTION_{f.Name}_{i}";
                item.Init(f, sel, i, OnFoodClicked); 

                spawnedItems.Add(item);
            }
        }

        validateButtonUI.interactable = false;
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

    private void OnFoodClicked(int index, bool isSelected)
    {
        if (isSelected)
        {
            // Un seul choix possible → reset les autres
            foreach (var item in spawnedItems)
                item.SetSelected(false);

            spawnedItems[index].SetSelected(true);
            currentSelection = index;
        }
        else
        {
            currentSelection = -1;
        }

        // ✅ On met à jour encodedAnswer directement
        encodedAnswer = (currentSelection == -1) ? -1 : currentSelection + 1;

        validateButtonUI.interactable = (currentSelection != -1);
        UpdateDebugText();
    }

    private void UpdateDebugText()
    {
        debugText.text = (encodedAnswer <= 0)
            ? "0"
            : encodedAnswer.ToString();
    }

    public void Btn_Validate()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        validateButtonUI.interactable = false;

        // ✅ On renvoie directement la variable encodée
        onAnswered?.Invoke(encodedAnswer, false);

        Destroy(gameObject);
    }
}