using UnityEngine;
using UnityEngine.EventSystems;

namespace RetroCat.Modules.RoomBox
{
    public class UISticker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Drag Settings")]
        [SerializeField] private float dragSensitivity = 1f;

        private bool _isDragging = false;
        private Vector2 _dragOffset;
        private RectTransform _rectTransform;
        private StickerData _stickerData;
        private Vector2 _originalPosition;
        private RectTransform _originalParent;
        private Canvas _canvas;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = _rectTransform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
        }
        
        public void SetStickerData(StickerData stickerData)
        {
            _stickerData = stickerData;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
            _isDragging = true;

            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = _rectTransform.parent as RectTransform;

            // Переносим стикер на канвас, чтобы ScrollView не перехватывал события
            if (_canvas != null)
                _rectTransform.SetParent(_canvas.transform, true);

            // Конвертируем позицию указателя в координаты канваса
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);

            _dragOffset = _rectTransform.localPosition - (Vector3)localPoint;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("OnPointerUp");
            if (_isDragging)
            {
                _isDragging = false;
                // Возвращаем родителя и позицию
                if (_originalParent != null)
                    _rectTransform.SetParent(_originalParent, true);

                ReturnToOriginalPosition();
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                // Конвертируем позицию указателя в локальные координаты канваса
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint);

                // Применяем смещение и обновляем позицию
                Vector3 newPosition = localPoint + _dragOffset;
                _rectTransform.localPosition = newPosition;
            }
        }
        
        private void ReturnToOriginalPosition()
        {
            _rectTransform.anchoredPosition = _originalPosition;
        }
        
        private void Update()
        {
            if (_isDragging)
            {
                // Дополнительная обработка для более плавного движения
                // Можно добавить дополнительные эффекты здесь
            }
        }
    }
}
