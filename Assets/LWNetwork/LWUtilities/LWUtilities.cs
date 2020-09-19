//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.IO;

//namespace LWUtilities
//{

//    public static class Utilities
//    {

//        /// <summary>
//        /// Set mouse locking(lock -> hide mouse) status
//        /// </summary>
//        /// <param name="_status">true = lock, false = unlock</param>
//        public static void SetMouseLockStatus(bool _status)
//        {
//            if (!_status)
//            {
//                Cursor.visible = true;
//                Cursor.lockState = CursorLockMode.None;
//            }
//            else
//            {
//                Cursor.visible = false;
//                Cursor.lockState = CursorLockMode.Locked;
//            }
//        }

//        /// <summary>
//        /// Use to trim string return by AssetDatabase.GetAssetPath to remove path before "Resource" text
//        /// </summary>
//        /// <param name="Clip"></param>
//        public static string GetAssetPath_ClipString(string Clip)
//        {
//            bool IsTargetStrFound = false;
//            //Keep path under resources folder
//            for (int i = 0; i < Clip.Length - 9; i++)
//            {
//                if (
//                   Clip[i] == 'R' &&
//                   Clip[i + 1] == 'e' &&
//                   Clip[i + 2] == 's' &&
//                   Clip[i + 3] == 'o' &&
//                   Clip[i + 4] == 'u' &&
//                   Clip[i + 5] == 'r' &&
//                   Clip[i + 6] == 'c' &&
//                   Clip[i + 7] == 'e' &&
//                   Clip[i + 8] == 's' &&
//                   Clip[i + 9] == '/'
//                )
//                {
//                    Clip = Clip.Remove(0, i + 10);
//                    IsTargetStrFound = true;
//                    break;
//                }
//            }

//            if (!IsTargetStrFound) return null;

//            for (int i = Clip.Length - 1; i > 0; i--)
//            {
//                if (Clip[i] == '.')
//                {
//                    Clip = Clip.Remove(i, Clip.Length - i);
//                    break;
//                }
//            }
//            return Clip;
//        }

//        /// <summary>
//        /// Delete all child objects under the root and instantiate a list of prefabs according to the array pass through
//        /// </summary>
//        /// <param name="prefab">The prefab you want to be instantiated</param>
//        /// <param name="root">The parent gameobject of all instantiated prefabs</param>
//        /// <param name="list">Array to iterate over and instantiated prefabs accodring to it</param>
//        /// <param name="action">Statement want to be run after prefab instantiated. The pass through parameter are as following: prefab instance, referenced array element,element's array index</param>
//        public static void InstantiatePrafabsWithArray<T>(GameObject prefab, Transform root, T[] array, Action<GameObject, T, int> action)
//        {
//            DeleteAllChildGameObject(root);
//            for (int _index = 0; _index < array.Length; _index++)
//            {
//                GameObject newObject = UnityEngine.Object.Instantiate(prefab);
//                SetParentAndNormalize(newObject.transform, root);
//                if (action != null)
//                    action(newObject, array[_index], _index);
//            }
//        }

//        /// <summary>
//        /// Delete child object and instantiate prefabs under the "root" GameObject
//        /// </summary>
//        /// <param name="prefab">The gameObject you want to instantiate</param>
//        /// <param name="root"></param>
//        /// <param name="length">Instantiate how many prefab?</param>
//        /// <param name="actionAfterEveryObjCreated">The statement you want to run after prefab is instantiated. The instance and instance index will be pass through</param>
//        public static void InstantiateGameObjectInRoot(GameObject prefab, Transform root, int length, Action<GameObject, int> actionAfterEveryObjCreated = null)
//        {
//            DeleteAllChildGameObject(root);
//            for (int i = 0; i < length; i++)
//            {
//                GameObject newObject = UnityEngine.Object.Instantiate(prefab);
//                if (actionAfterEveryObjCreated != null)
//                    actionAfterEveryObjCreated(newObject, i);
//                SetParentAndNormalize(newObject.transform, root);
//            }
//        }

//        /// <summary>
//        /// Delete all chield object under this transform
//        /// </summary>
//        /// <param name="_root">delete all GameObject under this transform</param>
//        public static void DeleteAllChildGameObject(Transform _root)
//        {
//            if (_root == null) { return; }
//            foreach (Transform child in _root)
//            {
//                GameObject.Destroy(child.gameObject);
//            }
//        }

