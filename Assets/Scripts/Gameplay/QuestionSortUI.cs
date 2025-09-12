using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;

public class QuestionSortUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RectTransform bottomPanel;
    [SerializeField] private TriDropZone dropZone;
    [SerializeField] private FoodDraggableUI foodPrefab;
    [SerializeField] private QuestionValidateButtonUI validateButtonUI; 
    [SerializeField] private InitialDockUI bottomDock;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<int> currentOrder = new List<int>();

    private QuestionData question;
    private Action<int, bool> onAnswered;

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;


        // écoute la zone pour activer/désactiver le bouton
        dropZone.OnFilledChanged += validateButtonUI.SetActiveState;



        dropZone.Init(q.PortionSelections);

        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = q.PortionSelections[i];

            FoodDraggableUI item = Instantiate(foodPrefab, bottomPanel, false);
            item.gameObject.name = $"SORT_{f.Name}_{i}";
            item.Init(f, sel, i);

            if (bottomDock != null && i < bottomDock.SlotCount)
                bottomDock.PlaceAtIndex(item, i, false);

            item.SetOriginalDock(bottomDock, i);
            item.CacheOriginalTransform();
            item.PlaySpawnAnimation(i * 0.08f);
        }
    }

    public void Btn_Validate()
    {
        InteractionManager.Instance.TriggerMediumVibration();

        currentOrder = dropZone.GetCurrentOrderIndices();
        int ordreEncode = EncodeOrder(currentOrder);

        onAnswered?.Invoke(ordreEncode, false);

        Destroy(gameObject);
    }

    private int EncodeOrder(List<int> order)
    {
        string concat = string.Concat(order);
        return int.Parse(concat);
    }
}