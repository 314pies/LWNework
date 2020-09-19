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
    /// Uses as key to access target method
    /// </summary>
    public struct MethodKey
    {
        public MethodKey(int _ownerIndex, byte _viewid, byte _functionid)
        {
            ownerId = _ownerIndex;
            viewid = _viewid;
            functionid = _functionid;
        }
        public int ownerId;
        byte viewid;
        byte functionid;
        public override string ToString()
        {
            return "MethodKey: " + ownerId + "." + viewid + "." + functionid;
        }
    }
    /// <summary>
    /// The format for caching target method
    /// </summary>
    public struct MethodPack
    {
        public MethodPack(object _instance, MethodInfo _methodInfo, Type[] _types)
        {
            instance = _instance;
            method = _methodInfo;
            types = _types;
        }
        /// <summary>
        /// The instance contain this method
        /// </summary>
        public object instance;
        /// <summary>
        /// The method
        /// </summary>
        public MethodInfo method;
        /// <summary>
        /// The parameters type for this functions
        /// </summary>
        public Type[] types;
    }

    public static partial class LWNetwork
    {
        /// <summary>
        /// The starting index for fast rpc decoding
        /// </summary>
        public const int FastRPCDedodeIndex = 6;
        public static partial class Core
        {
            /// <summary>
            /// Get the method(RPC) by ProcessKey
            /// </summary>
            public static Dictionary<MethodKey, MethodPack> RPCMethodDic = new Dictionary<MethodKey, MethodPack>();
            /// <summary>
            /// Get the method(FastRPC) by ProcessKey
            /// </summary>
            public static Dictionary<MethodKey, Action<byte[]>> FastRPCMethodDic = new Dictionary<MethodKey, Action<byte[]>>();

            /// <summary>
            /// Send a RPC byte array
            /// </summary>
            /// <param name="_targetPlayers">The target player indexs gonna sent to, will replace _target if this is not null</param>
            /// <param name="_target">The target gonna sent to, will be replace by _targetPlayers if its not null</param>
            /// <param name="_IsReliable">Is this message reliable</param>
            /// <param name="_owner">who is the owner of this LWview</param>
            /// <param name="_viewID">Which view id is it?</param>
            /// <param name="_functionID">The function id which will be define under LWview</param>
            /// <param name="_params">The parameters gonna be sent</param>
            public static void SendRPC(int[] _targetPlayers, Reciever _target, bool _IsReliable, int _owner, byte _viewID, byte _functionID, object[] _params)
            {
                byte[] _identity = BitConverter.GetBytes((System.Int32)(ByteArrayIdentifier.RPC));
                byte[] _playerID = BitConverter.GetBytes((System.Int32)_owner);
                byte[] paramsPack = SerializePar(_params);

                byte[] PackedData = new byte[10 + paramsPack.Length];
                System.Buffer.BlockCopy(_identity, 0, PackedData, 0, _identity.Length);
                System.Buffer.BlockCopy(_playerID, 0, PackedData, 4, _playerID.Length);
                PackedData[8] = _viewID; PackedData[9] = _functionID;
                System.Buffer.BlockCopy(paramsPack, 0, PackedData, 10, paramsPack.Length);  //10=4+4+1+1

                SendByteArray(_IsReliable, _target, null, PackedData);
            }

            /// <summary>
            /// Decode a byte array recieved remotely
            /// </summary>
            private static void RPCDecoding(byte[] _data)
            {
                int _owner = BitConverter.ToInt32(_data, 4);
                MethodKey _key = new MethodKey(_owner, _data[8], _data[9]);
                if (!RPCMethodDic.ContainsKey(_key))
                {
                    Debug.LogError("RPC " + _key.ToString() + " is not registered");
                    return;
                }
                MethodPack _targetMethod = RPCMethodDic[_key];
                object[] _prameters = DeserializePar(_data, 10, _targetMethod.types);
                _targetMethod.method.Invoke(_targetMethod.instance, _prameters);
            }

            /// <summary>
            /// Send a Fast RPC byte array
            /// </summary>
            /// <param name="_IsReliable">Is this message reliable</param>
            /// <param name="_owner">who is the owner of this LWview</param>
            /// <param name="_viewID">Which view id is it?</param>
            /// <param name="_functionID">The function id which will be define under LWview</param>
            /// <param name="_params">The parameters gonna be sent, which need to be converted to byte codes</param>
            public static void SendFastRPC(Reciever _target, bool _IsReliable, int _owner, byte _viewID, byte _functionID, byte[] _params)
            {
                SendByteArray(_IsReliable, _target, null,
                    PackFastRPCData(_owner, _viewID, _functionID, _params));
            }
            /// <summary>
            /// Send a Fast RPC byte array to specific player
            /// </summary>
            /// <param name="_IsReliable">Is this message reliable</param>
            /// <param name="_owner">who is the owner of this LWview</param>
            /// <param name="_viewID">Which view id is it?</param>
            /// <param name="_functionID">The function id which will be define under LWview</param>
            /// <param name="_params">The parameters gonna be sent, which need to be converted to byte codes</param>
            public static void SendFastRPC(int[] _targetPlayers, bool _IsReliable, int _owner, byte _viewID, byte _functionID, byte[] _params)
            {
                SendByteArray(_IsReliable, Reciever.Empty, _targetPlayers,
                    PackFastRPCData(_owner, _viewID, _functionID, _params));
            }

            private static byte[] PackFastRPCData(int _owner, byte _viewID, byte _functionID, byte[] _params)
            {
                int _paramsLegnth = 0;
                if (_params != null)
                    _paramsLegnth = _params.Length;

                byte[] PackedData = new byte[FastRPCDedodeIndex + _paramsLegnth];
                System.Buffer.BlockCopy(
                    BitConverter.GetBytes((System.Int32)_owner)
                    , 0, PackedData, 0, 4);
                PackedData[4] = _viewID; PackedData[5] = _functionID;

                if (_params != null)
                    System.Buffer.BlockCopy(_params, 0, PackedData, FastRPCDedodeIndex, _params.Length);  //6=4+1+1 

                return PackedData;
            }

            /// <summary>
            /// Decode a byte array which representing Fast RPC
            /// </summary>
            /// <param name="_data">The raw data</param>
            private static void FastRPCDecoding(int _owner, byte[] _data)
            {
                MethodKey _key = new MethodKey(_owner, _data[4], _data[5]);
                if (FastRPCMethodDic.ContainsKey(_key))
                {
                    byte[] _methodData = new byte[_data.Length - FastRPCDedodeIndex];

                    if (_methodData.Length != 0)
                    {
                        System.Buffer.BlockCopy(
                            _data, FastRPCDedodeIndex,
                            _methodData, 0,
                            _methodData.Length);
                    }
                    else
                        _methodData = null;

                    FastRPCMethodDic[_key](_methodData);
                }
                else
                    Debug.LogError("FastRPC " + _key.ToString() + " is not registered");
            }
        }
    }
}
