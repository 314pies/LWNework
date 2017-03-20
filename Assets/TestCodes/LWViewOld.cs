using UnityEngine;
using System.Reflection;
using System;


public class LWView : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ToByteCode(12,12,312,"CCC",3,123,123,1,231);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToByteCode(params object[] a)
    {
        foreach (object b in a)
        {
            print(b.GetType());
            if(b.GetType() == typeof(int))
            {

            }
        }
    }
}
