using UnityEngine;

namespace Clues
{
    public class TriggerAction
    {
        private readonly string _triggerAction;
        private readonly GameObject _triggerGameObject;
        public readonly string TriggerEvent;

        public TriggerAction(string trigger, GameObject triggerGameObject, string triggerAction)
        {
            TriggerEvent = trigger;
            _triggerGameObject = triggerGameObject;
            _triggerAction = triggerAction;
        }

        public void Trigger()
        {
            _triggerGameObject.SetActive(true);
            _triggerGameObject.BroadcastMessage("On" + _triggerAction);
        }
    }
}