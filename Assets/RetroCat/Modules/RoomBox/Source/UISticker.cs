using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace RetroCat.Modules.RoomBox
{
    public class UISticker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private float returnDuration = 0.3f;
        [SerializeField] private Ease returnEase = Ease.OutBack;
        [SerializeField] private float scaleOnDrag = 1.1f;
        [SerializeField] private float scaleDuration = 0.15f;
        [SerializeField] private Ease scaleEase = Ease.OutQuad;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image _stickerImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        // События для уведомления о перетаскивании
        public event Action<UISticker> OnDragStarted;
        public event Action<UISticker> OnDragEnded;
        
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        private RectTransform _rectTransform;
        private StickerData _stickerData;
        private Vector2 _originalPosition;
        private RectTransform _originalParent;
        private Canvas _canvas;
        private Vector3 _originalScale;
        private Sequence _currentAnimation;

        public StickerData StickerData => _stickerData;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = _rectTransform.parent as RectTransform;
            _originalScale = _rectTransform.localScale;
            _canvas = FindAnyObjectByType(typeof(Canvas)) as Canvas;
            
            // Получаем CanvasGroup если его нет
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Initialize(StickerData stickerData)
        {
            _stickerData = stickerData;
            _stickerImage.sprite = stickerData.Sprite;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
            _isDragging = true;

            // Останавливаем предыдущую анимацию
            _currentAnimation?.Kill();
            
            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = _rectTransform.parent as RectTransform;

            if (_canvas != null)
                _rectTransform.SetParent(_canvas.transform, true);

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);

            _dragOffset = _rectTransform.localPosition - (Vector3)localPoint;
            
            // Анимация увеличения при начале перетаскивания
            AnimateScaleOnDrag(true);
            
            // Уведомляем о начале перетаскивания
            OnDragStarted?.Invoke(this);
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
                
                // Уведомляем о конце перетаскивания
                OnDragEnded?.Invoke(this);
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
            // Останавливаем предыдущую анимацию
            _currentAnimation?.Kill();
            
            // Создаем последовательность анимаций
            _currentAnimation = DOTween.Sequence();
            
            // Анимация возврата к оригинальному размеру
            _currentAnimation.Join(_rectTransform.DOScale(_originalScale, scaleDuration)
                .SetEase(scaleEase));
            
            // Анимация возврата к оригинальной позиции
            _currentAnimation.Join(_rectTransform.DOAnchorPos(_originalPosition, returnDuration)
                .SetEase(returnEase));
            
            // Дополнительный эффект прозрачности
            if (_canvasGroup != null)
            {
                _currentAnimation.Join(_canvasGroup.DOFade(1f, returnDuration * 0.5f)
                    .SetEase(Ease.OutQuad));
            }
            
            // Добавляем небольшой эффект отскока
            _currentAnimation.OnComplete(() => {
                // Небольшая анимация отскока
                _rectTransform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 3, 0.5f)
                    .SetEase(Ease.OutElastic);
            });
        }
        
        private void AnimateScaleOnDrag(bool isDragging)
        {
            Vector3 targetScale = isDragging ? _originalScale * scaleOnDrag : _originalScale;
            
            _rectTransform.DOScale(targetScale, scaleDuration)
                .SetEase(scaleEase);
        }
        
        private void OnDestroy()
        {
            // Очищаем анимации при уничтожении объекта
            _currentAnimation?.Kill();
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
