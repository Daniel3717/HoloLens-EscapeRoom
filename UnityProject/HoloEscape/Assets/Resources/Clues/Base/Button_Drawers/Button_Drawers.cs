using UnityEngine;

namespace Clues.Base.Button_Drawers
{
    public class Button_Drawers : Clue
    {
        private AudioSource Drawer_noise;

        private GameObject Drawer1;

        //all the drawers start off in the closed position
        private bool drawer1_open;
        private GameObject Drawer2;
        private bool drawer2_open;
        private GameObject Drawer3;
        private bool drawer3_open;
        public GameObject ObjectToTrigger;
        public string TriggerName;

        private void Start()
        {
            Drawer1 = GameObject.Find("Drawer1");
            Drawer2 = GameObject.Find("Drawer2");
            Drawer3 = GameObject.Find("Drawer3");
            Drawer_noise = GetComponent<AudioSource>();

            AddAction("OnButtonPress", ObjectToTrigger, TriggerName);
        }

        private void OnButtonPress()
        {
            Trigger("OnButtonPress"); //triggers next object
        }

        private void OnDrawer1()
        {
            if (drawer1_open) //if drawer is open, we want to close it
            {
                drawer1_open = false;
                Drawer_noise.Play();
                Drawer1.GetComponent<Animator>().ResetTrigger("drawer1_opened");
                //closes drawer but with no animation (instantaneous close)
            }
            else //open the drawer
            {
                drawer1_open = true;
                Drawer_noise.Play();
            }
        }

        private void OnDrawer2()
        {
            if (drawer2_open) //if drawer is open, we want to close it
            {
                drawer2_open = false;
                Drawer_noise.Play();
                Drawer2.GetComponent<Animator>().ResetTrigger("drawer2_opened");
                //closes drawer but with no animation (instantaneous close)
            }
            else //open the drawer
            {
                drawer2_open = true;
                Drawer_noise.Play();
            }
        }

        private void OnDrawer3()
        {
            if (drawer3_open) //if drawer is open, we want to close it
            {
                drawer3_open = false;
                Drawer_noise.Play();
                Drawer3.GetComponent<Animator>().ResetTrigger("drawer3_opened");
                //closes drawer but with no animation (instantaneous close)
            }
            else //open the drawer
            {
                drawer3_open = true;
                Drawer_noise.Play();
            }
        }

        private void Update()
        {
            if (drawer1_open)
                Drawer1.GetComponent<Animator>().SetTrigger("drawer1_opened");

            if (drawer2_open)
                Drawer2.GetComponent<Animator>().SetTrigger("drawer2_opened");

            if (drawer3_open)
                Drawer3.GetComponent<Animator>().SetTrigger("drawer3_opened");
        }
    }
}