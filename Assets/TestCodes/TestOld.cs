using UnityEngine;
using System.Reflection;
using System.Collections;


public class TestOld : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        MethodInfo[] methodInfos = this.GetType().GetMethods();
        foreach (MethodInfo info in methodInfos)
        {
            foreach (object attribute in info.GetCustomAttributes(false))
            {
                if (attribute.ToString() == "LWRPC")
                {
                    print(info.Name);
                    object[] _Par = new object[2];
                    _Par[0] = 10;
                    _Par[1] = "AAA";

                    info.Invoke(this, _Par);
                    foreach (ParameterInfo parameter in info.GetParameters())
                    {
                        print(parameter.ParameterType);
                        if (parameter.ParameterType == typeof(int))
                        {
                            Debug.Log("It's an int");
                        }
                    }
                    info.Invoke(this, null);
                    //print("ACC: " + info.GetParameters());
                    //info.GetParameters();
                }
            }
            //print(info.Attributes);
            //if (info.Name.IndexOf("AA") > -1)
            //    info.Invoke(this, new object[]);
        }
    }


    [LWRPC]
    public void AnotherAAA(int c, string x)
    {
        Debug.Log("A____w____A" + c);
        Debug.Log(x);
    }
}