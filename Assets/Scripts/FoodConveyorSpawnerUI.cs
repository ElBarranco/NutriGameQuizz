using System.Collections.Generic;
using UnityEngine;

public class FoodConveyorSpawnerUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private FoodConveyorItemUI foodPrefab;
    [SerializeField] private Transform itemsParent;

    [Header("Paramètres")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float itemSpeed = 100f;

    private Queue<FoodData> queueFoods = new Queue<FoodData>();
    private List<PortionSelection> queuePortions = new List<PortionSelection>();
    private List<int> solutions = new List<int>(); 
    private bool isSpawning = false;

    public void Init(List<FoodData> foods, List<PortionSelection> portions, List<int> answers)
    {
        queueFoods.Clear();
        queuePortions.Clear();
        solutions.Clear();

        for (int i = 0; i < foods.Count; i++)
        {
            queueFoods.Enqueue(foods[i]);
            queuePortions.Add(portions[i]);
            solutions.Add(answers[i]); 
        }

        if (!isSpawning)
            StartCoroutine(SpawnLoop());
    }

    private System.Collections.IEnumerator SpawnLoop()
    {
        isSpawning = true;

        int index = 0;
        while (queueFoods.Count > 0)
        {
            FoodData f = queueFoods.Dequeue();
            PortionSelection sel = queuePortions[index];
            int expected = solutions[index]; 

            // Spawn l’aliment
            FoodConveyorItemUI item = Instantiate(foodPrefab, itemsParent);
            item.transform.position = spawnPoint.position;
            item.Init(f, sel, index);
            item.SetAsIntruder(expected == 0); 
            item.PlaySpawnAnimation();

            // Configurer son mouvement
            item.SetSpeed(itemSpeed);
            item.SetEndPoint(endPoint);

            index++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }
}