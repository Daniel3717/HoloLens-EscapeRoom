using System.Collections.Generic;
using UnityEngine;

namespace Clues
{
    public abstract class Clue : MonoBehaviour
    {
        public Transform ClueBase;

        private readonly Dictionary<string, TriggerAction> actions = new Dictionary<string, TriggerAction>();

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public void Initialise()
        {
            if (IsPropertySet("visible") && !GetProperty<bool>("visible"))
            {
                OnSetInvisible();
            }
            else
            {
                SetProperty("visible", true);
            }
        }

        public void SetProperty(JsonProperty property)
        {
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
            actions[action.TriggerEvent] = action;
        }

        public void AddAction(string trigger, GameObject nextClue, string action)
        {
            actions[trigger] = new TriggerAction(trigger, nextClue, action);
        }

        protected void Trigger(string action)
        {
            if (actions.ContainsKey(action))
                actions[action].Trigger();
        }

        protected void OnSetInvisible()
        {
            if (GetProperty<bool>("visible"))
            {
                SetProperty("originalScale", ClueBase.localScale);
            }
            SetProperty("visible", false);
        }

        protected void OnSetVisible()
        {
            if (!GetProperty<bool>("visible"))
            {
                ClueBase.localScale = GetProperty<Vector3>("originalScale");
            }
            SetProperty("visible", true);
        }
    }
}