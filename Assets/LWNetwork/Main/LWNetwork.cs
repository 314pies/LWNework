using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using LWNet.NetorkUtilities;

namespace LWNet
{
    /// <summary>
    /// LWNet networking mode
    /// </summary>
    public enum NetworkMode
    {
        UnInitialize,
        Offline,
        PUN,
        Unet
    }

    public static partial class LWNetwork
    {
        /// <summary>
        /// Is LWNetwork Initialized?
        /// </summary>
        private static bool IsInitialized = false;

        /// <summary>
        /// The current network mode
        /// </summary>
        private static NetworkMode networkMode = NetworkMode.Unet;
        public static NetworkMode CurrentNetworkMode { get { return networkMode; } }
        public static void SetNetworkMode(NetworkMode mode) { networkMode = mode; }


        /// <summary>
        /// List of index for all players in room
        /// This list didn't include BOT
        /// </summary>
        public static int[] PlayerList
        {
            get
            {
                if (networkMode == NetworkMode.Unet)
                {
                    return LWLanManager.PlayerList;
                }
                return null;
            }
        }

        /// <summary>
        /// The current master client ID
        /// </summary>
        public static int MasterClientId
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// Is local device master client?
        /// </summary>
        public static bool IsMasterClient
        {
            get
            {
                if (MasterClientId == localPlayerId)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Local player networking id
        /// </summary>
        public static int LocalPlayerId { get { return localPlayerId; } }
        /// <summary>
        /// Local player networking id
        /// </summary>
        private static int localPlayerId = -1;

        public static partial class Core
        {
            /// <summary>
            /// Initialize LWNetwork
            /// </summary>
            /// <param name="staticNetworkSceneObject">static network scene object in this scene</param>
            /// <param name="_PUNController">PUN Multiplayer controller</param>
            /// <returns></returns>
            public static bool Initialize(LWView[] staticNetworkSceneObject)
            {
                if (networkMode == NetworkMode.UnInitialize)
                {
                    Debug.Log("Network mode is not signed yet");
                    IsInitialized = false;
                    return IsInitialized;
                }

                localPlayerId = LWLanManager.LocalPlayerId;
                Debug.Log("Initializing LWNetwork");
                ///Get Static scene objects' view ID
                {
                    List<byte> _networkStaticObjectViewIDList = new List<byte>();
                    foreach (LWView _v in staticNetworkSceneObject)
                    {
                        _networkStaticObjectViewIDList.Add(_v.NetworkID.ViewId);
                    }
                    staticSceneObjectIDs = _networkStaticObjectViewIDList.ToArray();
                }

                RPCMethodDic.Clear();
                FastRPCMethodDic.Clear();
                networkObjectInstances.Clear();

                //Add staticSceneObjects to networkObjectInstances to link network object info(mostly for properties)
                //with its actual instance.
                //So that when later syncing network objects info to netowrk object instance, it won't create a new instance
                //Will also initialize the instance at the same time
                foreach (LWView _view in staticNetworkSceneObject)
                {
                    NetworkObjectInfo _info = new NetworkObjectInfo(
                                              _view.NetworkID,
                                              "Static/" + _view.gameObject.name,//Use gameObject name as resource path for a static scene object
                                              _view.gameObject.transform.position,
                                              _view.gameObject.transform.rotation,
                                              new byte[0]
                    );
                    _view.Initialize(new byte[0]);
                    networkObjectInstances.Add(_view.NetworkID.ToString(), new NetworkObjectInstance(_info, _view.gameObject));
                }

                //Sync Network Objects Info stored in sync-var(properties) to its instance
                foreach (KeyValuePair<LWLanManager.SyncVarKey, object> _property in LWLanManager.SyncVariable)
                {
                    if (LWNetUtilities.IsNetworkObjectProperties(_property.Key.key, _property.Value))
                        Core.SyncNetworkObjectInfoToGameObjectInstance((string)_property.Key.key, (NetworkObjectInfo)_property.Value);
                }

                IsInitialized = true;
                Debug.Log("LWNetwork Initialize complete");
                return IsInitialized;
            }

            /// <summary>
            /// De-Initialized LWNetwork, should be invoked when Disconnect/Leaving room 
            /// </summary>
            public static void DeInitialize()
            {
                IsInitialized = false;
                localPlayerId = -1;
                RPCMethodDic.Clear();
                FastRPCMethodDic.Clear();
                networkObjectInstances.Clear();
                Debug.Log("LWNetwork de-initialized");
            }
        }

    }
}
