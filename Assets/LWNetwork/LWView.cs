using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace LWNet
{
    /// <summary>
    /// Sorting methods
    /// </summary>
    public class MethodSortComparer : IComparer<FastRPCInstancePack>
    {
        public int Compare(FastRPCInstancePack x, FastRPCInstancePack y)
        {
            return String.Compare(x.attribute.MethodCode, y.attribute.MethodCode);
        }
    }
    /// <summary>
    /// All "complete" method pack including method and its instance
    /// </summary>
    public class FastRPCInstancePack
    {
        public FastRPCInstancePack(object _instance, MethodInfo _method, LWFastRPC _attribute)
        {
            instance = _instance;
            method = _method;
            attribute = _attribute;
        }
        /// <summary>
        /// The instance this method belong to
        /// </summary>
        public object instance { get; private set; }
        public MethodInfo method { get; private set; }
        public LWFastRPC attribute { get; private set; }
    }

    /// <summary>
    /// For storing info of an RPC method
    /// </summary>
    [Serializable]
    public class RPCMethodInfo
    {

        public string MethodName;
        public string Code;
    }

    /// <summary>
    /// LWNetwork Network GameObject
    /// </summary>
    public class LWView : MonoBehaviour
    {
        /// <summary>
        /// Is this gameobject belong to local user
        /// </summary>
        public bool IsMine { get { return (networkID.Owner == LWNetwork.LocalPlayerId); } }
        /// <summary>
        /// Is this network object belong to scene(Not belong to any player(client))
        /// </summary>
        public bool IsSceneObject
        {
            get
            {
                return Owner == -1;
            }
        }

        /// <summary>
        /// Define the method format for processing network object state when it's updated
        /// </summary>
        /// <param name="properties">Latest updated properties</param>
        public delegate void NetworkObjectStateUpdateEvent(byte[] properties);

        NetworkObjectStateUpdateEvent networkObjectStateUpdateEventHandlers;
        public event NetworkObjectStateUpdateEvent NetworkObjectStateUpdateEventHandlers
        {
            add
            {
                value(Properties);
                networkObjectStateUpdateEventHandlers += value;
            }
            remove
            {
                networkObjectStateUpdateEventHandlers -= value;
            }
        }


        /// <summary>
        /// Is this a static network Object? 
        /// A static network object will be placed in scene as default 
        /// and can't be delete. 
        ///This field can only be set via editor. 
        /// </summary>
        public bool IsStaticNetworkObject { get { return isStaticNetworkObject; } }
        [HideInInspector]
        [SerializeField]
        private bool isStaticNetworkObject = false;

        /// <summary>
        /// Which player this view belong(Player netowrk id)
        /// </summary>
        public int Owner { get { return networkID.Owner; } }

        /// <summary>
        /// The NetworkID of this network game object. 
        /// Will be set 
        /// when this gameObject was instantiated at runtime(non-static Network GameObject)
        /// or 
        /// being initializing from the editor(static Network Scene GameObject)
        /// </summary>
        public NetworkObjectKey NetworkID { get { return networkID; } }

        [HideInInspector]
        [SerializeField]
        private NetworkObjectKey networkID;
        /// <summary>
        /// The latest state(property) of this network object
        /// </summary>
        private byte[] Properties = new byte[0];

        /// <summary>
        /// Initialize this network object instance.
        /// Set network ID.
        /// Register RPC methods
        /// and setup network object state(properties) handler
        /// </summary>
        /// <param name="netwokorkID">Netwokork identifier.</param>
        /// <param name="properties">Properties.</param>
        public void Initialize(NetworkObjectKey netwokorkID, byte[] properties)
        {
            networkID = netwokorkID;
            Initialize(properties);
        }

        /// <summary>
        /// Initialize this network object instance.
        /// Register RPC methods
        /// and setup network object state(properties) handler
        /// </summary>
        /// <returns>The initialize.</returns>
        /// <param name="properties">Properties.</param>
        public void Initialize(byte[] properties)
        {
            RegisterFastRPCMethods();
            SetProperties(properties);
        }

        /// <summary>
        /// Set state(properties) data to this network object.
        /// Should only be invoked from <see cref="LWNetwork"/>
        /// </summary>
        /// <param name="properties"></param>
        public void SetProperties(byte[] properties)
        {
            if (properties == null)
            {
                Debug.LogWarning("NetworkObject properties cannot be null. It should be byte[0] instead");
                return;
            }
            if (!Properties.SequenceEqual(properties))
            {
                Properties = (byte[])properties.Clone();
                if (networkObjectStateUpdateEventHandlers != null)
                    networkObjectStateUpdateEventHandlers(Properties);
            }
        }

        /// <summary>
        /// Updates the state(properties) of this network object.
        /// </summary>
        /// <param name="properties">Properties.</param>
        public void UpdateNetworkObjectState(byte[] properties)
        {
            if (networkID.Owner == -1)//Is a network scene object
            {
                NetworkObjectInfo _currentInfo =
                    (NetworkObjectInfo)LWNetwork.GetRoomSyncVariable(networkID.ToString(), new NetworkObjectInfo(networkID, "", transform.position, transform.rotation, new byte[0]));
                _currentInfo.Properties = properties;
                LWNetwork.SetRoomSyncVariable(networkID.ToString(), _currentInfo);
            }
            else if(IsMine)
            {
                NetworkObjectInfo _currentInfo = (NetworkObjectInfo)LWNetwork.GetLocalPlayerSyncVariable(networkID.ToString(), null);
                if (_currentInfo == null)
                {
                    Debug.LogError("Network Object instance's info isn't found in sync-variable. " +
                        "NetworkID: " + networkID.ToString() + ", name: " + gameObject.name);
                    return;
                }
                _currentInfo.Properties = properties;
                LWNetwork.SetLocalPlayerSyncVariable(networkID.ToString(), _currentInfo);
            }
            else
            {
                Debug.Log("Cannot update NetworkObjectState. " + networkID);
            }
        }

        /// <summary>
        /// For avoid cleanup operation being invoked twice
        /// May have a better way to do it
        /// </summary>
        private bool isCleanUpDone = false;

        public void OnDestroy()
        {
            if (isCleanUpDone) { return; }

            for (int i = 0; i < FastRPCMethods.Length; i++)
                LWNetwork.Core.FastRPCMethodDic.Remove(new MethodKey(networkID.Owner, networkID.ViewId, (byte)i));

            Debug.Log("Network Object " + networkID.ToString() + " cleanup operation of LWView has finished");
            isCleanUpDone = true;
        }

        /// <summary>
        /// Call a remote process by its name
        /// </summary>
        /// <param name="_methodName">The name of the remote precess</param>
        /// <param name="_IsReliable">Send it reliable or not</param>
        /// <param name="_target">Target to send to</param>
        /// <param name="_params">The parameters passing to this method. Warning: Sending null is NOT support</param>
        [Obsolete]
        public void RPC(string _methodName, bool _IsReliable, Reciever _target, params object[] _params)
        {
            //if (!RPCMethodIndex.ContainsKey(_methodName))
            //{
            //    Debug.Log(_methodName + " RPC method isn't exist. Did you click the 'Refresh' button on LWView?");
            //    return;
            //}
            //LWNetwork.Core.SendRPC(null, _target, _IsReliable, NetworkID.Owner, NetworkID.ViewId, RPCMethodIndex[_methodName], _params);
        }

        /// <summary>
        /// Call a remote process(by its name) to specific player by the player network id
        /// </summary>
        /// <param name="_methodName">The name of the remote precess</param>
        /// <param name="_IsReliable">Send it reliable or not</param>
        /// <param name="TargetPlayersId">Send this RPC to specific players by its network id</param>
        /// <param name="_params">The parameters passing to this method</param>
        [Obsolete]
        public void RPC(string _methodName, bool _IsReliable, int[] TargetPlayersId, params object[] _params)
        {
            //if (!RPCMethodIndex.ContainsKey(_methodName))
            //{
            //    Debug.Log(_methodName + " RPC method isn't exist. Did you click the 'Refresh' button on LWView?");
            //    return;
            //}
            //LWNetwork.Core.SendRPC(TargetPlayersId, MessageReciever.Empty, _IsReliable, NetworkID.Owner, NetworkID.ViewId, RPCMethodIndex[_methodName], _params);
        }

        /// <summary>
        /// Call a efficient remote method
        /// </summary>
        /// <param name="_methodCode">name of the method</param>
        /// <param name="_isReliable">Send it reliable or not</param>
        /// <param name="_target">Target to send to</param>
        /// <param name="_params">The parameters gonna to send, must to be converted to bytecode</param>
        public void FastRPC(string _methodCode, bool _isReliable, Reciever _target, byte[] _params = null)
        {
            if (!FastRPCArrayIndex.ContainsKey(_methodCode))
            {
#if UNITY_EDITOR
                Debug.Log(_methodCode + " FastRPC method isn't exist. Did you click the 'refresh' button on LWView?");
#endif
                return;
            }

            //Invoking local FastRpc 
            //Optimize local RPC invoking
            if (_target == Reciever.All)
            {
                if (localFastRPCDict.ContainsKey(_methodCode))
                {
                    localFastRPCDict[_methodCode](_params);
                    _target = Reciever.Others;
                }
                else
                    Debug.Log("localFastRPCDict: " + _methodCode + " not found.");
            }

            LWNetwork.Core.SendFastRPC(_target, _isReliable, networkID.Owner, networkID.ViewId, FastRPCArrayIndex[_methodCode], _params);
        }

        /// <summary>
        /// Call a efficient remote method on specific player devices
        /// </summary>
        /// <param name="_methodCode">name of the method</param>
        /// <param name="_isReliable">Send it reliable or not</param>
        /// <param name="_targetPlayers">Target players index to send to</param>
        /// <param name="_params">The parameters gonna to send, must to be converted to bytecode</param>
        public void FastRPC(string _methodCode, bool _isReliable, int[] _targetPlayers, byte[] _params = null)
        {
            if (!FastRPCArrayIndex.ContainsKey(_methodCode))
            {
#if UNITY_EDITOR
                Debug.Log(_methodCode + " FastRPC method isn't exist. Did you click the 'refresh' button on LWView?");
#endif               
                return;
            }
            LWNetwork.Core.SendFastRPC(_targetPlayers, _isReliable, networkID.Owner, networkID.ViewId, FastRPCArrayIndex[_methodCode], _params);
        }

        /// <summary>
        /// List of fast RPC methods under this view
        /// </summary>
        [ReadOnly]
        [SerializeField]
        private RPCMethodInfo[] FastRPCMethods;
        /// <summary>
        ///RPC dictionary between method code and method itself.
        ///So the (Fast) RPC method on local object can be invoked without registering it to <see cref="LWNetwork.Core.FastRPCMethodDic"/>
        ///, which can also optimizing local(self) RPC invoking
        /// </summary>
        private Dictionary<string, Action<byte[]>> localFastRPCDictField = null;
        private Dictionary<string, Action<byte[]>> localFastRPCDict
        {
            get
            {
                if (localFastRPCDictField == null)
                {
                    localFastRPCDictField = new Dictionary<string, Action<byte[]>>();
                    foreach (FastRPCInstancePack _m in SearchForRPCMethods(typeof(LWFastRPC)))
                    {
                        Action<byte[]> _rpcMethod = (Action<byte[]>)Delegate.
                            CreateDelegate(typeof(Action<byte[]>), _m.instance, _m.method);

                        string _methodCode = _m.attribute.MethodCode;

                        if (!localFastRPCDictField.ContainsKey(_methodCode))
                            localFastRPCDictField.Add(_methodCode, _rpcMethod);
                        else
                            Debug.Log("localFastRPCDictField: Duplicate key found! " + _m.method.Name);
                    }
                }
                return localFastRPCDictField;
            }
        }



        #region Register (Fast)RPC methods
        /// <summary>
        /// Get FastRPC method array index with its  method codes
        /// </summary>
        private Dictionary<string, byte> FastRPCArrayIndexField = null;
        /// Get FastRPC method array index with its  method codes
        private Dictionary<string, byte> FastRPCArrayIndex
        {
            get
            {
                if (FastRPCArrayIndexField == null)
                {
                    FastRPCArrayIndexField = new Dictionary<string, byte>();
                    for (int i = 0; i < FastRPCMethods.Length; i++)
                        FastRPCArrayIndexField.Add(FastRPCMethods[i].Code, (byte)i);
                }
                return FastRPCArrayIndexField;
            }
        }

        /// <summary>
        /// Register FastRPCMehtods to <see cref="LWNetwork.Core.FastRPCMethodDic"/>
        /// </summary>
        private void RegisterFastRPCMethods()
        {
            List<FastRPCInstancePack> _rpcMethods = SearchForRPCMethods(typeof(LWFastRPC));
            foreach (FastRPCInstancePack _m in _rpcMethods)
            {
                if (!FastRPCArrayIndex.ContainsKey(_m.attribute.MethodCode))
                {
                    Debug.LogError("Method code" + _m.attribute.MethodCode + " cannot be found in FastRPCMethods." +
                        " Did you pressed refresh button on LWView?" +
                        "Interrupting FastRPC registration.");
                    return;
                }

                byte _RPCid = FastRPCArrayIndex[_m.attribute.MethodCode];
                Action<byte[]> action = (Action<byte[]>)Delegate.CreateDelegate(typeof(Action<byte[]>), _m.instance, _m.method);

                MethodKey _key = new MethodKey(networkID.Owner, networkID.ViewId, _RPCid);
                if (!LWNetwork.Core.FastRPCMethodDic.ContainsKey(_key))
                {
                    LWNetwork.Core.FastRPCMethodDic.Add(
                        new MethodKey(networkID.Owner, networkID.ViewId, _RPCid),
                        action
                    );
                }
                else
                {
                    Debug.LogWarning("FastRPC already in MethodDictionary which is not suppose to happen." +
                             " key: " + _key.ToString() + ". Will replace it with new method instance");
                    LWNetwork.Core.FastRPCMethodDic[_key] = action;
                }

            }
            Debug.Log(gameObject.name + ": " + _rpcMethods.Count + " LWFastRPC methods have been register.");
        }
        /// <summary>
        /// Search for all RPC Fast methods under this view
        /// </summary>
        /// <param name="RPCAttribute">"LWRPC" or "FastLWRPC"(FastLWRPC is now the only support type)</param>
        /// <returns></returns>
        private List<FastRPCInstancePack> SearchForRPCMethods(Type RPCAttribute)
        {
            List<FastRPCInstancePack> _methodList = new List<FastRPCInstancePack>();
            object[] AllScript = gameObject.GetComponents<MonoBehaviour>();
            foreach (object _instance in AllScript)
            {
                //This can cause by when certain monoBehavior is missing
                if (_instance == null)
                {
                    Debug.LogWarning("Null monoBehavior is found on " + gameObject.name);
                    continue;
                }

                foreach (MethodInfo info in _instance.GetType().GetMethods())
                {
                    //If this method have the attribute, it will get into this loop
                    foreach (object attribute in info.GetCustomAttributes(RPCAttribute, true))
                    {
                        _methodList.Add(new FastRPCInstancePack(_instance, info, (LWFastRPC)attribute));
                        break; //Will only do it once
                    }
                }
            }
            return _methodList;
        }
        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Set this network object as a static network scene gameobject
        /// </summary>
        /// <param name="viewID"></param>
        public void SetAsStatic(byte viewID)
        {
            Undo.RecordObject(this, "Setting LWView as static");
            networkID = new NetworkObjectKey(-1, viewID);
            isStaticNetworkObject = true;
        }

        private void Reset()
        {
            Undo.RecordObject(this, "Reset LWView: " + gameObject.name);
            networkID = new NetworkObjectKey();
            isStaticNetworkObject = false;
            Refresh();
        }

        /// <summary>
        /// Refresh this LWview
        /// </summary>
        public void Refresh()
        {
            Undo.RecordObject(this, "Update LWView RPC list");
            // ResignViewID();
            //GetRPCList();
            GetFastRPCList();
        }
        /// <summary>
        /// Get all FastRPC methods and save it to FastRPCMehtods
        /// </summary>
        private void GetFastRPCList()
        {
            List<FastRPCInstancePack> _rpcMethods = SearchForRPCMethods(typeof(LWFastRPC));
            if (_rpcMethods.Count >= 255)
            {
                Debug.LogError("FastRPC methods amount have reach it limits(255)!");
                return;
            }

            MethodSortComparer comparer = new MethodSortComparer();
            _rpcMethods.Sort(comparer);


            FastRPCMethods = new RPCMethodInfo[_rpcMethods.Count];
            HashSet<string> _methodCodes = new HashSet<string>();
            //Save all RPC method to FastRPCMethods
            for (int i = 0; i < _rpcMethods.Count; i++)
            {
                string _methodCode = _rpcMethods[i].attribute.MethodCode;
                if (_methodCodes.Contains(_methodCode))
                {
                    Debug.LogError("Multiple FastRPC methods are using the same method code. " +
                        "Name: " + _rpcMethods[i].method.Name + " MethodCode: " + _methodCode);
                    return;
                }

                FastRPCMethods[i] = new RPCMethodInfo()
                {
                    MethodName = _rpcMethods[i].method.Name,
                    Code = _methodCode
                };
                _methodCodes.Add(_methodCode);
            }
        }
#endif
    }
}
