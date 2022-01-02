using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace rwby
{
    public class PlayerPointerEventTrigger : MonoBehaviour, IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        IPointerDownHandler,
        IPointerClickHandler
        /*IScrollHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler*/
    {
        public UnityEvent<PlayerPointerEventData> OnPointerEnterEvent;
        public UnityEvent<PlayerPointerEventData> OnPointerExitEvent;
        public UnityEvent<PlayerPointerEventData> OnPointerUp;
        public UnityEvent<PlayerPointerEventData> OnPointerDown;
        public UnityEvent<PlayerPointerEventData> OnPointerClickEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData is PlayerPointerEventData)
            {
                PlayerPointerEventData playerEventData = (PlayerPointerEventData)eventData;
                OnPointerClickEvent?.Invoke(playerEventData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData is PlayerPointerEventData)
            {
                PlayerPointerEventData playerEventData = (PlayerPointerEventData)eventData;
                OnPointerEnterEvent?.Invoke(playerEventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData is PlayerPointerEventData)
            {
                PlayerPointerEventData playerEventData = (PlayerPointerEventData)eventData;
                OnPointerExitEvent?.Invoke(playerEventData); 
            }
        }

        private static string GetSourceName(PlayerPointerEventData playerEventData)
        {
            if (playerEventData.sourceType == PointerEventType.Mouse)
            {
                if (playerEventData.mouseSource is Behaviour) return (playerEventData.mouseSource as Behaviour).name;
            }
            else if (playerEventData.sourceType == PointerEventType.Touch)
            {
                if (playerEventData.touchSource is Behaviour) return (playerEventData.touchSource as Behaviour).name;
            }
            return null;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {

        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {

        }

        /*
        public void OnBeginDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnScroll(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }*/
    }
}