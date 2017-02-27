using UnityEngine;

namespace Clues
{
    public class Button : Clue
    {

        bool pressed = false;
        public GameObject ObjectToTrigger;
        public string TriggerName;
        AudioSource Button_noise;

        void Start()
        {
            AddAction("OnSelect", ObjectToTrigger, TriggerName);
            Button_noise = GetComponent<AudioSource>();
        }

        void OnSelect()
        {
            Trigger("OnSelect");
            Button_noise.Play();
            pressed = true; //triggers an animation of the button
        }

        void Update()
        {
            if (pressed)
            {
                GetComponent<Animator>().SetTrigger("button_pressed");
                pressed = false;
            }
        }
    }
}