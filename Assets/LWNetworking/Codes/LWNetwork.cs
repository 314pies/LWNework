using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LWNetworking
{
    /// <summary>
    /// Uses as key to access target process
    /// </summary>
    public struct ProcessKey
    {
        public ProcessKey(int _playerindex, byte _viewid, byte _functionid)
        {
            playerindex = _playerindex;
            viewid = _viewid;
            functionid = _functionid;
        }
        int playerindex;
        byte viewid;
        byte functionid;
    }

    /// <summary>
    /// The format for caching target method
    /// </summary>
    public struct TargetMethod
    {
        /// <summary>
        /// The instance contain this method
        /// </summary>
        public object instance;
        /// <summary>
        /// The method
        /// </summary>
        public MethodInfo method;
        public Type[] types;
    }

    public class LWNetwork
    {

        /// <summary>
        /// Local player networking id
        /// </summary>
        static public int playerID = -1;
        /// <summary>
        /// Encode RPC info to byte array and send it
        /// </summary>
        /// <param name="_viewID"></param>
        /// <param name="_functionID"></param>
        static public void SendRPC(bool _IsReliable, byte _viewID, byte _functionID, object[] _params)
        {
            byte[] _playerID = BitConverter.GetBytes((System.Int32)playerID);
            byte[] paramsPack = ParamsToByteArray(_params);

            byte[] PackedData = new byte[6 + paramsPack.Length];
            System.Buffer.BlockCopy(_playerID, 0, PackedData, 0, _playerID.Length);
            PackedData[4] = _viewID; PackedData[5] = _functionID;
            System.Buffer.BlockCopy(paramsPack, 0, PackedData, 6, paramsPack.Length);  //6=4+1+1
                                                                                       // MonoBehaviour.print(PackedData.Length);
        }
        static private byte[] ParamsToByteArray(object[] _params)
        {
            byte[] FinalArray = new byte[1000];
            int Legnth = 0;
            byte[] _convertedPar = new byte[0];
            foreach (object _obj in _params)
            {
                if (_obj.GetType() == typeof(int))
                {
                    _convertedPar = BitConverter.GetBytes((System.Int32)_obj);
                }
                else if (_obj.GetType() == typeof(float))
                {
                    _convertedPar = BitConverter.GetBytes((System.Single)_obj);
                }
                else if (_obj.GetType() == typeof(bool))
                {
                    _convertedPar = BitConverter.GetBytes((System.Boolean)_obj);
                }
                else if (_obj.GetType() == typeof(string))
                {
                    byte[] _newStr = System.Text.UnicodeEncoding.Unicode.GetBytes((string)_obj);
                    byte[] _newStrLegnth = BitConverter.GetBytes((System.Int32)_newStr.Length);
                    _convertedPar = new byte[_newStr.Length + 4];
                    System.Buffer.BlockCopy(_newStrLegnth, 0, _convertedPar, 0, _newStrLegnth.Length);
                    System.Buffer.BlockCopy(_newStr, 0, _convertedPar, 4, _newStr.Length);
                }
                else
                {
                    Debug.LogError(_obj.GetType() + " is not a supported type.");
                }
                System.Buffer.BlockCopy(_convertedPar, 0, FinalArray, Legnth, _convertedPar.Length);
                Legnth += _convertedPar.Length;
            }
            Array.Resize(ref FinalArray, Legnth);
            return FinalArray;
        }
        static public void OnRecieveByteArray(byte[] _data)
        {
            ProcessKey _key = new ProcessKey(BitConverter.ToInt32(_data, 0), _data[4], _data[5]);
            if (RPCMethodDic.ContainsKey(_key))
            {
                TargetMethod _targetMethod = RPCMethodDic[_key];
                _targetMethod.method.Invoke(_targetMethod.instance,new object[1]);
            }
        }

       
        /// <summary>
        /// Get the method by ProcessKey
        /// </summary>
        static Dictionary<ProcessKey, TargetMethod> RPCMethodDic;
    }
}