//        /// <summary>
//        /// Set transform's parent and Normalize it
//        /// </summary>
//        public static void SetParentAndNormalize(Transform targetTransform, Transform parent)
//        {
//            targetTransform.SetParent(parent);
//            Utilities.NormalizeTransform(targetTransform.transform);
//        }

//        /// <summary>
//        /// Instantiate prefab and "set parent" + normalize it
//        /// </summary>
//        /// <param name="prefab"></param>
//        /// <param name="parent"></param>
//        /// <returns></returns>
//        public static GameObject InstantiateAndNormalize(GameObject prefab,Transform parent, bool setActiveAfterInstantiated = false)
//        {
//            GameObject _newInstance = UnityEngine.Object.Instantiate(prefab);
//            SetParentAndNormalize(_newInstance.transform, parent);
//            if (setActiveAfterInstantiated)
//                _newInstance.SetActive(true);

//            return _newInstance;
//        }

//        /// <summary>
//        /// Set local position and rotation(eulerangles) to Vector3(0,0,0) and local scale to Vector3(1,1,1)
//        /// </summary>
//        public static void NormalizeTransform(Transform _transform)
//        {
//            _transform.localRotation = Quaternion.identity;
//            _transform.localPosition = Vector3.zero;
//            _transform.localScale = new Vector3(1, 1, 1);
//        }

//        /// <summary>
//        /// Clamp value between max and min range
//        /// </summary>
//        /// <typeparam name="T">T need to be an IComparable</typeparam>
//        /// <param name="value"></param>
//        /// <param name="max">max value</param>
//        /// <param name="min">min value</param>
//        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
//        {
//            if (value.CompareTo(min) < 0)
//            {
//                return min;
//            }
//            else if (value.CompareTo(max) > 0)
//            {
//                return max;
//            }
//            else
//            {
//                return value;
//            }
//        }

//        public static void ReplaceSkinMesh(SkinnedMeshRenderer _renderer, Mesh _newSharedMesh, Material[] _newMatrials, Bounds _bound)
//        {
//            if (_newMatrials != null)
//            {
//                _renderer.sharedMaterials = _newMatrials;
//            }

//            if (_newSharedMesh != null)
//            {
//                _renderer.sharedMesh = _newSharedMesh;
//            }

//            if (_bound != new Bounds())
//            {
//                Debug.Log("Replacing Bounds. It's not equal to new Bounds()");
//                _renderer.localBounds = _bound;
//            }
//        }

//        /// <summary>
//        /// Utilitiey replace first person
//        /// </summary>
//        /// <param name="ArmRenderer"></param>
//        /// <param name="_info"></param>
//        public static void ChangeArm(SkinnedMeshRenderer ArmRenderer, Loadout_Character _info)
//        {
//            try
//            {
//                string _characterID = _info.CharacterId;
//                CharacterSetup _characterInfo = LWContentLists.Characters[_characterID];
//                if (_characterInfo == null) { throw new Exception("_characterInfo not found. - " + _characterID); }
//                CustomSkinnedMesh customSkinnedMesh = null;
//                if (_info.SkinId == null)
//                {
//                    customSkinnedMesh = _characterInfo.FirstPersonArm;
//                }
//                else
//                {
//                    string _skinId = _info.SkinId;
//                    customSkinnedMesh = _characterInfo.SkinsDict[_skinId].FirstPersonArm;
//                }

//                Utilities.ReplaceSkinMesh(ArmRenderer, customSkinnedMesh);
//            }
//            catch (Exception exp)
//            {
//                Debug.Log("ChangeArm: " + exp);
//            }
//        }

//        /// <summary>
//        /// Convert object to "string" (ex: int(30) to "30")
//        /// </summary>
//        /// <param name="value">object to convert</param>
//        /// <returns></returns>
//        public static string ObjectToString(object value)
//        {
//            string _vakueStr = "?";

//            Debug.Log(value.GetType());


//            if (value.GetType() == typeof(string))
//                _vakueStr = (string)value;
//            else if (value.GetType() == typeof(int))
//                _vakueStr = "" + (int)value;
//            else if (value.GetType() == typeof(float))
//                _vakueStr = "" + (float)value;
//            else if (value.GetType() == typeof(double))
//                _vakueStr = "" + (double)value;
//            else if (value.GetType() == typeof(bool))
//                _vakueStr = "" + (bool)value;

//            return _vakueStr;
//        }

