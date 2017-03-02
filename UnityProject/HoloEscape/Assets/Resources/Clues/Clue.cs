using System.Collections.Generic;
using UnityEngine;

namespace Clues
{
    public abstract class Clue : MonoBehaviour
    {

        public bool Visible = true;
        public Transform ClueBase;

        private readonly Dictionary<string, TriggerAction> actions = new Dictionary<string, TriggerAction>();

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

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
            {
                SendMessage("OnSelect");
            }
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
                ClueBase.localScale = new Vector3(0,0,0);
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