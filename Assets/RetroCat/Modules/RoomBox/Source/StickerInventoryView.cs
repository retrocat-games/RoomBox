using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace RetroCat.Modules.RoomBox
{
    // Интерфейс для наблюдателя событий перетаскивания
    public interface IStickerDragObserver
    {
        void OnStickerDragStarted(UISticker sticker);
        void OnStickerDragEnded(UISticker sticker);
    }

    // Интерфейс для анимации контента
    public interface IContentAnimator
    {
        void AnimateContentShift(bool isExpanded, float duration);
        void SetExpandedState(bool isExpanded);
    }

    // Интерфейс для управления состоянием инвентаря
    public interface IInventoryStateManager
    {
        bool IsExpanded { get; }
        void SetExpandedState(bool isExpanded);
    }

    public class StickerInventoryView : MonoBehaviour, IStickerDragObserver, IContentAnimator, IInventoryStateManager
    {
        [Header("Inventory Settings")]
        [SerializeField] private StickerInventory inventory;
        [SerializeField] private RectTransform content;
        [SerializeField] private UISticker _stickerPrefab;
        [SerializeField] private WorldSticker _worldStickerPrefab;
        [SerializeField] private Vector2 itemSize = new Vector2(100f, 100f);

        [Header("Animation Settings")]
        [SerializeField] private float shiftDuration = 0.3f;
        [SerializeField] private Ease shiftEase = Ease.OutBack;
        [SerializeField] private float collapsedOffset = 0f;

        private List<UISticker> _spawnedStickers = new List<UISticker>();
        private bool _isExpanded = false;
        private Sequence _currentShiftAnimation;
        private UISticker _currentDraggedSticker;
        private int _currentSortingOrder = 0;
        private IWorldStickerFactory _worldStickerFactory;
        private RectTransform _scrollViewRect;
        private Camera _uiCamera;
        private Canvas _canvas;

        [Header("Drag Outside Settings")]
        [SerializeField] private float outsideScaleDuration = 0.2f;
        private bool _isPointerOutside;
        private Vector2 _draggedStickerOriginalSize;
        private Tweener _currentSizeTween;

        public bool IsExpanded => _isExpanded;

        private void Awake()
        {
            _worldStickerFactory = new WorldStickerFactory(_worldStickerPrefab);
            var scrollRect = content != null ? content.GetComponentInParent<ScrollRect>() : null;
            if (scrollRect != null)
            {
                _scrollViewRect = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
                var canvas = scrollRect.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    _uiCamera = canvas.worldCamera;
                    _canvas = canvas;
                }
            }
        }

        private void Start()
        {
            Load();
        }

        public void Load()
        {
            if (inventory == null || content == null)
                return;

            ClearExistingStickers();

            for (int i = 0; i < inventory.stickers.Count; i++)
            {
                StickerData stickerData = inventory.stickers[i];
                UISticker item = CreateStickerItem(stickerData, i);
                _spawnedStickers.Add(item);
            }

            UpdateContentSize();
        }

        private void ClearExistingStickers()
        {
            foreach (var sticker in _spawnedStickers)
            {
                if (sticker != null)
                {
                    // Отписываемся от событий
                    sticker.OnDragStarted -= OnStickerDragStarted;
                    sticker.OnDragEnded -= OnStickerDragEnded;
                    Destroy(sticker.gameObject);
                }
            }
            _spawnedStickers.Clear();
        }

        private UISticker CreateStickerItem(StickerData stickerData, int index)
        {
            UISticker item = Instantiate(_stickerPrefab);
            item.Initialize(stickerData);
            
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            rect.sizeDelta = itemSize;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(index * itemSize.x, 0f);

            var image = item.GetComponent<Image>();
            image.sprite = stickerData.Sprite;

            // Подписываемся на события перетаскивания
            item.OnDragStarted += OnStickerDragStarted;
            item.OnDragEnded += OnStickerDragEnded;

            return item;
        }

        private void UpdateContentSize()
        {
            if (content != null)
            {
                var size = content.sizeDelta;
                size.x = inventory.stickers.Count * itemSize.x;
                content.sizeDelta = size;
            }
        }

        public void OnStickerDragStarted(UISticker sticker)
        {
            _currentDraggedSticker = sticker;
            if (sticker != null)
            {
                var rect = sticker.GetComponent<RectTransform>();
                _draggedStickerOriginalSize = rect.sizeDelta;
                _isPointerOutside = IsPointerOutsideScrollView();
            }
            SetExpandedState(true);
        }

        public void OnStickerDragEnded(UISticker sticker)
        {
            if (IsPointerOutsideScrollView())
            {
                SpawnWorldSticker(sticker);
            }
            _currentDraggedSticker = null;
            _currentSizeTween?.Kill();
            SetExpandedState(false);
        }

        private bool IsPointerOutsideScrollView()
        {
            if (_scrollViewRect == null)
                return true;
            return !RectTransformUtility.RectangleContainsScreenPoint(_scrollViewRect, Input.mousePosition, _uiCamera);
        }

        private void SpawnWorldSticker(UISticker sticker)
        {
            if (sticker == null || Camera.main == null)
                return;

            var data = sticker.StickerData;
            if (data == null)
                return;
            
            _worldStickerFactory?.Create(GetStickerWorldPosition(sticker), 
                data,
                ++_currentSortingOrder);

            if (inventory != null)
                inventory.RemoveSticker(data);

            sticker.OnDragStarted -= OnStickerDragStarted;
            sticker.OnDragEnded -= OnStickerDragEnded;
            _spawnedStickers.Remove(sticker);
            Destroy(sticker.gameObject);

            UpdateContentSize();
        }
        
        private Vector3 GetStickerWorldPosition(UISticker sticker)
        {
            Vector3[] corners = new Vector3[4];
            sticker.RectTransform.GetWorldCorners(corners);
            Vector3 screenCenter = (corners[0] + corners[2]) / 2f;

            screenCenter.z = Mathf.Abs(Camera.main.transform.position.z);

            // Переводим из экранной в мировую
            return Camera.main.ScreenToWorldPoint(screenCenter);
        }


        private void Update()
        {
            if (_currentDraggedSticker == null)
                return;

            bool outside = IsPointerOutsideScrollView();
            if (outside != _isPointerOutside)
            {
                _isPointerOutside = outside;
                if (outside)
                    AnimateStickerToWorldSize(_currentDraggedSticker);
                else
                    AnimateStickerToInventorySize(_currentDraggedSticker);
            }
        }

        private void AnimateStickerToWorldSize(UISticker sticker)
        {
            if (sticker == null) return;

            var rect = sticker.GetComponent<RectTransform>();
            var sprite = sticker.StickerData != null ? sticker.StickerData.Sprite : null;
            Vector2 target = _draggedStickerOriginalSize;

            if (sprite != null)
            {
                target = GetWorldStickerScreenSize(sprite);
            }

            float scale = sticker.transform.localScale.x;
            float canvasScale = _canvas != null ? _canvas.scaleFactor : 1f;
            _currentSizeTween?.Kill();
            _currentSizeTween = rect.DOSizeDelta(target / (scale * canvasScale), outsideScaleDuration)
                .SetEase(Ease.OutQuad);
        }

        private void AnimateStickerToInventorySize(UISticker sticker)
        {
            if (sticker == null) return;

            var rect = sticker.GetComponent<RectTransform>();
            _currentSizeTween?.Kill();
            _currentSizeTween = rect.DOSizeDelta(_draggedStickerOriginalSize, outsideScaleDuration)
                .SetEase(Ease.OutQuad);
        }

        private Vector2 GetWorldStickerScreenSize(Sprite sprite)
        {
            if (sprite == null || Camera.main == null)
                return _draggedStickerOriginalSize;

            Vector2 worldSize = sprite.bounds.size;
            var camera = Camera.main;
            Vector3 origin = Vector3.zero;
            Vector3 screenOrigin = camera.WorldToScreenPoint(origin);
            Vector3 screenX = camera.WorldToScreenPoint(origin + new Vector3(worldSize.x, 0f, 0f));
            Vector3 screenY = camera.WorldToScreenPoint(origin + new Vector3(0f, worldSize.y, 0f));

            float width = Mathf.Abs(screenX.x - screenOrigin.x);
            float height = Mathf.Abs(screenY.y - screenOrigin.y);
            return new Vector2(width, height);
        }

        public void SetExpandedState(bool isExpanded)
        {
            if (_isExpanded != isExpanded)
            {
                _isExpanded = isExpanded;
                AnimateContentShift(isExpanded, shiftDuration);
            }
        }

        public void AnimateContentShift(bool isExpanded, float duration)
        {
            // Останавливаем предыдущую анимацию
            _currentShiftAnimation?.Kill();

            _currentShiftAnimation = DOTween.Sequence();

            if (isExpanded)
            {
                AnimateStickersOnExpand(duration);
            }
            else
            {
                AnimateStickersOnCollapse(duration);
            }
        }

        private void AnimateStickersOnExpand(float duration)
        {
            if (_currentDraggedSticker == null) return;

            // Находим индекс перетаскиваемого стикера
            int draggedIndex = _spawnedStickers.IndexOf(_currentDraggedSticker);
            if (draggedIndex == -1) return;

            // Анимируем только стикеры справа от перетаскиваемого
            for (int i = 0; i < _spawnedStickers.Count; i++)
            {
                var sticker = _spawnedStickers[i];
                if (sticker != null && sticker != _currentDraggedSticker)
                {
                    if (i > draggedIndex)
                    {
                        // Стикеры справа от перетаскиваемого - сдвигаем влево на размер одного стикера
                        Vector2 currentPos = sticker.GetComponent<RectTransform>().anchoredPosition;
                        Vector2 targetPos = new Vector2(currentPos.x - itemSize.x, currentPos.y);
                        
                        // Проверяем, чтобы не уйти в отрицательные координаты
                        targetPos.x = Mathf.Max(targetPos.x, 0f);
                        
                        sticker.GetComponent<RectTransform>().DOAnchorPos(targetPos, duration)
                            .SetEase(shiftEase);
                        
                        // Небольшое уменьшение размера
                        sticker.transform.DOScale(Vector3.one * 0.9f, duration * 0.5f)
                            .SetEase(Ease.OutQuad);
                    }
                    else
                    {
                        // Стикеры слева - небольшое уменьшение для визуального эффекта
                        sticker.transform.DOScale(Vector3.one * 0.95f, duration * 0.5f)
                            .SetEase(Ease.OutQuad);
                    }
                }
            }
        }

        private void AnimateStickersOnCollapse(float duration)
        {
            // Возвращаем все стикеры в исходное положение
            for (int i = 0; i < _spawnedStickers.Count; i++)
            {
                var sticker = _spawnedStickers[i];
                if (sticker != null)
                {
                    // Возврат к оригинальной позиции
                    Vector2 originalPos = new Vector2(i * itemSize.x, 0f);
                    sticker.GetComponent<RectTransform>().DOAnchorPos(originalPos, duration)
                        .SetEase(shiftEase);
                    
                    // Возврат к нормальному размеру
                    sticker.transform.DOScale(Vector3.one, duration * 0.5f)
                        .SetEase(Ease.OutBack);
                }
            }
        }

        private void OnDestroy()
        {
            // Очищаем анимации при уничтожении
            _currentShiftAnimation?.Kill();
        }
    }
}
