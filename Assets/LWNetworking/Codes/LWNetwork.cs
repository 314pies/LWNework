using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace LWNetworking
{
    public class LWNetwork
    {

        /// <summary>
        /// Local player networking id
        /// </summary>
        static public int playerID { get { return playerID; } }
        /// <summary>
        /// Encode RPC info to byte array and send it
        /// </summary>
        /// <param name="_viewID"></param>
        /// <param name="_functionID"></param>
        static public void SendRPC(bool _IsReliable,byte _viewID, byte _functionID, ref object[] _params)
        {
            byte[] _playerID = BitConverter.GetBytes((System.Int32)playerID);
            //viewID
            //functionID
            byte[] paramsPack = ParamsToByteArray(ref _params);


            //6=4+1+1
            byte[] PackedData = new byte[6+ paramsPack.Length];
            System.Buffer.BlockCopy(_playerID, 0, PackedData, 0, _playerID.Length);
            PackedData[4] = _viewID; PackedData[5] = _functionID;
            System.Buffer.BlockCopy(paramsPack, 0, PackedData, 6, paramsPack.Length);
            //Send(PackedData);
        }

        static private byte[] ParamsToByteArray(ref object[] _params)
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
                    System.Buffer.BlockCopy(_newStrLegnth, 0, _convertedPar, 0, _newStrLegnth.Length);
                    System.Buffer.BlockCopy(_newStr, 0, _convertedPar, 4, _newStr.Length);
                }
                System.Buffer.BlockCopy(_convertedPar, 0, FinalArray, Legnth, _convertedPar.Length);
                Legnth += _convertedPar.Length;
            }
            Array.Resize(ref FinalArray, Legnth);
            return FinalArray;
        }
    }
}
