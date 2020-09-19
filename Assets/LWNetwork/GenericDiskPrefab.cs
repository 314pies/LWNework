///This is some uncompleted codes, will finish it in the future if it's necessary

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System;
using LWUtilities;


/// <summary>
/// Prefab that will stored on disk and wont automatically loaded to memory.
/// Assign object under resource folder to it and manipulate it as a normal prefab object.
/// </summary>
/// <typeparam name="T">What kind of prefab</typeparam>
//[System.Serializable]
public class GenericDiskPrefab<T> where T : UnityEngine.Object
{
    /// <summary>
    /// Where the Target prefab is stored, will be automatically generated when assign obj in inspector
    /// </summary>
    [SerializeField]
    private string Path = null;
    /// <summary>
    /// Get the path of this prefab under resource folder
    /// </summary>
    public string GetPath { get { return Path; } }
    /// <summary>
    /// The instance of this prefab
    /// </summary>
    public T instance
    {
        get
        {
            return Resources.Load(Path,typeof(T)) as T;
        }
    }
}
#if UNITY_EDITOR
public class GenericDiskPrefabDrawer<T> : PropertyDrawer
{
    public UnityEngine.Object obj = null;
    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    return base.GetPropertyHeight(property, label) + 3f;
    //}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        //position.height = 18f;

        //Load obj instance to inspector
        SerializedProperty _PathStr = property.FindPropertyRelative("Path");
        if (_PathStr.stringValue != null)
        {
            obj = Resources.Load(_PathStr.stringValue);
        }

        EditorGUI.BeginChangeCheck();
        obj = EditorGUI.ObjectField(position, property.name, obj, typeof(T), false);
        // Update instance path when its assigned
        if (EditorGUI.EndChangeCheck())
        {
            if (obj != null) {
                string _newPath = AssetDatabase.GetAssetPath(obj);
                _newPath = Utilities.GetAssetPath_ClipString(_newPath);
                if (_newPath == null)
                {
                    Debug.Log("Failed to locate prefab path, did you place it under any resource folder?");
                }
                else
                {
                    _PathStr.stringValue = _newPath;
                    Debug.Log("Path string updated:" + _PathStr.stringValue);
                }
            }else
            {
                _PathStr.stringValue = null;
            }
           
        }
    }
}
#endif

/*
    code example
    public GameObject_ODP a;
*/

/// <summary>
/// GameObject - On Disk Prefab
/// </summary>
[System.Serializable]
public class GameObject_ODP : GenericDiskPrefab<GameObject> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GameObject_ODP))]
public class GameObject_ODP_Drawer : GenericDiskPrefabDrawer<GameObject> { }
#endif

/// <summary>
/// Font - On Disk Prefab
/// </summary>
[System.Serializable]
public class Font_ODP : GenericDiskPrefab<Font> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Font_ODP))]
public class Font_ODP_Drawer : GenericDiskPrefabDrawer<Font> { }
#endif

/// <summary>
/// Texture - On Disk Prefab
/// </summary>
[System.Serializable]
public class Texture_ODP : GenericDiskPrefab<Texture> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Texture_ODP))]
public class Texture_ODP_Drawer : GenericDiskPrefabDrawer<Texture> { }
#endif

/// <summary>
/// AnimationClip - On Disk Prefab
/// </summary>
[System.Serializable]
public class AnimationClip_ODP : GenericDiskPrefab<AnimationClip> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AnimationClip_ODP))]
public class AnimationClip_ODP_Drawer : GenericDiskPrefabDrawer<AnimationClip> { }
#endif

/// <summary>
/// Material - On Disk Prefab
/// </summary>
[System.Serializable]
public class Material_ODP : GenericDiskPrefab<Material> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Material_ODP))]
public class Material_ODP_Drawer : GenericDiskPrefabDrawer<Material> { }
#endif


/// <summary>
/// Mesh - On Disk Prefab
/// </summary>
[System.Serializable]
public class Mesh_ODP : GenericDiskPrefab<Mesh> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Mesh_ODP))]
public class Mesh_ODP_Drawer : GenericDiskPrefabDrawer<Mesh> { }
#endif

/// <summary>
/// Audio - On Disk Prefab
/// </summary>
[System.Serializable]
public class AudioClip_ODP : GenericDiskPrefab<AudioClip> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AudioClip_ODP))]
public class AudioClip_ODP_Drawer : GenericDiskPrefabDrawer<AudioClip> { }
#endif

/// <summary>
/// Audio - On Disk Prefab
/// </summary>
[System.Serializable]
public class Sprite_ODP : GenericDiskPrefab<Sprite> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Sprite_ODP))]
public class Sprite_ODP_Drawer : GenericDiskPrefabDrawer<Sprite> { }
#endif