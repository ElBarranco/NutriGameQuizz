using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class QuestionRecyclingUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private FoodConveyorSpawnerUI spawner;

    private QuestionData question;
    private Action<int, bool> onAnswered;

    [ReadOnly, SerializeField] private List<FoodConveyorItemUI> aliveItems = new List<FoodConveyorItemUI>();

    private void OnEnable()
    {
        FoodConveyorItemUI.OnAnyDestroyed += HandleItemDestroyed;
    }

    private void OnDisable()
    {
        FoodConveyorItemUI.OnAnyDestroyed -= HandleItemDestroyed;
    }

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;

        aliveItems.Clear();

        // Affiche le texte correspondant
        UpdateQuestionText(q.SousType);

        // Lance le spawn
        spawner.Init(q.Aliments, q.PortionSelections, q.Solutions);

        // Récupère les items spawnés au fur et à mesure
        aliveItems.AddRange(spawner.GetSpawnedItems());
    }

    private void HandleItemDestroyed(FoodConveyorItemUI item, bool wasIntruder)
    {
        if (aliveItems.Contains(item))
            aliveItems.Remove(item);


        // ✅ Ne check la fin QUE si tout a spawn
        if (spawner.HaveAllSpawned() && aliveItems.Count == 0)
        {
            Debug.Log("[QuestionRecyclingUI] Tous les items détruits → question terminée !");
            onAnswered?.Invoke(1, true);
            Destroy(gameObject);
        }
    }

    private void UpdateQuestionText(QuestionSubType subType)
    {
        string txt = subType switch
        {
            QuestionSubType.Proteine => "protéines",
            QuestionSubType.Glucide => "glucides",
            QuestionSubType.Lipide => "lipides",
            _ => ""
        };

        questionText.text = txt;
    }
}