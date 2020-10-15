using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;



public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    [SyncVar] public string matchID;
    [SyncVar] public int playerIndex;
    [SyncVar] public bool isReady;

    private NetworkMatchChecker networkMatchChecker;
    private GameObject playerLobbyUI;

    private void Awake()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            playerLobbyUI =  UILobby.instance.SpawnPlayerUIPrefab(this);
        }
    }

    public override void OnStopClient()
    {
        Debug.Log($"Client stopped");
        ClientDisconnect();
    }

    public override void OnStopServer()
    {
        Debug.Log($"Client stopped on server");
        ServerDisconnect();
    }

    #region HOST Match

    public void HostGame(bool isPublicMatch)
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdHostGame(matchID, isPublicMatch);
    }

    [Command]
    void CmdHostGame(string _matchID, bool isPublicMatch)
    {
        matchID = _matchID;
        if (MatchMaker.instance.HostGame(_matchID, gameObject, isPublicMatch, out playerIndex))
        {
            Debug.Log($"<color=green>Game hosted Successfully</color>");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true, _matchID, playerIndex);
        } else
        {
            Debug.Log($"<color=red>Game hosted failed</color>");
            TargetHostGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc] 
    private void TargetHostGame(bool success, string _matchID, int _playerIndex)
    {
        this.playerIndex = _playerIndex;
        this.matchID = _matchID;
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess(success, _matchID);
    }

    #endregion

    #region JOIN Match
    public void JoinGame(string _inputID)
    {
        CmdJoinGame(_inputID);
    }

    [Command]
    void CmdJoinGame(string _matchID)
    {
        matchID = _matchID;
        if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"<color=green>Game joined Successfully</color>");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game joined failed</color>");
            TargetJoinGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    private void TargetJoinGame(bool success, string _matchID, int indexPlayer)
    {
        this.playerIndex = indexPlayer;
        this.matchID = _matchID;
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success, _matchID);
    }

    #endregion

    #region Search Match
    public void SearchGame()
    {
        CmdSearchGame();
    }

    [Command]
    private void CmdSearchGame()
    {
        if (MatchMaker.instance.SearchGame(gameObject, out playerIndex, out matchID))
        {
            Debug.Log($"<color=green>Game Searched Successfully</color>");
            networkMatchChecker.matchId = matchID.ToGuid();
            TargetSearchGame(true, matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game Searched failed</color>");
            TargetSearchGame(false, matchID, playerIndex);
        }
    }

    [TargetRpc]
    private void TargetSearchGame(bool success, string _matchID, int indexPlayer)
    {
        this.playerIndex = indexPlayer;
        this.matchID = _matchID;
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.SearchSuccess(success, _matchID);
    }
    #endregion

    #region Begin Match
    public void BeginGame()
    {
        CmdBeginGame();
    }

    [Command]
    private void CmdBeginGame()
    {
        MatchMaker.instance.BeginGame(matchID);
        Debug.Log($"<color=red>Game Begined failed</color>");     
    }

    [TargetRpc]
    private void TargetBeginGame()
    {
        Debug.Log($"MatchID: {matchID} | Begginning");
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }

    public void StartGame()
    {
        TargetBeginGame();
    }

    #endregion

    #region Disconnect Match

    public void DisconnectGame()
    {
        CmdDisconnectGame();
    }

    [Command]
    private void CmdDisconnectGame()
    {
        ServerDisconnect();
    }

    private void ServerDisconnect()
    {
        MatchMaker.instance.DisconnectPlayer(this, matchID);
        networkMatchChecker.matchId = string.Empty.ToGuid();
        RpcDisconnectGame();
    }

    [ClientRpc]
    private void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    private void ClientDisconnect()
    {
        if (playerLobbyUI != null)
        {
            Destroy(playerLobbyUI);
        }
    }

    #endregion
}