//        /// <summary>
//        /// Apply store transform to transform pass through
//        /// </summary>
//        /// <param name="transform"></param>
//        /// <param name="data"><see cref="StoreTransform"/></param>
//        static public void ApplyStoreTransform(Transform transform, StoreTransform data)
//        {
//            transform.localPosition = data.LocalPosition;
//            transform.localEulerAngles = data.LocalRotation;
//            transform.localScale = data.LocalScale;
//        }

//        /// <summary>
//        /// Transitions between user interface.
//        /// </summary>
//        /// <param name="inTarget">In target.</param>
//        /// <param name="outTarget">Out target.</param>
//        public static void UITransition(GameObject inTarget, GameObject outTarget = null)
//        {
//            inTarget.SetActive(true);
//            if (outTarget)
//                TransitionHelper.TransitionOut(outTarget, () => { outTarget.SetActive(false); });
//            TransitionHelper.TransitionIn(inTarget);
//        }

//        /// <summary>
//        /// Transitions between user interface, will close the out target immediately after invoked
//        /// </summary>
//        /// <param name="inTarget">In Target.</param>
//        /// <param name="outTarget">Out target.</param>
//        public static void UITransition_CloseImmediately(GameObject inTarget, GameObject outTarget = null)
//        {
//            inTarget.SetActive(true);
//            TransitionHelper.TransitionIn(inTarget);
//            if (outTarget != null)
//                outTarget.SetActive(false);
//        }

//        /// <summary>
//        /// Transitions between user interface.
//        /// Will have callback invoked when all transition proccess is done
//        /// </summary>
//        /// <param name="inTarget">In target.</param>
//        /// <param name="outTarget">Out target.</param>
//        /// <param name="callBack">Callback after all transition proccess done</param>
//        public static void UITransition(GameObject inTarget, Action callBack, GameObject outTarget = null)
//        {
//            int completedOp = 0, targetOp = 1;
//            if (outTarget)
//            {
//                targetOp++;
//                TransitionHelper.TransitionOut(
//                    outTarget,
//                   () =>
//                   {
//                       completedOp++;
//                       outTarget.SetActive(false);
//                       if (completedOp == targetOp)
//                           callBack();
//                   }
//                );
//            }

//            inTarget.SetActive(true);
//            TransitionHelper.TransitionIn(inTarget, () =>
//            {
//                completedOp++;
//                if (completedOp == targetOp)
//                    callBack();
//            });
//        }

//        /// <summary>
//        /// Transitions out the user interface.
//        /// </summary>
//        /// <param name="outTarget">Out target.</param>
//        /// <param name="onComplete">On complete.</param>
//        public static void UITransitionOut(GameObject outTarget, Action onComplete = null)
//        {
//            if (onComplete == null)
//                TransitionHelper.TransitionOut(outTarget, () => { outTarget.SetActive(false); });
//            else
//                TransitionHelper.TransitionOut(outTarget, () => { outTarget.SetActive(false); onComplete(); });
//        }

//        /// <summary>
//        /// Check whether this index is in the range of Array/List
//        /// </summary>
//        /// <param name="index">The index to be checked</param>
//        /// <param name="array">The array to be checked</param>
//        /// <returns></returns>
//        public static bool IsValid(int index, ICollection array)
//        {
//            if (array == null)
//                return false;
//            if (index >= array.Count)
//                return false;
//            if (index < 0)
//                return false;

//            return true;
//        }

//        /// <summary>
//        /// Instantiate game object with resource path
//        /// </summary>
//        /// <param name="_path">path under resource folder</param>
//        /// <param name="_pos">Initialize position</param>
//        /// <param name="_rot">Initialize rotation</param>
//        /// <returns></returns>
//        public static GameObject InstantiateWithpath(string _path, Vector3 _pos, Quaternion _rot)
//        {
//            UnityEngine.Object _loadedObject = Resources.Load(_path);
//            GameObject _SpawnedObj = UnityEngine.Object.Instantiate(_loadedObject) as GameObject;
//            _SpawnedObj.transform.position = _pos;
//            _SpawnedObj.transform.rotation = _rot;
//            return _SpawnedObj;
//        }

//        /// <summary>
//        /// Clip the array index so that the it is guarantee to be in the range of array.
//        /// Pass the index as a reference so it will be automatically modified
//        /// </summary>
//        /// <param name="indexToClip"></param>
//        /// <param name="ArrayLegnth"></param>
//        public static void ClipArrayIndex(ref int indexToClip, int ArrayLegnth)
//        {
//            if (indexToClip < 0)
//                indexToClip = 0;
//            if (indexToClip >= ArrayLegnth)
//                indexToClip = ArrayLegnth - 1;
//        }

