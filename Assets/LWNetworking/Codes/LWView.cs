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
        void Start()
        {
            //List<MethodInfo> AllRPCMethod = SearchForRPCMethods();
            //for (int _index=0; _index < AllRPCMethod.Count; _index++)
            //{
            //    ProcessKey _newKey = new ProcessKey(LWNetwork.playerID, (byte)ViewID, (byte)_index);
            //    MethodPack _newMethodpack = new
            //    //LWNetwork.RPCMethodDic.Add(_newKey, AllRPCMethod[_index]);
            //}            
        }

        public List<MethodPack> SearchForRPCMethods()
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
                                _types[i] = paraInof[i].GetType();

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
        private byte Owner;//Which player own this view
        [SerializeField]
        private byte ViewID;
        /// <summary>
        /// Call a remote proccess
        /// </summary>
        public void RPC(bool _IsReliable, byte _functionid, params object[] _params)
        {
            LWNetwork.SendRPC(_IsReliable, ViewID, _functionid, _params);
        }
    }
}
