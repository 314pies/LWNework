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
    /// <summary>
    /// Some preset message recieve groups
    /// </summary>
    public enum Reciever
    {
        /// <summary>
        /// All client (include itself) will get this message
        /// </summary>
        All,
        /// <summary>
        /// Clients exclude itself will get this message
        /// </summary>
        Others,
        /// <summary>
        /// Send to master client
        /// </summary>
        MasterClient,
        /// <summary>
        /// Leave it empty
        /// </summary>
        Empty
    }

    public static partial class LWNetwork
    {        
        public static partial class Core
        {
            

            /// <summary>
            /// Send a encoded byte array
            /// </summary>
            /// <param name="IsReliable"></param>
            /// <param name="_RecieveGroup">The groups that will get this message. Will be ignored if _Recievers have actual instance(not null)</param>
            /// <param name="_Recievers">The specific users that will get this data. It will replace _RecieveGroup if it have actual instance(not null)</param>
            /// <param name="PackedData"></param>
            private static void SendByteArray(bool IsReliable, Reciever _RecieveGroup, int[] _Recievers, byte[] PackedData)
            {
                if (networkMode == NetworkMode.Unet)
                {
                    LWLanManager.SendByteArray(IsReliable, _RecieveGroup, _Recievers, PackedData);
                }
                else if (networkMode == NetworkMode.Offline)
                {
                    OnRecieveByteArray(PackedData);
                }
            }

            /// <summary>
            /// For indicate the purpose of this byte array while LWNetwork recieve a raw byte array
            /// </summary>
            public static class ByteArrayIdentifier
            {
                public const int RPC = -5;
            }
            /// <summary>
            /// Decode recieved byte code, got from multi-player controller
            /// </summary>
            /// <param name="_data">Encoded raw data</param>
            public static void OnRecieveByteArray(byte[] _data)
            {
                if (!IsInitialized) { Debug.Log("LWNetwork not initialized yet. Refuse receiving bytes"); return; }
                //Use for determinding what this byte array do
                int _identifier = BitConverter.ToInt32(_data, 0);

                //This identifier repersent a player index of FastRPC if its >=-1 
                if (_identifier > -2)
                {
                    FastRPCDecoding(_identifier, _data);
                }
                else if (_identifier == (System.Int32)ByteArrayIdentifier.RPC)
                {
                    RPCDecoding(_data);
                }
            }
        }
    }
}
