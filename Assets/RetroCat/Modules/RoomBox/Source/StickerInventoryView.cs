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
        [SerializeField] private Vector2 itemSize = new Vector2(100f, 100f);

        [Header("Animation Settings")]
        [SerializeField] private float shiftDuration = 0.3f;
        [SerializeField] private Ease shiftEase = Ease.OutBack;
        [SerializeField] private float collapsedOffset = 0f;

        private List<UISticker> _spawnedStickers = new List<UISticker>();
        private bool _isExpanded = false;
        private Sequence _currentShiftAnimation;
        private UISticker _currentDraggedSticker;

        public bool IsExpanded => _isExpanded;

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
            SetExpandedState(true);
        }

        public void OnStickerDragEnded(UISticker sticker)
        {
            SpawnWorldSticker(sticker);
            _currentDraggedSticker = null;
            SetExpandedState(false);
        }

        private void SpawnWorldSticker(UISticker sticker)
        {
            if (sticker == null || Camera.main == null)
                return;

            var data = sticker.StickerData;
            if (data == null)
                return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;

            var obj = new GameObject(data.name);
            obj.transform.position = worldPos;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = data.Sprite;
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
