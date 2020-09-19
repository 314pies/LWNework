using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace LWNet
{
    /// <summary>
    /// Caching for Common variable Type
    /// </summary>
    public static class CommonType
    {
        public readonly static Type TypeInt = typeof(int);
        public readonly static Type TypeByte = typeof(byte);
        public readonly static Type TypeFloat = typeof(float);
        public readonly static Type TypeBool = typeof(bool);
        public readonly static Type TypeString = typeof(string);
        public readonly static Type TypeVector3 = typeof(Vector3);
        public readonly static Type TypeQuaternion = typeof(Quaternion);
        public readonly static Type TypeByteArray = typeof(byte[]);
    }
}

namespace LWNet.NetorkUtilities
{
    /// <summary>
    /// Utilities to be used in LWNetwork
    /// </summary>
    public static class LWNetUtilities
    {
        /// <summary>
        /// Is this syn variable key pass through repersenting network game object?
        /// </summary>
        /// <param name="key">The key of this sync-var</param>
        /// <param name="value">The value of this sync-var</param>
        /// <returns></returns>
        public static bool IsNetworkObjectProperties(object key,object value)
        {
            string _key = (string)key;
            //Debug.Log(_key + ", " + value);

            if (_key.Length > SVK.NGO.Length)
            {
                if (_key.Substring(0, SVK.NGO.Length) == SVK.NGO)
                {
                    if (value == null)
                        return true;
                    if (value.GetType() == typeof(NetworkObjectInfo))
                        return true;
                }
            }
            return false;
        }
    }
    /// <summary>
    /// Event handlers when sync-var is update
    /// </summary>
    public class PlayerSynvVarUpdateEvent
    {
        Dictionary<string, List<Action<int, object>>> dict = new Dictionary<string, List<Action<int, object>>>();

        public void Invoke(string key, int playerID, object value)
        {
            if (dict.ContainsKey(key))
            {
                foreach (Action<int, object> action in dict[key])
                {
                    action(playerID, value);
                }
            }
        }

        public void Sub(string key, Action<int, object> eventHandler)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key,
                    new List<Action<int, object>>() { eventHandler }
                    );
            }
            else
            {
                dict[key].Add(eventHandler);
            }
        }

        public void UnSub(string key, Action<int, object> eventHandler)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Remove(eventHandler);
                if (dict[key].Count == 0)
                {
                    dict.Remove(key);
                }
            }
        }
    }

    public class RoomSynvVarUpdateEvent
    {
        Dictionary<string, List<Action<object>>> dict = new Dictionary<string, List<Action<object>>>();

        public void Invoke(string key, object value)
        {
            if (dict.ContainsKey(key))
            {
                foreach (Action<object> action in dict[key])
                {
                    action(value);
                }
            }
        }

        public void Sub(string key, Action<object> eventHandler)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key,
                    new List<Action<object>>() { eventHandler }
                    );
            }
            else
            {
                dict[key].Add(eventHandler);
            }
        }

        public void UnSub(string key, Action<object> eventHandler)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Remove(eventHandler);
                if (dict[key].Count == 0)
                {
                    dict.Remove(key);
                }
            }
        }
    }
}
