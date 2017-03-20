using UnityEngine;
using System.Collections;
using LWNetworking;

public class RPCInvokeTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<LWView>().RPC(false,87,12,"NameDoggyCattyKittyLionny",45,12);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}