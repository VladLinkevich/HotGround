using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHostClient : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private void Start()
    {
        if (!Application.isBatchMode)
        {
            Debug.Log($"=== Client Conected ===");
            networkManager.StartClient(); 
        } else
        {
            Debug.Log($"=== Server Starting ===");
        }
    }

    public void JoinLocal()
    {
        networkManager.networkAddress = "localhost";
        networkManager.StartClient();
    }


}
