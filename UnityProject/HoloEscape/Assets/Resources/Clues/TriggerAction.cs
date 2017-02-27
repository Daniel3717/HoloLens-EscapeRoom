using System.Runtime.CompilerServices;
using UnityEngine;

namespace Clues
{
    public class TriggerAction
    {
        public readonly string TriggerEvent;
        private readonly GameObject _triggerGameObject;
        private readonly string _triggerAction;

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