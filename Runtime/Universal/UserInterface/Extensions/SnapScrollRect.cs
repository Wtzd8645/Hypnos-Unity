using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hypnos.UI
{
    public class SnapScrollRect : ScrollRect
    {
        public enum SwipeDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        public float PageSize { get { return pageSize; } set { pageSize = value; } }
        public float PageSpace { get { return pageSpace; } set { pageSpace = value; } }
        public int TotalPageIndex { get { return totalPageIndex; } }
        public int CurrentPageIndex { get { return currentPageIndex; } }

        [SerializeField] protected SwipeDirection swipeDirection;
        [SerializeField] protected bool isAutoCalculateSize = true;
        [SerializeField] protected float pageSize;
        [SerializeField] protected float pageSpace;
        [SerializeField] protected int totalPageIndex;
        [SerializeField] protected float swipeThreshold = 10f;
        [SerializeField] protected float swipeSpeed = 10f;

        protected int currentPageIndex;
        protected float targetPos;

        protected Vector2 startDragPos;
        protected bool isDragging;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            CalculateContentSize();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            horizontal = swipeDirection == SwipeDirection.Left || swipeDirection == SwipeDirection.Right;
            vertical = swipeDirection == SwipeDirection.Up || swipeDirection == SwipeDirection.Down;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (isAutoCalculateSize)
            {
                CalculateContentSize();
            }

            ResetPosition();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (isDragging)
            {
                return;
            }

            switch (swipeDirection)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Right:
                {
                    content.anchoredPosition = new Vector2(Mathf.Lerp(content.anchoredPosition.x, targetPos, Time.unscaledDeltaTime * swipeSpeed), content.anchoredPosition.y);
                    break;
                }
                case SwipeDirection.Up:
                case SwipeDirection.Down:
                {
                    content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Lerp(content.anchoredPosition.y, targetPos, Time.unscaledDeltaTime * swipeSpeed));
                    break;
                }
            }
        }

        public virtual void CalculateContentSize()
        {
            if (content.TryGetComponent(out HorizontalOrVerticalLayoutGroup layoutGroup))
            {
                pageSpace = layoutGroup.spacing;
            }

            int childCount = content.childCount;
            for (int i = childCount - 1; i >= 0; --i)
            {
                if (!content.GetChild(i).gameObject.activeSelf)
                {
                    --childCount;
                }
            }

            if (childCount > 0)
            {
                RectTransform rt = content.GetChild(0) as RectTransform;
                switch (swipeDirection)
                {
                    case SwipeDirection.Left:
                    case SwipeDirection.Right:
                    {
                        pageSize = rt.rect.width + pageSpace;
                        break;
                    }
                    case SwipeDirection.Up:
                    case SwipeDirection.Down:
                    {
                        pageSize = rt.rect.height + pageSpace;
                        break;
                    }
                }
            }
            totalPageIndex = childCount > 0 ? childCount - 1 : 0;
        }

        public virtual void ResetPosition()
        {
            currentPageIndex = 0;
            switch (swipeDirection)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Up:
                {
                    targetPos = 0f;
                    break;
                }
                case SwipeDirection.Right:
                {
                    targetPos = -totalPageIndex * pageSize;
                    break;
                }
                case SwipeDirection.Down:
                {
                    targetPos = totalPageIndex * pageSize;
                    break;
                }
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            isDragging = true;
            startDragPos = eventData.position;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            StopMovement();
            base.OnEndDrag(eventData);
            switch (swipeDirection)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Right:
                {
                    SwipeTest(eventData.position.x - startDragPos.x);
                    break;
                }
                case SwipeDirection.Up:
                case SwipeDirection.Down:
                {
                    SwipeTest(eventData.position.y - startDragPos.y);
                    break;
                }
            }
            isDragging = false;
        }

        protected virtual void SwipeTest(float delta)
        {
            if (Mathf.Abs(delta) < swipeThreshold)
            {
                return;
            }

            switch (swipeDirection)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Down:
                {
                    if (delta > 0f)
                    {
                        Previous();
                    }
                    else
                    {
                        Next();
                    }
                    break;
                }
                case SwipeDirection.Right:
                case SwipeDirection.Up:
                {
                    if (delta > 0f)
                    {
                        Next();
                    }
                    else
                    {
                        Previous();
                    }
                    break;
                }
            }
        }

        public virtual void Previous()
        {
            if (currentPageIndex != 0)
            {
                --currentPageIndex;
                TurnPage();
            }
        }

        public virtual void Next()
        {
            if (currentPageIndex != totalPageIndex)
            {
                ++currentPageIndex;
                TurnPage();
            }
        }

        protected void TurnPage()
        {
            switch (swipeDirection)
            {
                case SwipeDirection.Left:
                {
                    targetPos = -currentPageIndex * pageSize;
                    break;
                }
                case SwipeDirection.Right:
                {
                    targetPos = (currentPageIndex - totalPageIndex) * pageSize;
                    break;
                }
                case SwipeDirection.Up:
                {
                    targetPos = currentPageIndex * pageSize;
                    break;
                }
                case SwipeDirection.Down:
                {
                    targetPos = (totalPageIndex - currentPageIndex) * pageSize;
                    break;
                }
            }
        }
    }
}