using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;
using LWNet.uNETTransport;
using LWUtilities;
using LWNet.NetorkUtilities;

namespace LWNet
{

    namespace uNETTransport
    {
        /// <summary>
        /// Byte message to server
        /// </summary>
        public class ByteMessage_TS : MessageBase
        {
            /// <summary>
            /// The message receivers' network id, 
            /// the first slot is for analyzing who is the target send to
            /// >=0 target to send to, -1 All, -2 Others,-3 master client,-4 Empty
            /// </summary>
            public int[] Receivers;
            public byte[] Datas;
        }

        /// <summary>
        /// Byte message to client
        /// </summary>
        public class ByteMessage_TC : MessageBase
        {
            public int Sender;
            public byte[] Datas;
        }

        public class PlayerListMessage : MessageBase
        {

            public int[] playerList;
        }

        public class EmptyMessage : MessageBase
        {
        }

        public class UpdateSyncVarPair : MessageBase
        {
            public LWLanManager.SyncVarKey Key;
            public byte[] Data;
        }

        public class PlayerNetworkId : MessageBase
        {
            public int PlayerId;
        }
    }


    public struct LANRoomInfo
    {
        public string Name;
        public string data;
        public LANRoomInfo(string _name, string _data)
        {
            Name = _name;
            data = _data;
        }
    }

    [System.Serializable]
    public struct LobbyPlayer
    {
        public string Name;
        public LobbyPlayer(string _name)
        {
            Name = _name;
        }
    }

    public class LWLanManager : NetworkManager
    {
        public static string LocalIPAddress
        {
            get
            {
                return Network.player.ipAddress;
            }
        }

        /// <summary>
        /// Is local client connected(in-room)?
        /// </summary>
        public static bool IsInRoom
        {
            get
            {               
                return LWSingleton.IsClientConnected();
            }
        }


        public static LWLanManager LWSingleton;

        public static List<LANRoomInfo> RoomList = new List<LANRoomInfo>();

        // Use this for initialization
        void Awake()
        {
            if (LWSingleton != null) { Destroy(gameObject); return; }
            LWSingleton = this;
            this.dontDestroyOnLoad = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)//Menu scene
                isLoadingMenuScene = false;

            if (SceneManager.GetActiveScene().buildIndex != 0)//In Game scene
                isLoadingGameScene = false;

            if (IsInRoom && SceneManager.GetActiveScene().buildIndex == 0)//Menu scene
                CleanUpSyncVariables(true);
        }
        static private LobbyPlayer localPlayerInfo;
        static private string broadCastData = "";
        static private bool IsServer = false;

        static private int localPlayerId = -1;
        static public int LocalPlayerId { get { return localPlayerId; } }

        #region public functions for lobby
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_broadCastData">Data to be broadcast on LAN, referring to <see cref="LANRoomInfo.data"/></param>
        public static void LWStartHost(string _broadCastData, LobbyPlayer _playerInfo, Action _onHostStart)
        {
            broadCastData = _broadCastData;
            LWSingleton.StartHost();
            
            //Start sending broadcasting message
            //LWSingleton.StartBroadCast("_broadCastData");

            localPlayerInfo = _playerInfo;
            onConnectLobby = _onHostStart;
            //onDisconnectLobby = _OnHostClose;
        }

        /// <summary>
        /// Scan for a room and join
        /// </summary>
        /// <param name="_autoJoin"></param>
        /// <param name="_playerInfo"></param>
        public static void LWScanRoomAndJoin(LobbyPlayer _playerInfo, Action _onConnect)
        {
            LWLanNetworkDiscovery.LWSingleton.Initialize();
            LWLanNetworkDiscovery.AutoJoin = true;
            LWLanNetworkDiscovery.LWSingleton.StartAsClient();
            LWSingleton.BroadCasting = true;
            Debug.Log("Start scanning room.");
            localPlayerInfo = _playerInfo;
            onConnectLobby = _onConnect;
        }

        public static void ConnectToRoom(string _address, LobbyPlayer _playerInfo, Action _onConnect, Action _onDisconnect)
        {
            Debug.Log("Trying to connect a room with IP address. " + _address);
            localPlayerInfo = _playerInfo;
            onConnectLobby = _onConnect;
            onDisconnectLobby = _onDisconnect;
            if (LWLanManager.LWSingleton.client == null || !LWLanManager.LWSingleton.client.isConnected)
            {
                LWLanManager.LWSingleton.networkAddress = _address;
                LWLanManager.LWSingleton.StartClient();
            }
        }

