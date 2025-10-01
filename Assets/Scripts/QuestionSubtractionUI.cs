using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class QuestionSubtractionUI : BaseQuestionUI
{
    [Header("Références")]
    [SerializeField] private List<GameObject> foodSlots;
    [SerializeField] private FoodSelectableSubtractionUI foodButtonPrefab;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Debug")]
    [SerializeField, ReadOnly] private int currentSelection = -1;


    
    private List<FoodSelectableSubtractionUI> spawnedItems = new List<FoodSelectableSubtractionUI>();

    public void Init(QuestionData q)
    {
        question = q;
        // Reset
        currentSelection = -1;
        guess = -1;
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
                item.Init(f, q.SousType, i, OnFoodClicked, sel); 

                spawnedItems.Add(item);
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
        guess = (currentSelection == -1) ? -1 : currentSelection + 1;

        UpdateDebugText();
    }

    private void UpdateDebugText()
    {
        debugText.text = (guess <= 0)
            ? "0"
            : guess.ToString();
    }

}