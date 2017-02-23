using UnityEngine;

namespace Clues {
    public class Button_Drawers : Clue
{
    public GameObject ObjectToTrigger;

    GameObject Drawer1;
    GameObject Drawer2;
    GameObject Drawer3;

    //all the drawers start off in the closed position
    bool drawer1_open = false;
    bool drawer2_open = false;
    bool drawer3_open = false;
    public string TriggerName;
    AudioSource Drawer_noise;

    void Start()
    {
        Drawer1 = GameObject.Find("Drawer1");
        Drawer2 = GameObject.Find("Drawer2");
        Drawer3 = GameObject.Find("Drawer3");
        Drawer_noise = GetComponent<AudioSource>();

        AddAction("OnButtonPress", ObjectToTrigger, TriggerName);
    }

    void OnButtonPress()
    {
        Trigger("OnButtonPress"); //triggers next object
    }

    void OnDrawer1()
    {
        if (drawer1_open) //if drawer is open, we want to close it
        {
            drawer1_open = false;
            Drawer_noise.Play();
            Drawer1.GetComponent<Animator>().ResetTrigger("drawer1_opened"); //closes drawer but with no animation (instantaneous close)
        }
        else //open the drawer
        {
            drawer1_open = true;
            Drawer_noise.Play();
        }
    }

    void OnDrawer2()
    {
        if (drawer2_open) //if drawer is open, we want to close it
        {
            drawer2_open = false;
            Drawer_noise.Play();
            Drawer2.GetComponent<Animator>().ResetTrigger("drawer2_opened"); //closes drawer but with no animation (instantaneous close)
        }
        else //open the drawer
        {
            drawer2_open = true;
            Drawer_noise.Play();
        }
    }

    void OnDrawer3()
    {
        if (drawer3_open) //if drawer is open, we want to close it
        {
            drawer3_open = false;
            Drawer_noise.Play();
            Drawer3.GetComponent<Animator>().ResetTrigger("drawer3_opened"); //closes drawer but with no animation (instantaneous close)
        }
        else //open the drawer
        {
            drawer3_open = true;
            Drawer_noise.Play();
        }
    }
   
    void Update()
    {

        if (drawer1_open)
        {
            Drawer1.GetComponent<Animator>().SetTrigger("drawer1_opened");
        }

        if (drawer2_open)
        {
            Drawer2.GetComponent<Animator>().SetTrigger("drawer2_opened");
        }

        if (drawer3_open)
        {
            Drawer3.GetComponent<Animator>().SetTrigger("drawer3_opened");
        }

    }

}

}
