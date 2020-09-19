using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using LWUtilities.CommonDelegate;
using LWUtilities;
using LWNet.NetorkUtilities;

namespace LWNet
{

    /// <summary>
    /// Use for accessing target Network Object. 
    /// It's like the ID for the network object
    /// </summary>
    [Serializable]
    public struct NetworkObjectKey
    {
        public NetworkObjectKey(int _owner, byte _viewId)
        {
            Owner = _owner;
            ViewId = _viewId;
        }
        public int Owner;
        public byte ViewId;

        public override string ToString()
        {
            return SVK.NGO + Owner + "." + ViewId;
        }
    }
    /// <summary>
    /// Network object info which will be synced over clients
    /// </summary>
    [Serializable]
    public class NetworkObjectInfo
    {
        /// <summary>
        /// Network Object's ID
        /// </summary>
        public NetworkObjectKey NetworkID;
        /// <summary>
        /// The asset path where instance will be loaded from
        /// </summary>
        public string ResourcePath;
        /// <summary>
        /// The position when this network object is instantiated
        /// </summary>        
        public SerializableVector3 Position;
        /// <summary>
        /// The rotation when this network object is instantiated
        /// </summary>
        public SerializableQuaternion Rotation;
        /// <summary>
        /// The latest state of this network object
        /// </summary>
        public byte[] Properties
        {
            get
            {
                if (properties != null)
                    return properties;
                else
                    return new byte[0];
            }
            set
            {
                if (value != null)
                    properties = value;
                else
                    properties = new byte[0];
            }
        }
        /// <summary>
        /// The latest state of this network object
        /// </summary>
        private byte[] properties = new byte[0];
        /// <summary>
        /// A seperated asset path to have different instance on local player(player owned this network object)
        /// </summary>
        public string SeperatedLocalResourcePath;

        public NetworkObjectInfo(NetworkObjectKey netoworkID, string path, Vector3 pos, Quaternion rot, byte[] initialProperties, string pathForLocal = "")
        {
            NetworkID = netoworkID;
            ResourcePath = path;
            Position = pos;
            Rotation = rot;
            Properties = initialProperties;
            SeperatedLocalResourcePath = pathForLocal;
        }

        /// <summary>
        /// Convert NetworkObjectInfo instance to byte array. 
        /// Need to use <see cref="DeserializeToObject(byte[])"/> to convert it back
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static byte[] SerializeToBytes(object instance)
        {
            NetworkObjectInfo _instance = (NetworkObjectInfo)instance;

            return LWNetwork.SerializePar(
                _instance.NetworkID.Owner,
                _instance.NetworkID.ViewId,
                _instance.ResourcePath,
                (Vector3)_instance.Position,
                (Quaternion)_instance.Rotation,
                _instance.Properties);
        }

        /// <summary>
        /// Convert byte[] returned from <see cref="SerializeToBytes(object)"/> back to object instance 
        /// </summary>
        /// <param name="serializedBytes"></param>
        /// <returns></returns>
        public static object DeserializeToObject(byte[] serializedBytes)
        {
            try
            {
                object[] _p = LWNetwork.DeserializePar(serializedBytes,
               sType.Int,
               sType.Byte,
               sType.String,
               sType.Vector3,
               sType.Quaternion,
               sType.ByteArray);

                NetworkObjectInfo _instance = new NetworkObjectInfo(
                    new NetworkObjectKey((int)_p[0], (byte)_p[1]),
                    (string)_p[2], (Vector3)_p[3], (Quaternion)_p[4], (byte[])_p[5]
                    );
                return _instance;
            }
            catch (Exception exp)
            {
                Debug.Log("NetworkObjectInfo.DeserializeToObject(): " + exp);
                return null;
            }                     
        }

    }
    /// <summary>
    /// Network object instance.
    /// Containing the GameObject instance of the network object and its <see cref="NetworkObjectInfo"/> which is being synced in sync-variables
    /// </summary>
    public class NetworkObjectInstance
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="info"><see cref="NetworkObjectInfo"/>Network Object info being synced in sync-variable</param>
        /// <param name="ins">Instance pf the network object</param>
        public NetworkObjectInstance(NetworkObjectInfo info, GameObject ins)
        {
            instance = ins;
            objInfo = info;
        }

