// ConveyorSlotUI.cs
using UnityEngine;

public class ConveyorSlotUI : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    private RectTransform endPoint;

    private float speed = 100f;
    private bool isMoving = false;   // bouge seulement si true
    private bool isPaused = false;   // pause globale (ex: Easy)

    private void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
    }

    /// <summary>Configure le slot (endpoint + vitesse) et démarre le mouvement.</summary>
    public void Init(RectTransform end, float moveSpeed)
    {
        endPoint = end;
        speed = moveSpeed;
        StartMoving();
    }

    public void StartMoving()   { isMoving = true;  }
    public void StopMoving()    { isMoving = false; }
    public void SetPaused(bool p) { isPaused = p;   }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetEndPoint(RectTransform end) => endPoint = end;
    public bool IsMoving() => isMoving && !isPaused;

    private void Update()
    {
        if (!isMoving || isPaused) return;

        rect.anchoredPosition -= new Vector2(0f, speed * Time.deltaTime);

        // sortie par le bas → cleanup slot + contenu
        if (endPoint != null && rect.position.y < endPoint.position.y)
            Destroy(gameObject);
    }
}