        public static void StopRoomScan()
        {
            Debug.Log("Stop scanning room.");
            LWSingleton.StopBroadCast();
        }

        public static void SetMode(int _modeId)
        {
            if (!IsServer) { Debug.Log("Only server are allowed to set Mode"); return; }
            SetSyncVariable(-1, SVK.LMd, _modeId);
        }

        public static void SetMap(int _mapId)
        {
            if (!IsServer) { Debug.Log("Only server are allowed to set Map"); return; }
            SetSyncVariable(-1, SVK.LMp, _mapId);
        }

        static private bool isLoadingGameScene = false;
        /// <summary>
        /// Should be only invoked on Host 
        /// </summary>
        public static void StartGame(string _sceneName)
        {
            if (IsServer)
            {
                Debug.Log("Starting Game");
                if (!isLoadingGameScene)
                {
                    LWSingleton.ServerChangeScene(_sceneName);
                    isLoadingGameScene = true;
                }
            }
        }

        private static bool isLoadingMenuScene = false;
        const string MenuSceneName = "UI_Menu";
        public static void LeaveRoom()
        {
            Debug.Log("Leavinging Room");
            if (LWLanManager.IsHost && SceneManager.GetActiveScene().buildIndex != 0)
            {
                Debug.Log("Host leaving in-game room, sending everyone to lobby");
                if (!isLoadingMenuScene)
                {
                    LWSingleton.ServerChangeScene(MenuSceneName);
                    isLoadingMenuScene = true;
                }
            }
            else
            {
                Debug.Log("Leaving room, disconnecting/closing host");
                LWSingleton.StopHost();
            }
        }

        #endregion

        #region public properties for lobby
        public static LobbyPlayer[] LobbyPlayers
        {
            get
            {
                List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();
                SyncVarKey _key;
                foreach (int i in playerList)
                {
                    _key.PlayerId = i; _key.key = SVK.LPP;
                    if (syncVariables.ContainsKey(_key))
                    {
                        lobbyPlayers.Add((LobbyPlayer)syncVariables[_key]);
                    }
                }
                return lobbyPlayers.ToArray();
            }
        }

        public static int ModeId
        {
            get
            {
                return (int)GetSyncVariable(-1, SVK.LMd, 0);
            }
        }

        public static int MapId
        {
            get
            {
                return (int)GetSyncVariable(-1, SVK.LMp, 0);
            }
        }


        /// <summary>
        /// You probably don't need this
        /// </summary>
        public static bool IsHost
        {
            get
            {
                return IsServer;
            }
        }

        #endregion

        #region Actions
        static private Action onConnectLobby = null;

        static public Action OnDisconnectLobby { get { return onDisconnectLobby; } set { onDisconnectLobby = value; } }
        static private Action onDisconnectLobby = null;
        #endregion

        #region Events
        public override void OnStartServer()
        {
            IsServer = true;
            RegisterServerHandler();
            Debug.Log("Server Started");
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            //connections.Add(conn.connectionId, conn);
            ServerPlayerList.Add(conn.connectionId);
            SyncPlayerList();
            PlayerNetworkId _msg = new PlayerNetworkId();
            _msg.PlayerId = conn.connectionId;
            NetworkServer.SendToAll(OnPlayerConnectId, _msg);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            //connections.Remove(conn.connectionId);
            ServerPlayerList.Remove(conn.connectionId);
            SyncPlayerList();
            PlayerNetworkId _msg = new PlayerNetworkId();
            _msg.PlayerId = conn.connectionId;
            NetworkServer.SendToAll(OnPlayerDisconnectId, _msg);
            SetSyncVariable(conn.connectionId, SVK.LPP, null);//Remove player lobby  in sync variables
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            //Register handler for clients
            RegisterClientHanlder();
            SendInitialRequest();
            LWNetwork.SetNetworkMode(NetworkMode.Unet);

            if (!IsHost)//Stop searching for room
            {
                StopBroadCast();
            }

            try { onConnectLobby(); } catch { }
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            CleanUpData();
            StopBroadCast();
            LWNetwork.SetNetworkMode(NetworkMode.UnInitialize);
            LWNetwork.Core.OnDisconnectFromServer();
            LWNetwork.Core.OnLeftRoom();
            Debug.Log("OnClientDisconnect");
            try { onDisconnectLobby(); } catch { }
        }

