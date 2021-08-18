using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace rwby
{
    public static class EventTriggerHelpers
    {
        public static void AddListener(this EventTrigger trigger, EventTriggerType eventType, UnityAction<BaseEventData> call)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener(call);
            trigger.triggers.Add(entry);
        }

        public static void AddOnSelectedListeners(this EventTrigger trigger, UnityAction<BaseEventData> call)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Select;
            entry.callback.AddListener(call);
            trigger.triggers.Add(entry);

            EventTrigger.Entry entryT = new EventTrigger.Entry();
            entryT.eventID = EventTriggerType.PointerEnter;
            entryT.callback.AddListener(call);
            trigger.triggers.Add(entryT);
        }

        public static void AddOnSubmitListeners(this EventTrigger trigger, UnityAction<BaseEventData> call)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Submit;
            entry.callback.AddListener(call);
            trigger.triggers.Add(entry);

            EventTrigger.Entry entryT = new EventTrigger.Entry();
            entryT.eventID = EventTriggerType.PointerClick;
            entryT.callback.AddListener(call);
            trigger.triggers.Add(entryT);
        }

        public static void RemoveAllListeners(this EventTrigger trigger)
        {
            trigger.triggers.Clear();
        }
    }
}