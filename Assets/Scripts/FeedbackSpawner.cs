using UnityEngine;

public class FeedbackSpawner : MonoBehaviour
{
    public static FeedbackSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject correctPrefab;
    [SerializeField] private GameObject wrongPrefab;

    [Header("Parent (Canvas UI)")]
    [SerializeField] private RectTransform feedbackParent; // Panel sous le Canvas

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Spawn feedback à la position d’un autre RectTransform UI.
    /// </summary>
    public void SpawnFeedbackAtRect(RectTransform source, bool isCorrect)
    {
        if (source == null || feedbackParent == null) return;

        GameObject prefab = isCorrect ? correctPrefab : wrongPrefab;

        // On instancie directement sous le même parent UI
        GameObject go = Instantiate(prefab, feedbackParent);

        RectTransform rt = go.GetComponent<RectTransform>();
        // On convertit la position locale de source vers celle du parent feedback
        Vector3 localPos = feedbackParent.InverseTransformPoint(source.position);
        rt.anchoredPosition = localPos;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }
}