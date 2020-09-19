using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LWUtilities.CommonDelegate;

namespace LWNet
{
    public static partial class LWNetwork
    {
        /// <summary>
        /// Leave the room we are currently in
        /// </summary>
        public static void LeaveRoom()
        {
            if (networkMode == NetworkMode.Unet)
                LWLanManager.LeaveRoom();
        }

        /// <summary>
        /// Event be invoked when a player disconnected            
        /// </summary>
        public static LWEvent_Int OnPlayerDisconnectedDelegate =
            new LWEvent_Int((_playerIndex) => { Debug.Log("LWNetwork: PlayerID " + _playerIndex + " has left the room."); });

        public delegate void OnMasterClientSwitched(int newMasterClientID);
        private static OnMasterClientSwitched onMasterClientSwitchedHandlers;
        public static event OnMasterClientSwitched OnMasterClientSwitchedHandlers
        {
            add
            {
                onMasterClientSwitchedHandlers += value;
            }
            remove
            {
                onMasterClientSwitchedHandlers -= value;
            }
        }

        public static partial class Core
        {
            /// <summary>
            /// Invoked when lost connection with server
            /// </summary>
            public static void OnDisconnectFromServer()
            {
                Debug.Log("LWNetwork: Lost server connection.");
                //Probably add some delegate as event here if needed
            }

            /// <summary>
            /// Called when the local user/client left a room.
            /// </summary>
            public static void OnLeftRoom()
            {
                Debug.Log("LWNetwork: User have left the room.");
                if (SceneManager.GetActiveScene().buildIndex != 0)
                {
                    Debug.Log("LWNetwork: Loading fisrt scene.");
                    SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
                }
            }

            /// <summary>
            ///  Invoked when a new player joined the room
            /// </summary>
            /// <param name="_playerIndex">Connected player index</param>
            public static void OnPlayerConnected(int _playerIndex)
            {
                Debug.Log("LWNetwork: A new player connected to the room.");
                //Probably add some delegate as event here if needed
            }

            /// <summary>
            /// Invoked when a player left the room
            /// </summary>
            public static void OnPlayerDisconnected(int _playerIndex)
            {

                //Perform some cleanup task

                #region Remove instance from networkObjectInstances
                //Instance that need to be removed from scene
                List<string> _removingInstance = new List<string>();
                foreach (string _keys in networkObjectInstances.Keys)
                {
                    Debug.Log(_keys + "," + networkObjectInstances.Keys.ToString() + ", name: " + networkObjectInstances[_keys].Instance.name);
                    if (networkObjectInstances[_keys].ObjInfo.NetworkID.Owner == _playerIndex)
                    {
                        _removingInstance.Add(_keys);
                    }
                }

                foreach (string _rk in _removingInstance)
                {
                    Debug.Log("Cleaning up player's FFF");
                    networkObjectInstances[_rk].DestroyInstance();
                    networkObjectInstances.Remove(_rk);
                }
                #endregion

                OnPlayerDisconnectedDelegate(_playerIndex);
            }

            public static void OnMasterClientChanged(int newMasterClientID)
            {
                Debug.Log("Master client switched, new master client ID: " + newMasterClientID);
                if (onMasterClientSwitchedHandlers != null)
                    onMasterClientSwitchedHandlers(newMasterClientID);
            }
        }
    }
}