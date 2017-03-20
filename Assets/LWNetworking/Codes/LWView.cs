using UnityEngine;
using System.Collections;
using System.Reflection;

namespace LWNetworking
{
    public class LWView : MonoBehaviour
    {
        void Start()
        {

            
        }

        public void SearchForRPCMethods()
        {
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
                            break;
                        }
                    }
                }
            }
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
