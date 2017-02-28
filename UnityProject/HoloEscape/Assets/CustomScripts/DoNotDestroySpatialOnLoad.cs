using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroySpatialOnLoad : MonoBehaviour {

    private static GameObject instance = null;

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

    }
}
