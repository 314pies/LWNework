using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace LWNet
{
    public class LWLanNetworkDiscovery : NetworkDiscovery
    {
        public static LWLanNetworkDiscovery LWSingleton;
        public static bool AutoJoin = true;
        // Use this for initialization
        void Start()
        {
            LWSingleton = this;
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            if (AutoJoin && !LWLanManager.LWSingleton.isNetworkActive)
            {
                if (LWLanManager.LWSingleton.client != null && LWLanManager.LWSingleton.client.isConnected)
                    return;

                LWLanManager.LWSingleton.networkAddress = fromAddress;
                LWLanManager.LWSingleton.StartClient();
            }
            else
            {
                if (!LWLanManager.RoomList.Contains(new LANRoomInfo(fromAddress, data)))
                {
                    LWLanManager.RoomList.Add(new LANRoomInfo(fromAddress, data));
                    Debug.Log("Discovered room. Address: " + fromAddress + ", data: " + data);
                }
            }
        }
    }
}