        /// <summary>
        /// Perform clean up operation and destroy the gameobject instance.
        /// </summary>
        public void DestroyInstance()
        {
            instance.GetComponent<LWView>().OnDestroy();
            GameObject.Destroy(instance);
            instance = null;
        }

        /// <summary>
        /// GameObject instance of the Network Object
        /// </summary>
        public GameObject Instance { get { return instance; } }
        private GameObject instance;

        /// <summary>
        /// Network Object info being synced in sync-variable
        /// </summary>
        public NetworkObjectInfo ObjInfo { get { return objInfo; } }
        private NetworkObjectInfo objInfo;

    }

    public static partial class LWNetwork
    {
        /// <summary>
        /// List of static scene objects' ID
        /// </summary>
        private static byte[] staticSceneObjectIDs = new byte[0];
        /// <summary>
        /// Get network scene object info using its network ID
        /// Will return null if it didn't exist
        /// </summary>
        /// <param name="_networkID">The network ID</param>
        /// <returns></returns>
        public static NetworkObjectInfo GetSceneObjectInfo(string _networkID)
        {
            object _info = GetRoomSyncVariable(_networkID, null);
            if (_info != null)
                return (NetworkObjectInfo)_info;
            else
                return null;
        }

        /// <summary>
        /// The network object gameobject instances exist in scene
        /// </summary>
        public static Dictionary<string, NetworkObjectInstance> NetworkObjectInstances
        {
            get
            {
                return networkObjectInstances;
            }
        }
        /// <summary>
        /// The network object gameobject instances exist in scene
        /// </summary>
        private static Dictionary<string, NetworkObjectInstance> networkObjectInstances = new Dictionary<string, NetworkObjectInstance>();

        /// <summary>
        /// Create a Network scene object belongs to local player
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="initialState">Initial state(properties) for this network object</param>
        /// <returns></returns>
        public static GameObject InstantiateSceneObject(string resourcePath, Vector3 position, Quaternion rotation, byte[] initialState = null)
        {
            byte[] _sceneObjectArray = (byte[])GetRoomSyncVariable(SVK.SOL, new byte[0]);

            int _viewId = LookForAvailableViewID(_sceneObjectArray, staticSceneObjectIDs);
            Debug.Log("Creating network scene object. NetworkID: -1." + _viewId);
            if (_viewId != -1)
            {
                //Update view ID list
                List<byte> _sceneObjectList = new List<byte>(_sceneObjectArray);
                _sceneObjectList.Add((byte)_viewId);

                //Generate network scene object key and info
                NetworkObjectKey _networkObjectKey = new NetworkObjectKey(-1, (byte)_viewId);
                NetworkObjectInfo _newNetworkObjectInfo = new NetworkObjectInfo(_networkObjectKey, resourcePath, position, rotation, initialState);

                Dictionary<object, object> _newSceneObjProperties = new Dictionary<object, object>()
                {
                    {SVK.SOL,_sceneObjectList.ToArray()},
                    { _networkObjectKey.ToString() ,_newNetworkObjectInfo}
                };
                SetRoomSyncVariables(_newSceneObjProperties);

                ///<see cref="SyncNetworkObjectInfoToGameObjectInstance"/> will be invoked right after <see cref="SetRoomSyncVariables(Dictionary{object, object})"/> is invoked
                ///So the Gameobject instance should already be in <see cref="networkObjectInstances"/>
                ///Grab it and return its instance
                return networkObjectInstances[_networkObjectKey.ToString()].Instance;
            }
            else
            {
                Debug.LogError("No available view ID for creating Network Scene Object. All viewID have already been used.");
                return null;
            }
        }
        /// <summary>
        /// Create a Network scene object belongs to scene
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="initialState"></param>
        /// <param name="speratedLocalPath">Initial state(properties) for this network object</param>
        /// <returns></returns>
        public static GameObject InstantiatedPlayerObject(string resourcePath, Vector3 position, Quaternion rotation, byte[] initialState = null, string speratedLocalPath = "")
        {
            byte[] _playerObjectArray = (byte[])GetLocalPlayerSyncVariable(SVK.POL, new byte[0]);
            int _viewId = LookForAvailableViewID(_playerObjectArray);
            Debug.Log("Creating player's network object. NetworkID: " + localPlayerId + "." + _viewId);
            if (_viewId != -1)
            {
                //Update view ID list
                List<byte> _objectIDList = new List<byte>(_playerObjectArray);
                _objectIDList.Add((byte)_viewId);

                //Generate network object key and info
                NetworkObjectKey _networkObjectKey = new NetworkObjectKey(localPlayerId, (byte)_viewId);
                NetworkObjectInfo _newNetworkObjectInfo = new NetworkObjectInfo(_networkObjectKey, resourcePath, position, rotation, initialState, speratedLocalPath);

                Dictionary<object, object> _newPlayerProperties = new Dictionary<object, object>()
                {
                    {SVK.POL,_objectIDList.ToArray()},
                    { _networkObjectKey.ToString() ,_newNetworkObjectInfo}
                };
                SetLocalPlayerSyncVariables(_newPlayerProperties);

                ///<see cref="SyncNetworkObjectInfoToGameObjectInstance"/> will be invoked right after <see cref="SetRoomSyncVariables(Dictionary{object, object})"/> is invoked
                ///So the Gameobject instance should already be in <see cref="networkObjectInstances"/>
                ///Grab it and return its instance
                return networkObjectInstances[_networkObjectKey.ToString()].Instance;
            }
            else
            {
                Debug.LogError("No available view ID for creating Player's network Object. All viewID have already been used.");
                return null;
            }
        }

        /// <summary>
        /// Destroy the network object(Remove it from room properties)
        /// </summary>
        /// <param name="obj">GameObject</param>
        public static void DestroyNetworkObject(GameObject obj)
        {
            DestroyNetworkObject(obj.GetComponent<LWView>().NetworkID);
        }

        /// <summary>
        /// Destroy the network object(Remove it from room properties)
        /// </summary>
        /// <param name="obj">LWView(Netowrk game object)</param>
        public static void DestroyNetworkObject(LWView view)
        {
            DestroyNetworkObject(view.NetworkID);
        }

        /// <summary>
        /// Destroy the network object(Remove it from room properties)
        /// </summary>
        /// <param name="networkID">View.</param>
        public static void DestroyNetworkObject(NetworkObjectKey networkID)
        {
            //Remove it from room properties
            if (networkID.Owner == -1)
            {
                List<byte> _sceneObjectList = new List<byte>(
                    (byte[])GetRoomSyncVariable(SVK.SOL, new byte[0])
                    );

                if (_sceneObjectList.Contains(networkID.ViewId))
                {
                    _sceneObjectList.Remove(networkID.ViewId);
                    Dictionary<object, object> _newSceneObjProperties = new Dictionary<object, object>()
                    {
                        {SVK.SOL,_sceneObjectList.ToArray()},
                        { networkID.ToString() ,null}
                    };

                    SetRoomSyncVariables(_newSceneObjProperties);
                }
                else if (GetRoomSyncVariable(networkID.ToString(), null) != null)
                {
                    Debug.LogError("DestroyNetworkObject:" +
                        " Network object is not stored in list but it did have the network info, will remove it from sceneObject List. " +
                        "NetworkID: " + networkID.ToString());

                    SetRoomSyncVariable(networkID.ToString(), null);
                }
            }
            //Remove it from player's properties
            else if (networkID.Owner == localPlayerId)
            {
                List<byte> _playerObjectList = new List<byte>(
                  (byte[])GetLocalPlayerSyncVariable(SVK.POL, new byte[0])
                  );

                if (_playerObjectList.Contains(networkID.ViewId))
                {
                    _playerObjectList.Remove(networkID.ViewId);

                    Dictionary<object, object> _newPlayerObjProperties = new Dictionary<object, object>()
                    {
                        {SVK.POL,_playerObjectList.ToArray()},
                        { networkID.ToString() ,null}
                    };
                    SetLocalPlayerSyncVariables(_newPlayerObjProperties);
                }
                else if (GetLocalPlayerSyncVariable(networkID.ToString(), null) != null)
                {
                    Debug.LogError("DestroyNetworkObject:" +
                       " Network object is not stored in list but it did have the network info, will remove it from sceneObject List. " +
                       "NetworkID: " + networkID.ToString());

                    SetLocalPlayerSyncVariable(networkID.ToString(), null);
                }
            }
            else
            {
                Debug.LogError("User can only delete network objects owned by scene and themself. " +
                    "Objecy's NetworkID: " + networkID.ToString());
            }
        }

        /// <summary>
        /// Directly delete network object using its network ID. 
        /// This may leads to several errors so use it with causion.
        /// (ex: Don't delete something that is not a network scene object)
        /// </summary>
        /// <param name="_networkID"></param>
        [Obsolete]
        public static void DestroyNetworkSceneObjects(params string[] _networkIDs)
        {
            Dictionary<object, object> _properties = new Dictionary<object, object>();
            foreach (string _id in _networkIDs) { _properties.Add(_id, null); }
            SetRoomSyncVariables(_properties);
        }

        /// <summary>
        /// Instantiate and initialize network gameobject instance according to <see cref="NetworkObjectInfo"/>
        /// </summary>
        /// <param name="networkObjectInfo"></param>
        /// <returns></returns>
        private static GameObject InstantiateNetworkGameObjectInstance(NetworkObjectInfo networkObjectInfo)
        {
            string _resourcePath;
            if (networkObjectInfo.SeperatedLocalResourcePath != "" && networkObjectInfo.NetworkID.Owner == localPlayerId)
                _resourcePath = networkObjectInfo.SeperatedLocalResourcePath;
            else
                _resourcePath = networkObjectInfo.ResourcePath;

            GameObject _instance = LWUtilities.Utilities.InstantiateWithpath(_resourcePath, networkObjectInfo.Position, networkObjectInfo.Rotation);
            _instance.GetComponent<LWView>().Initialize(networkObjectInfo.NetworkID, networkObjectInfo.Properties);
            return _instance;
        }
        /// <summary>
        /// Looks for available identifier in certain range which is bewteen byte's minimun and maximun value
        /// </summary>
        /// <returns>Available identifier. Will return -1 if no ID available</returns>
        /// <param name="ExistingViewIDs">Existing identifier.</param>
        /// <param name="min">Minimum value of the range.</param>
        /// <param name="max">Maximun value of the range.</param>
        private static int LookForAvailableViewID(byte[] ExistingViewIDs, byte[] ExcludeViewIDs = null)
        {
            HashSet<byte> _ViewIDHashSet = new HashSet<byte>();

            if (ExcludeViewIDs != null)
            {
                foreach (byte _viewId in ExcludeViewIDs)
                {
                    _ViewIDHashSet.Add(_viewId);
                }
            }

            //Add existing ID to hashset
            foreach (byte _viewId in ExistingViewIDs)
            {
                _ViewIDHashSet.Add(_viewId);
            }

            //Search between min value and max value
            for (byte _potentialViewId = byte.MinValue; _potentialViewId < byte.MaxValue; _potentialViewId++)
            {
                if (!_ViewIDHashSet.Contains(_potentialViewId))
                    return (int)_potentialViewId;
            }
            return -1;
        }

        public static partial class Core
        {
            /// <summary>
            /// Synchronize in-game gameObject instance with NetworkObject info stored in sync-variable
            /// </summary>
            public static void SyncNetworkObjectInfoToGameObjectInstance(string networkID, NetworkObjectInfo networkObjectInfo)
            {
                Debug.Log("Syncing Network Object. NetworkID: " + networkID);
                if (!networkObjectInstances.ContainsKey(networkID))
                {
                    //Instantiated instance and register to NetworkObjectInstances
                    if (networkObjectInfo != null)
                    {
                        GameObject _instance = InstantiateNetworkGameObjectInstance(networkObjectInfo);
                        networkObjectInstances.Add(networkID, new NetworkObjectInstance(networkObjectInfo, _instance));
                    }
                }
                else//Already have an instance in the dictionary
                {
                    //Remove it's instance
                    if (networkObjectInfo == null)
                    {
                        networkObjectInstances[networkID].DestroyInstance();
                        networkObjectInstances.Remove(networkID);
                    }
                    //check whether is the same object(by resource path)
                    else if (networkObjectInstances[networkID].ObjInfo.ResourcePath == networkObjectInfo.ResourcePath)
                    {
                        //Just update the network object state(properties)
                        networkObjectInstances[networkID].Instance.GetComponent<LWView>().SetProperties(networkObjectInfo.Properties);
                    }
                    else//Delete the previous network object and create a new one to replace it
                    {
                        networkObjectInstances[networkID].DestroyInstance();
                        GameObject _instance = InstantiateNetworkGameObjectInstance(networkObjectInfo);
                        networkObjectInstances[networkID] = new NetworkObjectInstance(networkObjectInfo, _instance);
                    }
                }
            }
        }
    }
}
