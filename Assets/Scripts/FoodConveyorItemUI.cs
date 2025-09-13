using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using NaughtyAttributes;
public class FoodConveyorItemUI : FoodDraggableUI
{
    public static event System.Action<FoodConveyorItemUI> OnAnyThrown; // ✅ global : quand lancé
    public event System.Action<FoodConveyorItemUI, bool> OnItemDestroyed; // ✅ par item : notifie la question

    [Header("Conveyor Settings")]
    [SerializeField] private float speed = 100f; // pixels par seconde
    private RectTransform endPoint;
    [SerializeField] private RectTransform titre; // assigner dans l’inspecteur
    private bool isMoving = true;

    [Header("Throw Settings")]
    [SerializeField] private float throwSpeedThreshold = 1200f; // vitesse minimale pour un "lancer"
    [SerializeField] private float throwDecay = 0.98f;           // ralentissement progressif
    [SerializeField] private float maxLifetime = 5f;             // sécurité auto-destruction

    private Vector2 lastPos;
    private Vector2 velocity;
    private bool isThrown = false;
    private float lifeTimer = 0f;
    [ReadOnly] [SerializeField] private bool isIntruder = false;

    [Header("Debug Intrus")]
    [SerializeField] private bool showIntruderDebug = false;
    [SerializeField] private Image debugImage; // une image à colorer (UI)
    [SerializeField] private Color intruderColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f);

    public static event System.Action<FoodConveyorItemUI, bool> OnAnyDestroyed;


    protected override void Awake()
    {
        base.Awake();
        lastPos = rect.anchoredPosition;
        UpdateDebugColor();
    }

    private void Update()
    {
        if (isThrown)
        {
            rect.anchoredPosition += velocity * Time.deltaTime;
            velocity *= throwDecay;
            lifeTimer += Time.deltaTime;

            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, rect.position, null) || lifeTimer > maxLifetime)
            {
                NotifyDestroyed();
                Destroy(gameObject);
            }
            return;
        }

        if (!isMoving) return;

        rect.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

        if (endPoint != null && rect.position.y < endPoint.position.y)
        {
            NotifyDestroyed();
            Destroy(gameObject);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        isMoving = false;
        isThrown = false;
        velocity = Vector2.zero;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        Vector2 currentPos = rect.anchoredPosition;
        velocity = (currentPos - lastPos) / Time.deltaTime;
        lastPos = currentPos;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (velocity.magnitude > throwSpeedThreshold)
        {
            isThrown = true;
            lifeTimer = 0f;

            DisableTitre();
            OnAnyThrown?.Invoke(this);

            bool isCorrect = isIntruder; // ✔️ si c’est un intrus → bonne réponse
            FeedbackSpawner.Instance.SpawnFeedbackAtRect(rect, isCorrect);
            ScoreManager.Instance.EnregistrerRecyclingAnswer(isCorrect);

            // ⚠️ On ne détruit pas tout de suite (il "vole"), mais on considérera que c’est fini pour la question
            NotifyDestroyed();
        }
        else
        {
            if (!isBeingKilled)
            {
                base.OnEndDrag(eventData);

                if (transform.parent == originalParent)
                    isMoving = true;
            }
        }
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetEndPoint(RectTransform target) => endPoint = target;

    public void StopMovement() => isMoving = false;

    public void SetAsIntruder(bool value)
    {
        isIntruder = value;
        UpdateDebugColor();
    }

    public void PlayCollectedAnimation()
    {
        MarkAsKilled();
        StopMovement();

        rect.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                NotifyDestroyed();
                Destroy(gameObject);
            });
    }

    public void PlayRejectedAnimation()
    {
        StopMovement();
        rect.DOShakePosition(0.3f, 10f, 20, 90f)
            .OnComplete(() => isMoving = true);
    }

    private void MarkAsKilled()
    {
        isBeingKilled = true;
    }

    private void DisableTitre()
    {
        if (titre != null)
            titre.gameObject.SetActive(false);
    }

    public bool IsIntruder() => isIntruder;

    private void UpdateDebugColor()
    {
        if (debugImage != null && showIntruderDebug)
        {
            debugImage.color = isIntruder ? intruderColor : normalColor;
        }
    }

    private void OnDestroy()
    {
        OnAnyDestroyed?.Invoke(this, isIntruder);
    }

    // === 🔔 Notify Question quand détruit ===
    private void NotifyDestroyed()
    {
        OnItemDestroyed?.Invoke(this, isIntruder);
    }
}