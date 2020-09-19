using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LWNet;

public class TestNetworkBeha : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            var latestPos =  LWNetwork.SerializePar(transform.position);
            Debug.Log("sending");
            GetComponent<LWView>().FastRPC("Test233", false, Reciever.All, latestPos);
        }
    }

    [LWFastRPC("Test233")]
    public void Test(byte[] data)
    {
        var par = LWNetwork.DeserializePar(data, sType.Vector3);
        transform.position = (Vector3)par[0];
    }
}
