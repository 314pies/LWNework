using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using LWUtilities.CommonDelegate;
using LWNet.NetorkUtilities;

namespace LWNet
{

    public static partial class LWNetwork
    {
        /// <summary>
        /// Get properties for this room
        /// </summary>
        public static Dictionary<object, object> RoomSyncVariables
        {
            get
            {
                if (networkMode == NetworkMode.Unet)
                {
                    return LWLanManager.RoomSyncVariables;
                }
                else if (networkMode == NetworkMode.Offline)
                {
                    throw new NotImplementedException("");
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get room syncronized variable
        /// </summary>
        /// <param name="key">key for getting variable</param>
        /// <returns></returns>
        public static object GetRoomSyncVariable(string _key, object _defaultValue)
        {
            if (networkMode == NetworkMode.Unet)
            {
                return LWLanManager.GetSyncVariable(-1, _key, _defaultValue);
            }
            return _defaultValue;
        }

        /// <summary>
        /// Set room syncronized variable
        /// </summary>
        /// <param name="key">key for saving variable</param>
        /// <returns></returns>        
        public static bool SetRoomSyncVariable(string _key, object _value)
        {
            if (networkMode == NetworkMode.Unet)
            {
                LWLanManager.SetSyncVariable(-1, _key, _value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Set multiple sync-var(properties) at once
        /// </summary>
        /// <param name="properties"></param>
        public static void SetRoomSyncVariables(Dictionary<object, object> properties)
        {
            {
                foreach (KeyValuePair<object, object> _p in properties)
                    LWLanManager.SetSyncVariable(-1, (string)_p.Key, _p.Value);
            }
        }
        private static RoomSynvVarUpdateEvent roomSynvVarUpdateEventHandlers = new RoomSynvVarUpdateEvent();
        public static void SubscribeRoomSynvVarUpdate(string synvVarKey, Action<object> eventHandler)
        {
            roomSynvVarUpdateEventHandlers.Sub(synvVarKey, eventHandler);
        }
        public static void UnSubscribeRoomSynvVarUpdate(string synvVarKey, Action<object> eventHandler)
        {
            roomSynvVarUpdateEventHandlers.UnSub(synvVarKey, eventHandler);
        }

        static string getBOTKey(int playerID, string key)
        {
            return (SVK.VP + '(' + playerID + ')' + key);
        }
        static bool parseBOTKey(string BOTKey, ref int playerID, ref string key)
        {
            try
            {
                if (BOTKey.Substring(0, SVK.VP.Length) != SVK.VP)
                    return false;

                int _endIndex = 0;
                for (int i = SVK.VP.Length + 1; i < BOTKey.Length; i++)
                {
                    if (BOTKey[i] == ')')
                    {
                        _endIndex = i;
                        break;
                    }
                }
                string _playerIndexStr = BOTKey.Substring(SVK.VP.Length + 1, _endIndex - SVK.VP.Length - 1);
                key = BOTKey.Substring(_endIndex + 1);

                if (!Int32.TryParse(_playerIndexStr, out playerID))
                {
                    return false;
                }
                return true;
            }
            catch (Exception exp)
            {
                Debug.Log("parseBOTKey: " + exp);
                return false;
            }
        }

        /// <summary>
        /// Set property(sync variable) for this player
        /// </summary>
        /// <returns>Is setting operation success?</returns>
        public static bool SetPlayerSyncVariable(int _playerId, string _key, object _value)
        {
            if (networkMode == NetworkMode.Unet)
            {
                if (_playerId < 0)
                {
                    string _BOTkey = getBOTKey(_playerId, _key);
                    LWLanManager.SetSyncVariable(-1, _BOTkey, _value);
                }

                LWLanManager.SetSyncVariable(_playerId, _key, _value);
                return true;
            }
            return false;
        }
        /// <summary>
        ///  Set property(sync variable) for local player
        /// </summary>
        /// <returns></returns>
        public static bool SetLocalPlayerSyncVariable(string _key, object _value)
        {
            if (networkMode == NetworkMode.Unet)
            {
                LWLanManager.SetSyncVariable(localPlayerId, _key, _value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Set multiple sync-var(properties) at once
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static bool SetLocalPlayerSyncVariables(Dictionary<object, object> properties)
        {
            if (networkMode == NetworkMode.Unet)
            {
                foreach (KeyValuePair<object, object> entry in properties)
                {
                    LWLanManager.SetSyncVariable(localPlayerId, (string)entry.Key, entry.Value);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get properties(sync variable) for this player
        /// Support virtual player(player ID<0)
        /// </summary>
        public static object GetPlayerSyncVariable(int _playerId, string _key, object _defaultValue)
        {
            if (networkMode == NetworkMode.Unet)
            {
                if (_playerId < 0)//Virtual Player(BOT)
                {
                    string _BOTkey = getBOTKey(_playerId, _key);
                    return LWLanManager.GetSyncVariable(-1, _BOTkey, _defaultValue);//Use Room Sync var  
                }
                else
                {
                    return LWLanManager.GetSyncVariable(_playerId, _key, _defaultValue);
                }
            }
            return _defaultValue;
        }
        /// <summary>
        /// Get properties(sync variable) for local player
        /// </summary>
        public static object GetLocalPlayerSyncVariable(string _key, object _defaultValue)
        {
            return GetPlayerSyncVariable(localPlayerId, _key, _defaultValue);
        }
        private static PlayerSynvVarUpdateEvent playerSynvVarUpdateEventHandlers = new PlayerSynvVarUpdateEvent();
        public static void SubscribePlayerSynvVarUpdate(string synvVarKey, Action<int, object> eventHandler)
        {
            playerSynvVarUpdateEventHandlers.Sub(synvVarKey, eventHandler);
        }
        public static void UnSubscribePlayerSynvVarUpdate(string synvVarKey, Action<int, object> eventHandler)
        {
            playerSynvVarUpdateEventHandlers.UnSub(synvVarKey, eventHandler);
        }

        public static partial class Core
        {
            public static void OnRoomSynvVarUpdate(string key, object value)
            {
                int _playerID = 0;
                string _key = "";
                bool _isBOTKey = parseBOTKey(key, ref _playerID, ref _key);
                if (!_isBOTKey)
                {
                    roomSynvVarUpdateEventHandlers.Invoke(key, value);
                }
                else
                {
                    playerSynvVarUpdateEventHandlers.Invoke(_key, _playerID, value);
                }
            }

            public static void OnPlayerSynvVarUpdate(int playerID, string key, object value)
            {
                playerSynvVarUpdateEventHandlers.Invoke(key, playerID, value);
            }
        }
    }
}