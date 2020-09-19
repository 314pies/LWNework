using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LWNet;

public class TestCodeForLWLAN : MonoBehaviour
{
    public Text PlayerNameList;

    // Use this for initialization
    void Start()
    {

    }

    public void Disconnect()
    {
        LWLanManager.LeaveRoom();
    }

    public void StartHost()
    {
        LWLanManager.LWStartHost("Demo 1 Room",
            new LobbyPlayer("Server User Name"),
            () => { Debug.Log("Host Started"); }
            );
    }


    public void Connect()
    {
        LWLanManager.ConnectToRoom("127.0.0.1",
            new LobbyPlayer("Client User Name"),
            () =>
            {
                Debug.Log("Connected");
            },
            () =>
            {
                Debug.Log("Disconnected");
            }
        );
    }

    public int PlayerId = 0;
    //public string key = "77888";
    //public string value = "FAFAFF";

    // Update is called once per frame
    void Update()
    {
        PlayerId = LWLanManager.LocalPlayerId;
        string _playerInfosStr = "";
        foreach (LobbyPlayer _playerInfo in LWLanManager.LobbyPlayers) { _playerInfosStr += _playerInfo.Name + "\n"; }
        PlayerNameList.text = _playerInfosStr;
    }
}
