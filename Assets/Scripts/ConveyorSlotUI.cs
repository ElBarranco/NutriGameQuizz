using UnityEngine;

public class ConveyorSlotUI : MonoBehaviour
{
    private float speed = 100f;
    [SerializeField] private RectTransform rect;
    private RectTransform endPoint;

    private bool isMoving = false; // 🚀 bouge seulement si true

    public void Init(RectTransform end, float moveSpeed)
    {
        endPoint = end;
        speed = moveSpeed;
        StartMoving();
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        rect.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

        if (endPoint != null && rect.position.y < endPoint.position.y)
        {
            Destroy(gameObject); // détruit le slot ET tout son contenu
        }
    }
}