using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FoodDraggableUI : FoodItemBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event System.Action<FoodDraggableUI> OnAnyBeginDrag;
    public static event System.Action<FoodDraggableUI> OnAnyEndDrag;

    [SerializeField] protected Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    
    [SerializeField] private Button removeButton;
    [SerializeField] private int currentIndex = 0;

    private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] protected RectTransform rect;
    private Vector2 startPos;
    private Transform startParent;
    private Vector2 pointerOffset;
    private bool wasDropped;

    // Fallback si pas de dock assigné
    private Vector2 originalPos;
    protected Transform originalParent;

    private static Transform dragLayer; // ✅ Global
    private static bool dragLayerChecked = false;

    // Dock d’origine + index (pour un reset avec tween)
    private InitialDockUI originalDock;
    private int originalDockIndex = -1;

    // Dropzone courante
    private DropZoneUI currentDropZone;

    private FoodData food;
    private PortionSelection portion;

    protected bool isBeingKilled = false;

    protected virtual void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas != null) canvas = canvas.rootCanvas;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // ✅ Trouver le DragLayer global une seule fois
        if (!dragLayerChecked)
        {
            dragLayerChecked = true;
            if (canvas != null)
            {
                Transform found = canvas.rootCanvas.transform.Find("DragLayer");
                if (found != null)
                    dragLayer = found;
                else
                    Debug.LogWarning("⚠️ Aucun DragLayer trouvé dans le Canvas root !");
            }
        }
    }

    public void PlaySpawnAnimation(float delay = 0f)
    {
        rect.localScale = Vector3.zero;
        rect.DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);
    }

    public virtual void Init(FoodData f, PortionSelection sel, QuestionSubType questionSubType, int index)
    {
        food = f;
        currentIndex = index;
        nameText.text = PortionTextFormatter.ToDisplayWithFood(food, sel);
        icon.sprite = SpriteLoader.LoadFoodSprite(f.Name);

        base.UpdateHintInfo(food, questionSubType);
    }

    // Fallback seulement si pas de dock d’origine
    public void CacheOriginalTransform()
    {
        originalParent = rect.parent;
        originalPos = rect.anchoredPosition;
    }

    // À appeler au spawn pour enregistrer le dock et l’index initial
    public void SetOriginalDock(InitialDockUI dock, int index)
    {
        originalDock = dock;
        originalDockIndex = index;
    }

    public void NotifyDropped() => wasDropped = true;

    public void SetCurrentDropZone(DropZoneUI dz)
    {
        currentDropZone = dz;
        if (removeButton) removeButton.gameObject.SetActive(dz != null);
    }

    public void Btn_Remove()
    {
        if (currentDropZone != null)
            currentDropZone.RemoveItem(this);
        else
            ResetToOriginal(true);
    }

    public void ResetToOriginal(bool tween)
    {
        if (isBeingKilled) return;

        rect.DOKill(false);

        if (originalDock != null && originalDockIndex >= 0)
        {
            originalDock.PlaceAtIndex(this, originalDockIndex, tween);
        }
        else
        {
            rect.SetParent(originalParent, worldPositionStays: false);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;

            if (tween) rect.DOAnchorPos(originalPos, 0.25f).SetEase(Ease.OutBack);
            else rect.anchoredPosition = originalPos;
        }

        SetCurrentDropZone(null);
    }

    private void OnTransformParentChanged()
    {
        SetCurrentDropZone(GetComponentInParent<DropZoneUI>());
    }

    // ---------- Drag & Drop ----------
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        wasDropped = false;
        startPos = rect.anchoredPosition;
        startParent = rect.parent;

        if (canvasGroup) canvasGroup.blocksRaycasts = false;

        // ✅ Déplacer dans le DragLayer global (toujours tout devant)
        if (dragLayer != null)
            transform.SetParent(dragLayer, true);

        Vector2 localPoint;
        RectTransform canvasRT = canvas.transform as RectTransform;
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? eventData.pressEventCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out localPoint))
            pointerOffset = rect.anchoredPosition - localPoint;
        else
            pointerOffset = Vector2.zero;

        OnAnyBeginDrag?.Invoke(this);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 localPoint;
        RectTransform canvasRT = canvas.transform as RectTransform;
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? eventData.pressEventCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out localPoint))
            rect.anchoredPosition = localPoint + pointerOffset;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup) canvasGroup.blocksRaycasts = true;
        OnAnyEndDrag?.Invoke(this);
        StartCoroutine(EndDragDeferred());
    }

    private System.Collections.IEnumerator EndDragDeferred()
    {
        yield return null;
        if (wasDropped) { wasDropped = false; yield break; }
        if (transform.parent != startParent) yield break;
        rect.DOAnchorPos(startPos, 0.25f).SetEase(Ease.OutBack);
    }

    public void MarkAsKilled() => isBeingKilled = true;

    // ---------- Getters ----------
    public FoodData GetFood() => food;
    public int GetIndex() => currentIndex;
}