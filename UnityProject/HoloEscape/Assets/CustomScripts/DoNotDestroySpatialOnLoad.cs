using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroySpatialOnLoad : MonoBehaviour {

    private static GameObject instance = null;
    private int CountdownMax = 60;
    private int CountdownLeft = 60;

    // Use this for initialization
    void Start()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this.gameObject;
        DontDestroyOnLoad(this.gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        /*
        CountdownLeft--;
        if (CountdownLeft<0)
        {
            Debug.Log("Spatial position:"+this.gameObject.transform.position + "   Camera position:" + Camera.main.transform.position);
            CountdownLeft = CountdownMax;
        }
        */
    }
}
