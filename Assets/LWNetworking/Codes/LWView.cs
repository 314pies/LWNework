using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace LWNetworking
{
    public class MethodSortComparer : IComparer<MethodPack>
    {
        public int Compare(MethodPack x, MethodPack y)
        {
            return String.Compare(x.method.Name, y.method.Name);
        }
    }

    public class LWView : MonoBehaviour
    {
        void Awake()
        {

            RegisterRPCMethods();
        }

        private void RegisterRPCMethods()
        {
            List<MethodPack> methodPacks = SearchForRPCMethods();
            if (methodPacks.Count >= 255)
            {
                Debug.LogError("RPC methods amount have reach it limits(255)!");
                return;
            }

            for (int _methodId = 0; _methodId < methodPacks.Count; _methodId++)
            {
                //ProcessKey _newKey = new ProcessKey(Owner,ViewID,(byte)i);
                LWNetwork.RPCMethodDic.Add(
                    new ProcessKey(Owner, ViewID, (byte)_methodId),
                    methodPacks[_methodId]
                );
                MethodToID.Add(methodPacks[_methodId].method.Name, (byte)_methodId);
            }

            Debug.Log(methodPacks.Count + " LWRPC methods have been register.");
        }

        private List<MethodPack> SearchForRPCMethods()
        {
            List<MethodPack> _methodList = new List<MethodPack>();
            object[] AllScript = gameObject.GetComponents<MonoBehaviour>();
            foreach (object _instance in AllScript)
            {
                foreach (MethodInfo info in _instance.GetType().GetMethods())
                {
                    foreach (object attribute in info.GetCustomAttributes(false))
                    {
                        if (attribute.ToString() == "LWRPC")
                        {
                            //Found a RPC method
                            ParameterInfo[] paraInof = info.GetParameters();
                            Type[] _types = new Type[paraInof.Length];
                            for (int i = 0; i < paraInof.Length; i++)
                                _types[i] = paraInof[i].ParameterType;

                            MethodPack _newMethodpack = new MethodPack(_instance, info, _types);
                            _methodList.Add(_newMethodpack);
                            break;
                        }
                    }
                }
            }
            MethodSortComparer comparer = new MethodSortComparer();
            _methodList.Sort(comparer);
            return _methodList;
        }


        [SerializeField]
        private int Owner;//Which player own this view
        [SerializeField]
        private byte ViewID;


        /// <summary>
        /// A dictionary to cast method name to it's index
        /// </summary>
        private Dictionary<string, byte> MethodToID = new Dictionary<string, byte>();

        /// <summary>
        /// Call a remote proccess by its index
        /// </summary>
        public void RPC(bool _IsReliable, byte _functionid, params object[] _params)
        {
            LWNetwork.SendRPC(_IsReliable,Owner, ViewID, _functionid, _params);
        }

        public void RPC(bool _IsReliable, string _methodName, params object[] _params)
        {
            if (!MethodToID.ContainsKey(_methodName))
            {
                Debug.Log(_methodName+ " method isn't exist.");
                return;
            }

            LWNetwork.SendRPC(_IsReliable, Owner, ViewID, MethodToID[_methodName], _params);
        }
    }
}