        public override void OnStopClient()
        {
            CleanUpData();
            StopBroadCast();
            LWNetwork.SetNetworkMode(NetworkMode.UnInitialize);
            LWNetwork.Core.OnDisconnectFromServer();
            LWNetwork.Core.OnLeftRoom();
            Debug.Log("OnStopClient");
            try { onDisconnectLobby(); } catch { }
        }

        #endregion

        #region Data
        #region ServerOnly
        /// <summary>
        /// This player list will be sync over network
        /// </summary>
        private List<int> ServerPlayerList = new List<int>();
        /// <summary>
        /// Get connections with its connection id
        /// </summary>
        //private Dictionary<int, NetworkConnection> connections = new Dictionary<int, NetworkConnection>();
        #endregion

        #region Synchronized Data

        private static int[] playerList = new int[0];
        public static int[] PlayerList { get { return playerList; } }

        /// <summary>
        /// When <see cref="PlayerId"/> equal to -1 means it belong to Room
        /// </summary>
        public struct SyncVarKey { public int PlayerId; public string key; }
        /// <summary>
        /// This variable list will be sync over network
        /// </summary>
        public static Dictionary<SyncVarKey, object> SyncVariable { get { return syncVariables; } }
        /// <summary>
        /// This variable list will be sync over network
        /// </summary>
        private static Dictionary<SyncVarKey, object> syncVariables = new Dictionary<SyncVarKey, object>();

        /// <summary>
        /// Synchronized variable to be used in Lobby such as current Map, player's name...etc
        /// </summary>
        private readonly static HashSet<string> LobbySynvVarKeys
            = new HashSet<string>() { SVK.LPP, SVK.LMd, SVK.LMp };

        /// <summary>
        /// Clean up synchronize data . 
        /// This will only clean up local caches. 
        /// </summary>
        /// <param name="excludeLobbySyncVar">Set to true to exclude those sync-var key in <see cref="LobbySynvVarKeys"/></param>
        private static void CleanUpSyncVariables(bool excludeLobbySyncVar)
        {
            List<SyncVarKey> _keysToRemove = new List<SyncVarKey>();
            foreach (SyncVarKey _key in syncVariables.Keys)
            {
                if (excludeLobbySyncVar && LobbySynvVarKeys.Contains(_key.key)) { continue; }
                _keysToRemove.Add(_key);
            }
            foreach (SyncVarKey _key in _keysToRemove)
                syncVariables.Remove(_key);

            Debug.Log("LWLAN Sync var cleaned up");
        }

        /// <summary>
        /// Loop over all sync variable and grab those belong to Room
        /// </summary>
        public static Dictionary<object, object> RoomSyncVariables
        {
            get
            {
                Dictionary<object, object> _roomSync = new Dictionary<object, object>();
                foreach (KeyValuePair<SyncVarKey, object> _content in syncVariables)
                {
                    if (_content.Key.PlayerId == -1)
                        _roomSync.Add(_content.Key, _content.Value);
                }

                return _roomSync;
            }
        }
        //public Dictionary<SyncVarKey, object> SyncVariables { get { return syncVariables; } }
        public string SyncVariablesToString()
        {
            string _str = "";
            foreach (KeyValuePair<SyncVarKey, object> entry in syncVariables)
            {
                string _valueStr = Utilities.ObjectToString(entry.Value);
                _str += entry.Key.PlayerId + "," + entry.Key.key + ": " + _valueStr + "\n";
            }
            return _str;
        }

        #endregion
        #endregion

        #region Methods

