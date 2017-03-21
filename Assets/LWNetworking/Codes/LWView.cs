using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace LWNetworking
{
    public class LWView : MonoBehaviour
    {
        void Start()
        {
            List<MethodInfo> AllRPCMethod = SearchForRPCMethods();
            for (int _index=0; _index < AllRPCMethod.Count; _index++)
            {
                ProcessKey _newKey = new ProcessKey(LWNetwork.playerID, (byte)ViewID, (byte)_index);
                //LWNetwork.RPCMethodDic.Add(_newKey, AllRPCMethod[_index]);
            }            
        }

        public List<MethodInfo> SearchForRPCMethods()
        {
            List<MethodInfo> _methodList = new List<MethodInfo>();
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
                            _methodList.Add(info);
                            break;
                        }
                    }
                }
            }
            _methodList.Sort();
            return _methodList;
        }

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
