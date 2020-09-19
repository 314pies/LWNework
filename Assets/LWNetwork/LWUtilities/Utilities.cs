
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace LWUtilities
{
    namespace CommonDelegate
    {
        /// <summary>
        /// Delegate which don't have parameter
        /// </summary>
        public delegate void LWEvent_NoParam();
        /// <summary>
        /// Delegate with one int as parameter
        /// </summary>
        /// <param name="p0"></param>
        public delegate void LWEvent_Int(int p0);
        public delegate void LWEvent_Int_Int(int p0, int p1);
    }


    /// <summary>
    /// Since unity doesn't flag the Vector3 as serializable, we
    /// need to create our own version. This one will automatically convert
    /// between Vector3 and SerializableVector3
    /// </summary>
    [System.Serializable]
    public struct SerializableVector3
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }

        /// <summary>
        /// Automatic conversion from SerializableVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SerializableVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    }
    /// <summary>
    /// Since unity doesn't flag the Quaternion as serializable, we
    /// need to create our own version. This one will automatically convert
    /// between Quaternion and SerializableQuaternion
    /// </summary>
    [System.Serializable]
    public struct SerializableQuaternion
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// w component
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        /// <param name="rW"></param>
        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        /// <summary>
        /// Automatic conversion from SerializableQuaternion to Quaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Quaternion(SerializableQuaternion rValue)
        {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        /// <summary>
        /// Automatic conversion from Quaternion to SerializableQuaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableQuaternion(Quaternion rValue)
        {
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }


    public static class Utilities
    {
        /// <summary>
        /// Convert object to "string" (ex: int(30) to "30")
        /// </summary>
        /// <param name="value">object to convert</param>
        /// <returns></returns>
        public static string ObjectToString(object value)
        {
            string _vakueStr = "?";

            Debug.Log(value.GetType());


            if (value.GetType() == typeof(string))
                _vakueStr = (string)value;
            else if (value.GetType() == typeof(int))
                _vakueStr = "" + (int)value;
            else if (value.GetType() == typeof(float))
                _vakueStr = "" + (float)value;
            else if (value.GetType() == typeof(double))
                _vakueStr = "" + (double)value;
            else if (value.GetType() == typeof(bool))
                _vakueStr = "" + (bool)value;

            return _vakueStr;
        }

        /// <summary>
        /// Instantiate game object with resource path
        /// </summary>
        /// <param name="_path">path under resource folder</param>
        /// <param name="_pos">Initialize position</param>
        /// <param name="_rot">Initialize rotation</param>
        /// <returns></returns>
        public static GameObject InstantiateWithpath(string _path, Vector3 _pos, Quaternion _rot)
        {
            UnityEngine.Object _loadedObject = Resources.Load(_path);
            GameObject _SpawnedObj = UnityEngine.Object.Instantiate(_loadedObject) as GameObject;
            _SpawnedObj.transform.position = _pos;
            _SpawnedObj.transform.rotation = _rot;
            return _SpawnedObj;
        }

        /// <summary>
        /// Use to trim string return by AssetDatabase.GetAssetPath to remove path before "Resource" text
        /// </summary>
        /// <param name="Clip"></param>
        public static string GetAssetPath_ClipString(string Clip)
        {
            bool IsTargetStrFound = false;
            //Keep path under resources folder
            for (int i = 0; i < Clip.Length - 9; i++)
            {
                if (
                   Clip[i] == 'R' &&
                   Clip[i + 1] == 'e' &&
                   Clip[i + 2] == 's' &&
                   Clip[i + 3] == 'o' &&
                   Clip[i + 4] == 'u' &&
                   Clip[i + 5] == 'r' &&
                   Clip[i + 6] == 'c' &&
                   Clip[i + 7] == 'e' &&
                   Clip[i + 8] == 's' &&
                   Clip[i + 9] == '/'
                )
                {
                    Clip = Clip.Remove(0, i + 10);
                    IsTargetStrFound = true;
                    break;
                }
            }

            if (!IsTargetStrFound) return null;

            for (int i = Clip.Length - 1; i > 0; i--)
            {
                if (Clip[i] == '.')
                {
                    Clip = Clip.Remove(i, Clip.Length - i);
                    break;
                }
            }
            return Clip;
        }
    }

    public class GenericSerialization
    {

        // Convert an object to a byte array
        static public byte[] ObjectToByteArray(System.Object obj)
        {
            //Need support null expression
            if (obj == null)
                return new byte[0];//Return a byte[] with length 0 for null

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        static public System.Object ByteArrayToObject(byte[] arrBytes)
        {
            if (arrBytes == null)
                return null;
            if (arrBytes.Length == 0)
                return null;

            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);

            return obj;
        }

        /// <summary>
        /// Get the actual byte segment of byte array and convert the byte array to an Object
        /// </summary>
        /// <param name="ByteArrayFromFastRPC"></param>
        /// <returns></returns>
        static public System.Object FastRPCByteArrayToObject(byte[] ByteArrayFromFastRPC)
        {
            int _ActualLegnth = ByteArrayFromFastRPC.Length - LWNet.LWNetwork.FastRPCDedodeIndex;

            byte[] actualByte = new byte[_ActualLegnth];
            Array.Copy(ByteArrayFromFastRPC, 6, actualByte, 0, _ActualLegnth);

            return ByteArrayToObject(actualByte);
        }


    }
}