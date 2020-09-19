using UnityEngine;
using System.Collections;
using LWNet;
using UnityEngine.SceneManagement;
namespace LWNet
{


    /// <summary>
    /// LWNet initialization, should be placed in "Game scene"
    /// </summary>
    public class LWNetInitiazlization : MonoBehaviour
    {
        /// <summary>
        /// Will set <see cref="LWNetwork.networkMode"/> to OfflineMode
        /// </summary>
        //public bool SetToOfflineMode = false;

        //public BOTManager botManager;

        [Tooltip("Automatically start as host.")]
        [SerializeField]
        private bool autoHost = false;

        static public LWNetInitiazlization Singleton { get; private set; }

        public LWView[] StaticSceneObjects
        {
            get
            {
                return staticSceneObjects;
            }
        }
        /// <summary>
        /// The ID of all static network scene object
        /// </summary>
        [ReadOnly]
        [SerializeField]
        private LWView[] staticSceneObjects;



        private void Awake()
        {
            Singleton = this;
            if (autoHost == true)
            {
                if(LWLanManager.LWSingleton == null)
                {
                    Debug.Log("Instantiating LWLanManager.");
                    Instantiate(LWNetSetup.Singleton.LWLanManager);
                }
                if (LWLanManager.LWSingleton != null)
                {
                    LWLanManager.LWSingleton.StartHost();
                    LWLanManager.LWSingleton.ReceiveNetworkIDEventHandler += Initialize;
                }
                else
                    Debug.Log("LWLanManager not found. ");
            }
            else
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize LWnetwork
        /// </summary>
        void Initialize()
        {
            //SceneManager.sceneLoaded += OnSceneLoaded;
            //Debug.Log("OnSceneLoaded: " + scene.name + ". Initializing LW Network");      

            bool _isSucceed = false;

            _isSucceed = LWNetwork.Core.Initialize(staticSceneObjects);
        }

        void OnDestroy()
        {
            if (autoHost == true)
            {
                if (LWLanManager.LWSingleton != null)
                {
                    LWLanManager.LWSingleton.StopHost();
                    LWLanManager.LWSingleton.ReceiveNetworkIDEventHandler -= Initialize;
                }
            }

            //SceneManager.sceneLoaded -= OnSceneLoaded;
            LWNetwork.Core.DeInitialize();
            Debug.Log("LWNetInitiazlization Destroyed");
        }

#if UNITY_EDITOR
        public void UpdateStaticSceneObjects(LWView[] _staticSceneObjList)
        {
            UnityEditor.Undo.RecordObject(this, "Updating static scene object list");
            staticSceneObjects = _staticSceneObjList;
        }
#endif
    }
}