using System.Collections.Generic;
using UnityEngine;

namespace Clues
{
    public abstract class Clue : MonoBehaviour
    {
        private readonly Dictionary<string, TriggerAction> actions = new Dictionary<string, TriggerAction>();

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public string GetDescription()
        {
            return GetProperty<string>("description");
        }

        public void SetProperty(string property, object value)
        {
            if (property == "active" && value is bool)
                gameObject.SetActive((bool) value);

            properties[property] = value;
        }

        public T GetProperty<T>(string property)
        {
            if (properties.ContainsKey(property) && properties[property] is T)
                return (T) properties[property];
            else
                return default(T);
        }

        public void AddAction(string trigger, TriggerAction action)
        {
            actions.Add(trigger, action);
        }

        public void AddAction(string trigger, GameObject nextClue, string action)
        {
            actions.Add(trigger, new TriggerAction(nextClue, action));
        }

        protected void Trigger(string action)
        {
            if (actions.ContainsKey(action))
                actions[action].Trigger();
        }
    }
}