//        /// <summary>
//        /// Clip the array index so that the it is guarantee to be in the range of array.
//        /// The result(clipped) index will be return
//        /// </summary>
//        /// <param name="indexToClip"></param>
//        /// <param name="ArrayLegnth"></param>
//        public static int ClipArrayIndex(int indexToClip, int ArrayLegnth)
//        {
//            if (indexToClip < 0)
//                indexToClip = 0;
//            if (indexToClip >= ArrayLegnth)
//                indexToClip = ArrayLegnth - 1;
//            return indexToClip;
//        }

//        /// <summary>
//        /// Check whether the index is valid for accessing array
//        /// </summary>
//        /// <param name="arrayIndex"></param>
//        /// <param name="ArrayLegnth"></param>
//        /// <returns></returns>
//        public static bool CheckArrayIndex(int arrayIndex, int ArrayLegnth)
//        {
//            if (arrayIndex >= 0 && arrayIndex < ArrayLegnth)
//                return true;
//            else
//                return false;
//        }

//        /// <summary>
//        /// Check whether the index is valid for accessing array
//        /// </summary>
//        /// <param name="arrayIndex"></param>
//        /// <param name="array"></param>
//        /// <returns></returns>
//        public static bool CheckArrayIndex(int arrayIndex, Array array)
//        {
//            return CheckArrayIndex(arrayIndex, array.Length);
//        }

//        public static string GetColoredString(string content,Color color,FontStyle fontStyle = FontStyle.Normal)
//        {
//            string _friendlyColorCode = ColorUtility.ToHtmlStringRGB(color);
//            string _returnStr = "<color=#" + _friendlyColorCode + ">" + content + "</color>";
//            if(fontStyle == FontStyle.Bold)
//            {
//                _returnStr = "<b>" + _returnStr + "</b>";
//            }
//            return _returnStr;
//        }
//    }

//    public class GenericSerialization
//    {

//        // Convert an object to a byte array
//        static public byte[] ObjectToByteArray(System.Object obj)
//        {
//            //Need support null expression
//            if (obj == null)
//                return new byte[0];//Return a byte[] with length 0 for null

//            BinaryFormatter bf = new BinaryFormatter();
//            MemoryStream ms = new MemoryStream();
//            bf.Serialize(ms, obj);

//            return ms.ToArray();
//        }

//        // Convert a byte array to an Object
//        static public System.Object ByteArrayToObject(byte[] arrBytes)
//        {
//            if (arrBytes == null)
//                return null;
//            if (arrBytes.Length == 0)
//                return null;

//            MemoryStream memStream = new MemoryStream();
//            BinaryFormatter binForm = new BinaryFormatter();
//            memStream.Write(arrBytes, 0, arrBytes.Length);
//            memStream.Seek(0, SeekOrigin.Begin);
//            var obj = binForm.Deserialize(memStream);

//            return obj;
//        }

//        /// <summary>
//        /// Get the actual byte segment of byte array and convert the byte array to an Object
//        /// </summary>
//        /// <param name="ByteArrayFromFastRPC"></param>
//        /// <returns></returns>
//        static public System.Object FastRPCByteArrayToObject(byte[] ByteArrayFromFastRPC)
//        {
//            int _ActualLegnth = ByteArrayFromFastRPC.Length - LWNet.LWNetwork.FastRPCDedodeIndex;

//            byte[] actualByte = new byte[_ActualLegnth];
//            Array.Copy(ByteArrayFromFastRPC, 6, actualByte, 0, _ActualLegnth);

//            return ByteArrayToObject(actualByte);
//        }


//    }

//    /// <summary>
//    /// Since unity doesn't flag the Vector3 as serializable, we
//    /// need to create our own version. This one will automatically convert
//    /// between Vector3 and SerializableVector3
//    /// </summary>
//    [System.Serializable]
//    public struct SerializableVector3
//    {
//        /// <summary>
//        /// x component
//        /// </summary>
//        public float x;

//        /// <summary>
//        /// y component
//        /// </summary>
//        public float y;

//        /// <summary>
//        /// z component
//        /// </summary>
//        public float z;

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="rX"></param>
//        /// <param name="rY"></param>
//        /// <param name="rZ"></param>
//        public SerializableVector3(float rX, float rY, float rZ)
//        {
//            x = rX;
//            y = rY;
//            z = rZ;
//        }

