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
            properties[property] = value;
        }

        public void SetProperty(Property property)
        {
            properties[property.name] = property.GetObject();
        }

        public T GetProperty<T>(string property)
        {
            if (properties.ContainsKey(property) && properties[property] is T)
                return (T) properties[property];
            else
                return default(T);
        }

        public void AddAction(TriggerAction action)
        {
            actions.Add(action.TriggerEvent, action);
        }

        public void AddAction(string trigger, GameObject nextClue, string action)
        {
            actions.Add(trigger, new TriggerAction(trigger, nextClue, action));
        }

        protected void Trigger(string action)
        {
            if (actions.ContainsKey(action))
                actions[action].Trigger();
        }
    }
}