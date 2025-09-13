using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using NaughtyAttributes;

public class FoodConveyorItemUI : FoodDraggableUI
{
    public static event System.Action<FoodConveyorItemUI> OnAnyThrown;
    public event System.Action<FoodConveyorItemUI, bool> OnItemDestroyed;

    [Header("Conveyor Settings")]
    [SerializeField] private RectTransform titre;
    private bool isMoving = true;

    [Header("Throw Settings")]
    [SerializeField] private float throwSpeedThreshold = 1200f;
    [SerializeField] private float throwDecay = 0.98f;
    [SerializeField] private float maxLifetime = 5f;

    private Vector2 lastPos;
    private Vector2 velocity;
    private bool isThrown = false;
    private float lifeTimer = 0f;
    [ReadOnly][SerializeField] private bool isIntruder = false;

    private Transform conveyorParent;
    private Transform dragParent;

    [Header("Debug Intrus")]
    [SerializeField] private bool showIntruderDebug = false;
    [SerializeField] private Image debugImage;
    [SerializeField] private Color intruderColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f);

    public static event System.Action<FoodConveyorItemUI, bool> OnAnyDestroyed;
    public static event System.Action<FoodConveyorItemUI> OnAnyBeginDragConveyor;
    public static event System.Action<FoodConveyorItemUI> OnAnyEndDragConveyor;

    protected override void Awake()
    {
        base.Awake();
        lastPos = rect.anchoredPosition;

        conveyorParent = transform.parent;   // le slot/tapis d’origine
        dragParent = conveyorParent.parent;  // parent global (canvas, etc.)

        UpdateDebugColor();
    }
    public override void Init(FoodData f, PortionSelection sel, int index)
    {
        base.Init(f, sel, index);

        rect.anchoredPosition = Vector2.zero;
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
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(dragParent, true);
        base.OnBeginDrag(eventData);
        isMoving = false;
        isThrown = false;
        velocity = Vector2.zero;

        // 🚀 Notifie début de drag
        OnAnyBeginDragConveyor?.Invoke(this);
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
        // 🚀 Notifie fin de drag
        OnAnyEndDragConveyor?.Invoke(this);

        if (velocity.magnitude > throwSpeedThreshold)
        {
            isThrown = true;
            lifeTimer = 0f;

            DisableTitre();
            OnAnyThrown?.Invoke(this);

            bool isCorrect = isIntruder;
            FeedbackSpawner.Instance.SpawnFeedbackAtRect(rect, isCorrect);
            ScoreManager.Instance.EnregistrerRecyclingAnswer(isCorrect);

            NotifyDestroyed();
        }
        else
        {
            if (!isBeingKilled)
            {
                base.OnEndDrag(eventData);

                // ✅ si reset → on remet dans le slot/conveyor
                if (transform.parent == dragParent)
                {
                    transform.SetParent(conveyorParent, true); // true = conserve la position monde
                    // tween fluide vers (0,0) dans le slot
                    rect.DOKill(); // kill tweens en cours pour éviter les conflits
                    rect.DOLocalMove(Vector3.zero, 0.25f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => isMoving = true);
                }
            }
        }
    }



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
        MarkAsKilled();
        StopMovement();
        DisableTitre();

        float jumpPower = Random.Range(600f, 1000f);
        float duration = 0.85f;
        float randomRot = Random.Range(-360f, 360f);

        // Force un minimum de déplacement horizontal (au moins 80px à gauche/droite)
        float offsetX = Random.Range(0, 2) == 0
            ? Random.Range(-700f, -200f)   // toujours bien à gauche
            : Random.Range(200f, 700f);    // toujours bien à droite

        float offsetY = Random.Range(120f, 200f);
        Vector2 randomOffset = new Vector2(offsetX, offsetY);

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOJumpAnchorPos(rect.anchoredPosition + randomOffset, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad));
        seq.Join(rect.DOScale(0.6f, 0.45f).SetEase(Ease.OutBack));
        seq.Join(rect.DORotate(new Vector3(0, 0, randomRot), duration, RotateMode.FastBeyond360));
        seq.Join(icon.DOFade(0.4f, duration));

        seq.OnComplete(() =>
        {
            OnDestroy();
            Destroy(gameObject);
        });
    }

    private void MarkAsKilled()
    {
        isBeingKilled = true;
    }

    public void DisableTitre()
    {
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

    private void NotifyDestroyed()
    {
        OnItemDestroyed?.Invoke(this, isIntruder);
    }
}