using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;

public class QuestionSortUI : BaseQuestionUI
{
    [Header("Références")]
    [SerializeField] private RectTransform bottomPanel;
    [SerializeField] private TriDropZone dropZone;
    [SerializeField] private FoodDraggableUI foodPrefab;
    [SerializeField] private QuestionValidateButtonUI validateButtonUI; 
    [SerializeField] private InitialDockUI bottomDock;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<int> currentOrder = new List<int>();

    


    public void Init(QuestionData q)
    {
        question = q;



        dropZone.Init(q.PortionSelections);

        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = q.PortionSelections[i];

            FoodDraggableUI item = Instantiate(foodPrefab, bottomPanel, false);
            item.gameObject.name = $"SORT_{f.Name}_{i}";
            item.Init(f, sel, q.SousType, i);

            if (bottomDock != null && i < bottomDock.SlotCount)
                bottomDock.PlaceAtIndex(item, i, false);

            item.SetOriginalDock(bottomDock, i);
            item.CacheOriginalTransform();
            item.PlaySpawnAnimation(i * 0.08f);
        }
    }

    public void UpdateCurrentOrder(List<int> newOrder)
    {
        currentOrder = newOrder;
        guess = EncodeOrder(currentOrder);       
    }

    private int EncodeOrder(List<int> order)
    {
        string concat = string.Concat(order);
        return int.Parse(concat);
    }
}