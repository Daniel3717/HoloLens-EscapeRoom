using UnityEngine;

namespace Clues
{
    public class TriggerAction
    {
        private readonly GameObject _triggerGameObject;
        private readonly string _triggerAction;

        public TriggerAction(GameObject triggerGameObject, string triggerAction)
        {
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