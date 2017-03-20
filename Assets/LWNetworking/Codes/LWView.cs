using UnityEngine;
using System.Collections;

namespace LWNetworking
{
    public class LWView : MonoBehaviour
    {
        [SerializeField]
        private byte ViewID;
        void Start()
        {
        }

        /// <summary>
        /// Call a remote proccess
        /// </summary>
        public void RPC(bool _IsReliable, byte _functionid, params object[] _params)
        {
            LWNetwork.SendRPC(_IsReliable,ViewID, _functionid, _params);
        }
    }
}
