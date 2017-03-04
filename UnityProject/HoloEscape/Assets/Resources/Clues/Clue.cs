using System.Collections.Generic;
using UnityEngine;

namespace Clues
{
    public abstract class Clue : MonoBehaviour
    {
        private readonly Dictionary<string, List<TriggerAction>> actions = new Dictionary<string, List<TriggerAction>>();

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public Transform ClueBase;

        public bool Visible = true;

        public void Initialise()
        {
            if (Visible && (!IsPropertySet("visible") || GetProperty<bool>("visible")))
            {
                SetProperty("visible", true);
            }
            else
            {
                SetProperty("visible", true);
                OnSetInvisible();
            }
            Debug.Log("Initialise");
        }

        public void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0))
                SendMessage("OnSelect");
        }

        public void SetProperty(JsonProperty property)
        {
            Debug.Log("property set");
            properties[property.name] = property.GetObject();
        }

        public void SetProperty(string property, object value)
        {
            properties[property] = value;
        }

        public T GetProperty<T>(string property)
        {
            if (properties.ContainsKey(property) && properties[property] is T)
                return (T) properties[property];
            return default(T);
        }

        public bool IsPropertySet(string property)
        {
            return properties.ContainsKey(property);
        }

        public void AddAction(TriggerAction action)
        {
            if (!actions.ContainsKey(action.TriggerEvent))
                actions[action.TriggerEvent] = new List<TriggerAction>();
            actions[action.TriggerEvent].Add(action);
        }

        public void AddAction(string trigger, GameObject nextClue, string action)
        {
            if (!actions.ContainsKey(trigger))
                actions[trigger] = new List<TriggerAction>();
            actions[trigger].Add(new TriggerAction(trigger, nextClue, action));
        }

        protected void Trigger(string action)
        {
            if (actions.ContainsKey(action))
                foreach (var t in actions[action])
                    t.Trigger();
        }

        protected void OnSetInvisible()
        {
            if (GetProperty<bool>("visible"))
            {
                SetProperty("originalScale", ClueBase.localScale);
                ClueBase.localScale = new Vector3(0, 0, 0);
            }
            SetProperty("visible", false);
        }

        protected void OnSetVisible()
        {
            if (!GetProperty<bool>("visible"))
                ClueBase.localScale = GetProperty<Vector3>("originalScale");
            SetProperty("visible", true);
        }
    }
}
