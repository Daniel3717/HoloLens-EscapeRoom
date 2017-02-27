using UnityEngine;

namespace Clues.Base.Button
{
    public class Button : Clue
    {
        private AudioSource Button_noise;
        public GameObject ObjectToTrigger;

        private bool pressed;
        public string TriggerName;

        private void Start()
        {
            AddAction("OnSelect", ObjectToTrigger, TriggerName);
            Button_noise = GetComponent<AudioSource>();
        }

        private void OnSelect()
        {
            Trigger("OnSelect");
            Button_noise.Play();
            pressed = true; //triggers an animation of the button
        }

        private void Update()
        {
            if (pressed)
            {
                GetComponent<Animator>().SetTrigger("button_pressed");
                pressed = false;
            }
        }
    }
}