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
    private bool allSpawned = false;


    [ReadOnly, SerializeField] private List<FoodConveyorItemUI> aliveItems = new List<FoodConveyorItemUI>();

    private void OnEnable()
    {
        FoodConveyorItemUI.OnAnyDestroyed += HandleItemDestroyed;
        FoodConveyorItemUI.OnAnySpawned += HandleItemSpawned;
    }

    private void OnDisable()
    {
        FoodConveyorItemUI.OnAnyDestroyed -= HandleItemDestroyed;
        FoodConveyorItemUI.OnAnySpawned -= HandleItemSpawned;
    }

    public void Init(QuestionData q)
    {
        question = q;

        aliveItems.Clear();
        UpdateQuestionText(q.SousType);
        spawner.Init(q.Aliments, q.PortionSelections, q.Solutions, q.SousType);
    }

    private void HandleItemSpawned(FoodConveyorItemUI item)
    {
        aliveItems.Add(item);
    }

    private void HandleItemDestroyed(FoodConveyorItemUI item, bool wasIntruder)
    {
        if (aliveItems.Contains(item))
            aliveItems.Remove(item);

        Debug.Log($"[QuestionRecyclingUI] Check condition → HaveAllSpawned: {allSpawned}, AliveItems.Count: {aliveItems.Count}");

        if (allSpawned && aliveItems.Count == 0)
        {
            Debug.Log("[QuestionRecyclingUI] Tous les items détruits → question terminée !");
            GameManager.Instance.OnQuestionAnswered(1);
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

    public void SetAllSpawned()
    {
        allSpawned = true;
    }
}