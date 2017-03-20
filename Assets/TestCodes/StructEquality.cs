using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public struct TestingStruct
{

    public TestingStruct(int _F, int _S)
    {
        F = _F;
        S = _S;
    }
    public int F;
    public int S;
};




public class StructEquality : MonoBehaviour
{
    public Dictionary<TestingStruct, int> TestingDict = new Dictionary<TestingStruct, int>();

    // Use this for initialization
    void Start()
    {
        TestingStruct FI = new TestingStruct();
        FI.F = 8;
        FI.S = 7;
        TestingDict.Add(FI, 8787478);

        int a, b;
        a = 8;
        b = 7;
        Debug.Log("Getting value :" + TestingDict[new TestingStruct(a,b)]);
    }




    public void GetValue(TestingStruct _a)
    {


    }
    // Update is called once per frame
    void Update()
    {

    }
}
