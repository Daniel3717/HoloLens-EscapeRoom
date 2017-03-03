using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroyEver : MonoBehaviour {

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
