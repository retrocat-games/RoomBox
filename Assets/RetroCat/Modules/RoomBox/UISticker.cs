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
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
        }
        
        public void SetStickerData(StickerData stickerData)
        {
            _stickerData = stickerData;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
            _isDragging = true;
            
            // Конвертируем позицию мыши в локальные координаты канваса
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                eventData.position,
                Camera.main, 
                out localPoint);
            
            _dragOffset = _rectTransform.anchoredPosition - localPoint;
            _originalPosition = _rectTransform.anchoredPosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("OnPointerUp");
            if (_isDragging)
            {
                _isDragging = false;
                ReturnToOriginalPosition();
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                // Конвертируем позицию мыши в локальные координаты канваса
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    eventData.position,
                    Camera.main,
                    out localPoint);
                
                // Применяем смещение и обновляем позицию
                Vector2 newPosition = localPoint + _dragOffset;
                _rectTransform.anchoredPosition = newPosition;
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