        public static void SendByteArray(bool _isReliable, Reciever _RecieveGroup, int[] _Recievers, byte[] PackedData)
        {
            ByteMessage_TS _msg = new ByteMessage_TS();
            _msg.Datas = PackedData;
            if (_Recievers != null)
            {
                _msg.Receivers = _Recievers;
            }
            else
            {
                int _firstSlotData = -1;
                switch (_RecieveGroup)
                {
                    case Reciever.All:
                        _firstSlotData = -1; break;
                    case Reciever.Others:
                        _firstSlotData = -2; break;
                    case Reciever.MasterClient:
                        _firstSlotData = -3; break;
                    case Reciever.Empty:
                        _firstSlotData = -4; break;
                }
                _msg.Receivers = new int[1] { _firstSlotData };
            }

            if (!_isReliable)
                LWSingleton.client.SendUnreliable(TransportBytesId, _msg);
            else
                LWSingleton.client.SendByChannel(TransportBytesId, _msg, Channels.DefaultReliable);


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_playerId">-1 means it belongs to room</param>
        /// <param name="_key"></param>
        /// <param name="_value"></param>
        public static void SetSyncVariable(int _playerId, string _key, object _value)
        {
            SyncVarKey _syncVarKey = new SyncVarKey();
            _syncVarKey.PlayerId = _playerId;
            _syncVarKey.key = _key;

            LWSingleton.UpdateSyncVarRequest(_syncVarKey, _value);
        }
        /// <summary>
        /// 
        /// </summary>
        ///  <param name="_playerId">-1 means it belongs to room</param>
        /// <param name="_key"></param>
        /// <param name="_defaultValue"></param>
        /// <returns></returns>
        public static object GetSyncVariable(int _playerId, string _key, object _defaultValue)
        {
            SyncVarKey _syncVarKey = new SyncVarKey();
            _syncVarKey.PlayerId = _playerId;
            _syncVarKey.key = _key;
            if (syncVariables.ContainsKey(_syncVarKey))
                return syncVariables[_syncVarKey];
            else
                return _defaultValue;
        }
        /// <summary>
        /// Clean up data when leaving LAN room
        /// </summary>
        private void CleanUpData()
        {
            Debug.Log("LWLAN cleaning up data.");
            IsServer = false;
            playerList = new int[0];
            CleanUpSyncVariables(false);
            ServerPlayerList = new List<int>();
            localPlayerId = -1;
            //connections.Clear();
        }
        /// <summary>
        /// Update SyncVariables
        /// </summary>
        /// <param name="LWLANSynvcVarKey">Key</param>
        /// <param name="syncValue">Data. will remove it if its null</param>
        private void UpdateSyncVariables(SyncVarKey LWLANSynvcVarKey, object syncValue)
        {
            if (!syncVariables.ContainsKey(LWLANSynvcVarKey))
            {
                if (syncValue != null)
                    syncVariables.Add(LWLANSynvcVarKey, syncValue);
            }
            else
            {
                if (syncValue != null)
                    syncVariables[LWLANSynvcVarKey] = syncValue;
                else
                    syncVariables.Remove(LWLANSynvcVarKey);
            }

            if (LWNetUtilities.IsNetworkObjectProperties(LWLANSynvcVarKey.key, syncValue))
            {
                LWNetwork.Core.SyncNetworkObjectInfoToGameObjectInstance(
                    LWLANSynvcVarKey.key, (NetworkObjectInfo)syncValue
                );
            }

            if (LWLANSynvcVarKey.PlayerId == -1)
            {
                LWNetwork.Core.OnRoomSynvVarUpdate(LWLANSynvcVarKey.key, syncValue);
            }
            else
            {
                LWNetwork.Core.OnPlayerSynvVarUpdate(LWLANSynvcVarKey.PlayerId, LWLANSynvcVarKey.key, syncValue);
            }
        }

        private bool BroadCasting = false;
        private void StartBroadCast(string _broadCastData)
        {
            LWLanNetworkDiscovery.LWSingleton.Initialize();
            LWLanNetworkDiscovery.LWSingleton.broadcastData = broadCastData;
            LWLanNetworkDiscovery.LWSingleton.StartAsServer();
            BroadCasting = true;
        }
        private void StopBroadCast()
        {
            if (BroadCasting)
            {
                LWLanNetworkDiscovery.LWSingleton.StopBroadcast();
                BroadCasting = false;
            }
        }
        #region ClientMethods
        private void RegisterClientHanlder()
        {
            client.RegisterHandler(SyncPlayerListId, OnPlayerListUpdate);
            client.RegisterHandler(UpdateSyncVarPairId, OnSynVarPairUpdate);
            client.RegisterHandler(TransportBytesId, OnClientRecievedBytes);
            client.RegisterHandler(RecieveNetworkId_Id, OnRecievedNetworkId);
            client.RegisterHandler(ClientResendNetworkObjSpawnReqId, SendNetworkObjectSpawnRequest);
            client.RegisterHandler(OnPlayerConnectId, OnLWPlayerConnect);
            client.RegisterHandler(OnPlayerDisconnectId, OnLWPlayerDisconnect);
        }

        private void SendInitialRequest()
        {
            client.SendByChannel(InitialRequestId, new EmptyMessage(), Channels.DefaultReliable);
        }

        private void UpdateSyncVarRequest(SyncVarKey _key, object syncValue)
        {
            //Update local first
            UpdateSyncVariables(_key, syncValue);

            byte[] ByteData = GenericSerialization.ObjectToByteArray(syncValue);

            UpdateSyncVarPair _msg = new UpdateSyncVarPair();
            _msg.Data = ByteData;
            _msg.Key = _key;

            client.SendByChannel(UpdateSyncVarPairId, _msg, Channels.DefaultReliable);
        }

        #endregion

        #region ServerMethods

        private void RegisterServerHandler()
        {
            NetworkServer.RegisterHandler(InitialRequestId, OnRecievedInitialRequest);
            NetworkServer.RegisterHandler(UpdateSyncVarPairId, OnRecievedUpdateSyncVarRequest);
            NetworkServer.RegisterHandler(TransportBytesId, OnServerRecievedBytes);
        }

        private void SendPlayerIdToClient(int _playerId)
        {
            PlayerNetworkId _msg = new PlayerNetworkId();
            _msg.PlayerId = _playerId;
            NetworkServer.SendToClient(_playerId, RecieveNetworkId_Id, _msg);
        }

        private void SyncPlayerList()
        {
            PlayerListMessage _msg = new PlayerListMessage();
            _msg.playerList = ServerPlayerList.ToArray();

            NetworkServer.SendByChannelToAll(
                SyncPlayerListId,
                _msg,
                Channels.DefaultReliable
            );
        }

        /// <summary>
        /// Synchronize <see cref="syncVariables"/> on specific client
        /// </summary>
        private void Sync_SyncVarsToClient(int _connectionId)
        {
            foreach (KeyValuePair<SyncVarKey, object> entry in syncVariables)
            {
                byte[] ByteData = GenericSerialization.ObjectToByteArray(entry.Value);

                UpdateSyncVarPair _msg = new UpdateSyncVarPair();
                _msg.Data = ByteData;
                _msg.Key = entry.Key;

                NetworkServer.SendToClient(_connectionId, UpdateSyncVarPairId, _msg);
            }
        }

        public static void HaveAllClientsResendNetworkObjSpawnReq()
        {
            NetworkServer.SendToAll(ClientResendNetworkObjSpawnReqId, new EmptyMessage());
        }
        #endregion

        #endregion

        #region RegisteredHandler
        //Message id
        const short InitialRequestId = MsgType.Highest + 1;
        const short SyncPlayerListId = MsgType.Highest + 2;
        const short UpdateSyncVarPairId = MsgType.Highest + 3;
        const short TransportBytesId = MsgType.Highest + 4;
        const short RecieveNetworkId_Id = MsgType.Highest + 5;
        const short OnPlayerConnectId = MsgType.Highest + 6;
        const short OnPlayerDisconnectId = MsgType.Highest + 7;
        const short ClientResendNetworkObjSpawnReqId = MsgType.Highest + 8;


        #region Server 

        public void OnRecievedInitialRequest(NetworkMessage _msg)
        {
            SendPlayerIdToClient(_msg.conn.connectionId);
            SyncPlayerList();
            Sync_SyncVarsToClient(_msg.conn.connectionId);
        }

        public void OnRecievedUpdateSyncVarRequest(NetworkMessage _msg)
        {
            UpdateSyncVarPair _pair = _msg.ReadMessage<UpdateSyncVarPair>();

            foreach (int i in ServerPlayerList)
            {
                //The client sent this already update locally
                if (i != _msg.conn.connectionId)
                    NetworkServer.SendToClient(i, UpdateSyncVarPairId, _pair);
            }
        }

        public void OnServerRecievedBytes(NetworkMessage _msg)
        {
            ByteMessage_TS _msgIn = _msg.ReadMessage<ByteMessage_TS>();
            ByteMessage_TC _msgOut = new ByteMessage_TC();
            _msgOut.Sender = _msg.conn.connectionId;
            _msgOut.Datas = _msgIn.Datas;

            int channelId = _msg.channelId;

            if (_msgIn.Receivers[0] == -1)//Send to all
            {
                NetworkServer.SendByChannelToAll(TransportBytesId, _msgOut, channelId);
                // Debug.Log("all");
            }
            else if (_msgIn.Receivers[0] == -2)//Others
            {
                foreach (int i in playerList)
                {
                    if (i != _msgOut.Sender)
                        NetworkServer.SendToClient(i, TransportBytesId, _msgOut);
                }
                //foreach(int i in connections.Keys)
                //{
                //    if (i != _msgOut.Sender)
                //        connections[i].SendByChannel(TransportBytesId, _msgOut, channelId);
                //}
                //Debug.Log("Others");
            }
            else if (_msgIn.Receivers[0] == -3)//Master client
            {
                NetworkServer.SendToClient(0, TransportBytesId, _msgOut);
                // Debug.Log("Master");
            }
            else if (_msgIn.Receivers[0] >= 0)//Send to specific player
            {
                HashSet<int> _sentClients = new HashSet<int>();
                foreach (int i in _msgIn.Receivers)
                {
                    //Prevent from sending to the same client
                    if (!_sentClients.Contains(i))
                    {
                        NetworkServer.SendToClient(i, TransportBytesId, _msgOut);
                        _sentClients.Add(i);
                    }
                }
                //foreach (int i in _msgIn.Receivers)
                //{
                //    if (connections.ContainsKey(i))
                //        connections[i].SendByChannel(TransportBytesId, _msgOut, channelId);
                //}
                //  Debug.Log("specific");
            }
        }

        #endregion

        #region Client

        public delegate void ReceiveNetworkIDEvent();
        public ReceiveNetworkIDEvent ReceiveNetworkIDEventHandler;
        public void OnRecievedNetworkId(NetworkMessage _msg)
        {
            localPlayerId = _msg.ReadMessage<PlayerNetworkId>().PlayerId;
            Debug.Log("local Player ID received: " + localPlayerId);
            //Set my player lobby profile
            SetSyncVariable(localPlayerId, SVK.LPP, localPlayerInfo);
            if (ReceiveNetworkIDEventHandler != null)
                ReceiveNetworkIDEventHandler();
        }

        public void OnPlayerListUpdate(NetworkMessage _msg)
        {
            playerList = _msg.ReadMessage<PlayerListMessage>().playerList;
        }

        public void OnSynVarPairUpdate(NetworkMessage _msg)
        {
            UpdateSyncVarPair _pair = _msg.ReadMessage<UpdateSyncVarPair>();
            SyncVarKey _key = _pair.Key;
            object _data = GenericSerialization.ByteArrayToObject(_pair.Data);

            UpdateSyncVariables(_key, _data);

        }

        public void OnClientRecievedBytes(NetworkMessage _msg)
        {
            byte[] _data = _msg.ReadMessage<ByteMessage_TC>().Datas;
            LWNetwork.Core.OnRecieveByteArray(_data);
        }

        public void OnLWPlayerConnect(NetworkMessage _msg)
        {
            int _id = _msg.ReadMessage<PlayerNetworkId>().PlayerId;
            LWNetwork.Core.OnPlayerConnected(_id);
        }

        public void OnLWPlayerDisconnect(NetworkMessage _msg)
        {
            int _id = _msg.ReadMessage<PlayerNetworkId>().PlayerId;
            LWNetwork.Core.OnPlayerDisconnected(_id);
        }


        /// <summary>
        /// SendNetworkObjectSpawnRequest to ensure that we have the correct network objects
        /// </summary>
        private void SendNetworkObjectSpawnRequest(NetworkMessage _msg)
        {
            //LWNetwork.SendNetworkObjectSpawnRequest();
        }

        #endregion

        #endregion

#if UNITY_EDITOR
        private void Update()
        {
            isPlayerInRoom = IsInRoom;
        }

        [Header("Status")]
        [ShownAsLabel]
        [SerializeField]
        private bool isPlayerInRoom;
#endif
    }
}

