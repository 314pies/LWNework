using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWNet
{
    [System.Obsolete]
    public class NetworkObjectStateHandler : MonoBehaviour
    {
        /// <summary>
        /// Invoked when netowrk object state(properties) is updated. 
        /// Need to be attached to a network object
        /// </summary>
        /// <param name="properties"></param>
        public virtual void OnPropertiesUpdate(byte[] properties)
        {

        }
    }
}