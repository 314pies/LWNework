using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LWNet;
using LWUtilities;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject_ODP a;
    public void SpawnObject()
    {
        LWNetwork.InstantiatedPlayerObject(a.GetPath, Vector3.zero, Quaternion.identity);
    }
}
