using UnityEngine;


namespace Clues
{
    public class Button_In_Drawer : Clue
    {

        public GameObject ObjectToTrigger;
        bool pressed = false;
        AudioSource Button_noise;

        void Start()
        {
            AddAction("OnSelect", ObjectToTrigger, "ButtonPress");
            Button_noise = GetComponent<AudioSource>();
        }

        void OnSelect()
        {
            Trigger("OnSelect");
            Button_noise.Play();
            pressed = true;
        }

        void Update()
        {
            if (pressed)
            {
                //GetComponent<Animator>().SetTrigger("drawer_button_pressed"); //can't seem to fix this
                pressed = false;
            }
        }

    }
}