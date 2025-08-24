using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FoodDraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event System.Action<FoodDraggableUI> OnAnyBeginDrag;
    public static event System.Action<FoodDraggableUI> OnAnyEndDrag;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI gramsText;
    [SerializeField] private Button removeButton;

    private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform rect;
    private Vector2 startPos;
    private Transform startParent;
    private Vector2 pointerOffset;
    private bool wasDropped;

    // Fallback si pas de dock assigné
    private Vector2 originalPos;
    private Transform originalParent;

    // Dock d’origine + index (pour un reset avec tween)
    private InitialDockUI originalDock;
    private int originalDockIndex = -1;

    // Dropzone courante
    private DropZoneUI currentDropZone;

    private FoodData food;
    private PortionSelection portion;
    private float grams;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas != null) canvas = canvas.rootCanvas;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(Btn_Remove);
            removeButton.gameObject.SetActive(false);
        }
    }
    public void PlaySpawnAnimation(float delay = 0f)
    {
        rect.localScale = Vector3.zero;
        rect.DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);
    }
    public void Init(FoodData f, PortionSelection sel)
    {
        food = f;
        grams = 100f;

        nameText.text = PortionTextFormatter.ToDisplayWithFood(food, sel);

        if (gramsText) gramsText.text = $"{Mathf.RoundToInt(grams)} g";
        icon.sprite = FoodSpriteLoader.LoadFoodSprite(f.Name);
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

    public void NotifyDropped() { wasDropped = true; }

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
        rect.DOKill(false);

        // Utilise le dock d’origine si disponible (tween garanti via PlaceAtIndex)
        if (originalDock != null && originalDockIndex >= 0)
        {
            originalDock.PlaceAtIndex(this, originalDockIndex, tween);
        }
        else
        {
            // Fallback : parent/pos d’origine
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
    public void OnBeginDrag(PointerEventData eventData)
    {
        wasDropped = false;
        startPos = rect.anchoredPosition;
        startParent = rect.parent;

        if (canvasGroup) canvasGroup.blocksRaycasts = false;
        rect.SetAsLastSibling();

        Vector2 localPoint;
        RectTransform canvasRT = canvas.transform as RectTransform;
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? eventData.pressEventCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out localPoint))
            pointerOffset = rect.anchoredPosition - localPoint;
        else
            pointerOffset = Vector2.zero;


        OnAnyBeginDrag?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 localPoint;
        RectTransform canvasRT = canvas.transform as RectTransform;
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? eventData.pressEventCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out localPoint))
            rect.anchoredPosition = localPoint + pointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
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

    // ---------- Getters ----------
    public FoodData GetFood() => food;
    public float GetGrams() => grams;
}