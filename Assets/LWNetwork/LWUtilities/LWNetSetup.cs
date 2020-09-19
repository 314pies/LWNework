using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LWNet
{
    /// <summary>
    /// Some serialized configuration for LWNet
    /// </summary>
    public class LWNetSetup : ScriptableObject
    {
        public const string FileName = "LWNetSetup";
        const string SavingPath = "Assets/LocalWarfare/Resources/";

        [SerializeField]
        public GameObject LWLanManager;
        public static LWNetSetup Singleton
        {
            get
            {
                return Resources.Load<LWNetSetup>(FileName);
            }
        }

//#if UNITY_EDITOR
//        [MenuItem("LWTools/LWNet Setup")]
//        public static void OpenLWNetSetup()
//        {
//            LWNetSetup _EditedData = Resources.Load<LWNetSetup>(FileName);
//            if (_EditedData != null)
//            {
//                Debug.Log("Opening Weapon System Setting");
//                EditorGUIUtility.PingObject(_EditedData);
//                Selection.objects = new UnityEngine.Object[] { _EditedData };
//            }
//            else
//            {
//                var asset = CreateInstance<LWNetSetup>();
//                AssetDatabase.CreateAsset(asset, SavingPath + FileName + ".asset");
//                AssetDatabase.SaveAssets();
//                AssetDatabase.Refresh();
//                _EditedData = Resources.Load<LWNetSetup>(FileName);
//                Selection.objects = new UnityEngine.Object[] { _EditedData };
//            }
//        }
//#endif
    }
}