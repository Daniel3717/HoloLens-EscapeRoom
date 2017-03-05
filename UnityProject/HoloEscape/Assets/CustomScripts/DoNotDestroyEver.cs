using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroyEver : MonoBehaviour {

    //use only on one object (due to static)
    private static GameObject instance = null;

    void Start()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this.gameObject;
            DontDestroyOnLoad(instance);
        }

    }
}
