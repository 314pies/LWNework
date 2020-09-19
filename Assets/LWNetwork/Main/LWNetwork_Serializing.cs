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
    /// Serializing type
    /// </summary>
    public enum sType
    {
        Int,
        Byte,
        UShort,
        Float,
        Bool,
        String,
        Vector3,
        Quaternion,
        ByteArray
    }
    public static partial class LWNetwork
    {
        /// <summary>
        /// This will accurately preallocate array size before serializing
        /// Convert parameters to byte array.
        /// Note: Null value is NOT support
        /// </summary>
        /// <param name="bytesSize">The size of the serialized bytes array, if it's known, giving this parameter can optimize memory allocating performance</param>
        /// <param name="_params"></param>
        /// <returns></returns>
        public static byte[] PreAllocSerializePar(int bytesSize, params object[] _params)
        {
            byte[] FinalArray = new byte[bytesSize];
            int Legnth = 0;
            byte[] _convertedPar = new byte[0];
            foreach (object _obj in _params)
            {
                if (_obj.GetType() == typeof(int))
                {
                    _convertedPar = BitConverter.GetBytes((System.Int32)_obj);
                }
                else if (_obj.GetType() == typeof(byte))
                {
                    _convertedPar = new byte[1] { (byte)_obj };
                }
                else if (_obj.GetType() == typeof(float))
                {
                    _convertedPar = BitConverter.GetBytes((System.Single)_obj);
                }
                else if (_obj.GetType() == typeof(ushort))
                {
                    _convertedPar = BitConverter.GetBytes((ushort)_obj);
                }
                else if (_obj.GetType() == typeof(bool))
                {
                    _convertedPar = BitConverter.GetBytes((System.Boolean)_obj);
                }
                else if (_obj.GetType() == typeof(string))
                {
                    _convertedPar = Core.StringToByteArray((string)_obj);
                }
                else if (_obj.GetType() == typeof(Vector3))
                {
                    _convertedPar = Core.Vector3ToByteArray((Vector3)_obj);
                }
                else if (_obj.GetType() == typeof(Quaternion))
                {
                    _convertedPar = Core.QuaternionToByteArray((Quaternion)_obj);
                }
                else if (_obj.GetType() == typeof(byte[]))
                {
                    _convertedPar = Core.ByteArrayToHeaderedByteArray((byte[])_obj);
                }
                else
                {
                    Debug.LogError(_obj.GetType() + " is not a supported type.");
                }
                //Resize FinalArray if it's not big enough
                if (Legnth + _convertedPar.Length >= FinalArray.Length)
                    Array.Resize(ref FinalArray, Legnth + _convertedPar.Length + 128);

                System.Buffer.BlockCopy(_convertedPar, 0, FinalArray, Legnth, _convertedPar.Length);
                Legnth += _convertedPar.Length;
            }

            if (FinalArray.Length != Legnth)
                Array.Resize(ref FinalArray, Legnth);

            return FinalArray;
        }

        /// <summary>
        /// Convert parameters to byte array.
        /// Note: Null value is NOT support
        /// </summary>
        /// <param name="_params">parameters</param>
        /// <returns></returns>
        public static byte[] SerializePar(params object[] _params)
        {
            //Use 512 as default bytes array size
            return PreAllocSerializePar(512, _params);
        }

        /// <summary>
        /// Convert byte array to parameters
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static object[] DeserializePar(byte[] _data, int _startIndex, params sType[] _ConvertType)
        {
            if (_data == null) { return null; }
            object[] _prameters = new object[_ConvertType.Length];

            int StartIndex = _startIndex;//Where to start decoding
            for (int i = 0; i < _ConvertType.Length; i++)
            {
                sType _type = _ConvertType[i];
                if (_type == sType.Int)
                {
                    _prameters[i] = BitConverter.ToInt32(_data, StartIndex);
                    StartIndex += 4;
                }
                else if (_type == sType.Byte)
                {
                    _prameters[i] = _data[StartIndex];
                    StartIndex += 1;
                }
                else if (_type == sType.Float)
                {
                    _prameters[i] = BitConverter.ToSingle(_data, StartIndex);
                    StartIndex += 4;
                }
                else if (_type == sType.UShort)
                {
                    _prameters[i] = BitConverter.ToUInt16(_data, StartIndex);
                    StartIndex += 2;
                }
                else if (_type == sType.Bool)
                {
                    _prameters[i] = BitConverter.ToBoolean(_data, StartIndex);
                    StartIndex += 1;
                }
                else if (_type == sType.String)
                {
                    _prameters[i] = Core.ByteArrayToString(_data, StartIndex);
                    int _strByteLength = BitConverter.ToInt32(_data, StartIndex);//The length of string byte[]
                    StartIndex += (4 + _strByteLength);
                }
                else if (_type == sType.Vector3)
                {
                    _prameters[i] = Core.ByteArrayToVector3(_data, StartIndex);
                    StartIndex += 12;
                }
                else if (_type == sType.Quaternion)
                {
                    _prameters[i] = Core.ByteArrayToQuaternion(_data, StartIndex);
                    StartIndex += 16;
                }
                else if (_type == sType.ByteArray)
                {
                    _prameters[i] = Core.HeaderedByteArrayToByteArray(_data, StartIndex);
                    int _strByteLength = BitConverter.ToInt32(_data, StartIndex);//The length of byte[]
                    StartIndex += (4 + _strByteLength);
                }
            }
            return _prameters;
        }

        /// <summary>
        /// Convert byte array to parameters
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static object[] DeserializePar(byte[] _data, params sType[] _ConvertType)
        {
            return DeserializePar(_data, 0, _ConvertType);
        }

        /// <summary>
        /// Convert byte array to parameters
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        [Obsolete]
        public static object[] DeserializePar(byte[] _data, int _startIndex, params Type[] _ConvertType)
        {
            object[] _prameters = new object[_ConvertType.Length];

            int StartIndex = _startIndex;//Where to start decoding
            for (int i = 0; i < _ConvertType.Length; i++)
            {
                Type _type = _ConvertType[i];
                if (_type == typeof(int))
                {
                    _prameters[i] = BitConverter.ToInt32(_data, StartIndex);
                    StartIndex += 4;
                }
                else if (_type == typeof(byte))
                {
                    _prameters[i] = _data[StartIndex];
                    StartIndex += 1;
                }
                else if (_type == typeof(float))
                {
                    _prameters[i] = BitConverter.ToSingle(_data, StartIndex);
                    StartIndex += 4;
                }
                else if (_type == typeof(bool))
                {
                    _prameters[i] = BitConverter.ToBoolean(_data, StartIndex);
                    StartIndex += 1;
                }
                else if (_type == typeof(string))
                {
                    _prameters[i] = Core.ByteArrayToString(_data, StartIndex);
                    int _strByteLength = BitConverter.ToInt32(_data, StartIndex);//The length of string byte[]
                    StartIndex += (4 + _strByteLength);
                }
                else if (_type == typeof(Vector3))
                {
                    _prameters[i] = Core.ByteArrayToVector3(_data, StartIndex);
                    StartIndex += 12;
                }
                else if (_type == typeof(Quaternion))
                {
                    _prameters[i] = Core.ByteArrayToQuaternion(_data, StartIndex);
                    StartIndex += 16;
                }
                else if (_type == typeof(byte[]))
                {
                    _prameters[i] = Core.HeaderedByteArrayToByteArray(_data, StartIndex);
                    int _strByteLength = BitConverter.ToInt32(_data, StartIndex);//The length of byte[]
                    StartIndex += (4 + _strByteLength);
                }
            }
            return _prameters;
        }

        /// <summary>
        /// Convert byte array to parameters as a FastRPC array(data will start at FastRPCDedodeIndex position)
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        [Obsolete]
        public static object[] DeserializeAsFastRPCBytes(byte[] _data, params Type[] _ConvertType)
        {
            return DeserializePar(_data, FastRPCDedodeIndex, _ConvertType);
        }

        /// <summary>
        /// Custom serialization method written for LWNetwork
        /// </summary>
        public static partial class Core
        {
            /// <summary>
            /// Convert Vector3 to byte[]
            /// </summary>
            /// <returns></returns>
            public static byte[] Vector3ToByteArray(Vector3 _vector3)
            {
                byte[] _returnByteArray = new byte[12];
                byte[] _x = BitConverter.GetBytes(_vector3.x);
                byte[] _y = BitConverter.GetBytes(_vector3.y);
                byte[] _z = BitConverter.GetBytes(_vector3.z);

                System.Buffer.BlockCopy(_x, 0, _returnByteArray, 0, 4);
                System.Buffer.BlockCopy(_y, 0, _returnByteArray, 4, 4);
                System.Buffer.BlockCopy(_z, 0, _returnByteArray, 8, 4);
                return _returnByteArray;
            }
            /// <summary>
            /// Convert byte array to Vector3
            /// </summary>
            /// <param name="_decodedArray">Target array</param>
            /// <param name="_startIndex">Start decode index</param>
            /// <returns></returns>
            public static Vector3 ByteArrayToVector3(byte[] _decodedArray, int _startIndex)
            {
                return new Vector3(
                            BitConverter.ToSingle(_decodedArray, _startIndex),
                            BitConverter.ToSingle(_decodedArray, _startIndex + 4),
                            BitConverter.ToSingle(_decodedArray, _startIndex + 8)
                        );
            }
            /// <summary>
            /// Convert Quaternion to byte array
            /// </summary>
            /// <param name="_rot">rotation</param>
            /// <returns></returns>
            public static byte[] QuaternionToByteArray(Quaternion _rot)
            {
                byte[] _convertedByteArray = new byte[16];
                byte[] _x = BitConverter.GetBytes(_rot.x);
                byte[] _y = BitConverter.GetBytes(_rot.y);
                byte[] _z = BitConverter.GetBytes(_rot.z);
                byte[] _w = BitConverter.GetBytes(_rot.w);

                System.Buffer.BlockCopy(_x, 0, _convertedByteArray, 0, 4);
                System.Buffer.BlockCopy(_y, 0, _convertedByteArray, 4, 4);
                System.Buffer.BlockCopy(_z, 0, _convertedByteArray, 8, 4);
                System.Buffer.BlockCopy(_w, 0, _convertedByteArray, 12, 4);
                return _convertedByteArray;
            }
            /// <summary>
            /// Contert byte array to Quaternion
            /// </summary>
            /// <returns></returns>
            public static Quaternion ByteArrayToQuaternion(byte[] _data, int _startIndex)
            {
                return new Quaternion(
                            BitConverter.ToSingle(_data, _startIndex),
                            BitConverter.ToSingle(_data, _startIndex + 4),
                            BitConverter.ToSingle(_data, _startIndex + 8),
                            BitConverter.ToSingle(_data, _startIndex + 12)
                        );
            }
            /// <summary>
            /// Convert string to byte array
            /// </summary>
            /// <returns></returns>
            public static byte[] StringToByteArray(string _targetString)
            {

                byte[] _newStr = System.Text.UnicodeEncoding.Unicode.GetBytes(_targetString);
                byte[] _newStrLegnth = BitConverter.GetBytes((System.Int32)_newStr.Length);
                byte[] _returnArray = new byte[_newStr.Length + 4];
                System.Buffer.BlockCopy(_newStrLegnth, 0, _returnArray, 0, _newStrLegnth.Length);
                System.Buffer.BlockCopy(_newStr, 0, _returnArray, 4, _newStr.Length);
                return _returnArray;
            }
            /// <summary>
            /// Convert byte array to string 
            /// </summary>
            /// <returns></returns>
            public static string ByteArrayToString(byte[] _data, int _startIndex)
            {
                int Legnth = BitConverter.ToInt32(_data, _startIndex);
                _startIndex += 4;
                byte[] StringByte = new byte[Legnth];
                System.Buffer.BlockCopy(_data, _startIndex, StringByte, 0, Legnth);
                string _returnString = System.Text.Encoding.Unicode.GetString(StringByte);
                return _returnString;
            }

            /// <summary>
            /// Byte array with 2 byte at the first 2 index which indicating the length(Int16) 
            /// </summary>
            /// <param name="_arr">byte array</param>
            /// <returns></returns>
            public static byte[] ByteArrayToHeaderedByteArray(byte[] _arr)
            {
                byte[] HeaderedByte = new byte[_arr.Length + 4];

                int ByteSize = _arr.Length;
                byte[] _size = BitConverter.GetBytes((System.Int32)ByteSize);
                HeaderedByte[0] = _size[0]; HeaderedByte[1] = _size[1];
                HeaderedByte[2] = _size[2]; HeaderedByte[3] = _size[3];

                System.Buffer.BlockCopy(_arr, 0, HeaderedByte, 4, _arr.Length);
                return HeaderedByte;
            }

            public static byte[] HeaderedByteArrayToByteArray(byte[] _data, int _startIndex)
            {
                int _legnth = BitConverter.ToInt32(_data, _startIndex);
                byte[] _arr = new byte[_legnth];
                System.Buffer.BlockCopy(_data, _startIndex + 4, _arr, 0, _legnth);
                return _arr;
            }
        }
    }
}