//        /// <summary>
//        /// Returns a string representation of the object
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return String.Format("[{0}, {1}, {2}]", x, y, z);
//        }

//        /// <summary>
//        /// Automatic conversion from SerializableVector3 to Vector3
//        /// </summary>
//        /// <param name="rValue"></param>
//        /// <returns></returns>
//        public static implicit operator Vector3(SerializableVector3 rValue)
//        {
//            return new Vector3(rValue.x, rValue.y, rValue.z);
//        }

//        /// <summary>
//        /// Automatic conversion from Vector3 to SerializableVector3
//        /// </summary>
//        /// <param name="rValue"></param>
//        /// <returns></returns>
//        public static implicit operator SerializableVector3(Vector3 rValue)
//        {
//            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
//        }
//    }

//    /// <summary>
//    /// Since unity doesn't flag the Quaternion as serializable, we
//    /// need to create our own version. This one will automatically convert
//    /// between Quaternion and SerializableQuaternion
//    /// </summary>
//    [System.Serializable]
//    public struct SerializableQuaternion
//    {
//        /// <summary>
//        /// x component
//        /// </summary>
//        public float x;

//        /// <summary>
//        /// y component
//        /// </summary>
//        public float y;

//        /// <summary>
//        /// z component
//        /// </summary>
//        public float z;

//        /// <summary>
//        /// w component
//        /// </summary>
//        public float w;

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="rX"></param>
//        /// <param name="rY"></param>
//        /// <param name="rZ"></param>
//        /// <param name="rW"></param>
//        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
//        {
//            x = rX;
//            y = rY;
//            z = rZ;
//            w = rW;
//        }

//        /// <summary>
//        /// Returns a string representation of the object
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
//        }

//        /// <summary>
//        /// Automatic conversion from SerializableQuaternion to Quaternion
//        /// </summary>
//        /// <param name="rValue"></param>
//        /// <returns></returns>
//        public static implicit operator Quaternion(SerializableQuaternion rValue)
//        {
//            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
//        }

//        /// <summary>
//        /// Automatic conversion from Quaternion to SerializableQuaternion
//        /// </summary>
//        /// <param name="rValue"></param>
//        /// <returns></returns>
//        public static implicit operator SerializableQuaternion(Quaternion rValue)
//        {
//            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
//        }
//    }

//    [System.Serializable]
//    public class StoreTransform
//    {
//        public Vector3 LocalPosition;
//        /// <summary>
//        /// Euler angles
//        /// </summary>
//        public Vector3 LocalRotation;
//        public Vector3 LocalScale = Vector3.one;
//    }

//    /// <summary>
//    /// Resources for creating a grid panel,ex: root transform, prefab to be instantiated
//    /// So that user can drag and drop resources to it from the inspector
//    /// </summary>
//    [System.Serializable]
//    public class GridPanelResources
//    {
//        public Transform root;
//        public GameObject gridPrefab;
//    }

//    namespace CommonDelegate
//    {
//        /// <summary>
//        /// Delegate which don't have parameter
//        /// </summary>
//        public delegate void LWEvent_NoParam();
//        /// <summary>
//        /// Delegate with one int as parameter
//        /// </summary>
//        /// <param name="p0"></param>
//        public delegate void LWEvent_Int(int p0);

//        public delegate void LWEvent_Int_Int(int p0, int p1);
//    }

//    public static class StackExtension
//    {
//        /// <summary>
//        /// An extended method for peek in stack, 
//        /// will return null if there's nothing can be peeked
//        /// </summary>
//        /// <returns>The top item in this stack, will be null if the stack is empty</returns>
//        /// <param name="stack">Stack.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public static T NullablePeek<T>(this Stack<T> stack) where T : class
//        {
//            T _currentTop = null;

//            try
//            {
//                _currentTop = stack.Peek();
//            }
//            catch (InvalidOperationException exp)
//            {
//                _currentTop = null;
//            }

//            return _currentTop;
//        }

//        /// <summary>
//        /// An extended method for Pop in stack, 
//        /// will return null if there's nothing can be popped
//        /// </summary>
//        /// <returns>The top item in this stack, will be null if the stack is empty</returns>
//        /// <param name="stack">Stack.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public static T NullablePop<T>(this Stack<T> stack) where T : class
//        {
//            T _currentTop = null;
//            try
//            {
//                _currentTop = stack.Pop();
//            }
//            catch (InvalidOperationException exp)
//            {
//                _currentTop = null;
//            }

//            return _currentTop;
//        }
//    }
//}