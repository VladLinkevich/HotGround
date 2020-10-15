using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{

    public static UILobby instance;

    [Header("Host Join")]
    [SerializeField] private TMP_InputField joinMatchInput;
    [SerializeField] private List<Selectable> lobbySelectables = new List<Selectable>();
    [SerializeField] private GameObject lobbyGameObject;
    [SerializeField] private GameObject searchGameObject;

    [Header("Lobby")]
    [SerializeField] private Transform UIPlayerParent;
    [SerializeField] private GameObject UIPlayerPrefab;
    [SerializeField] private TMP_Text textMatchID;
    [SerializeField] private GameObject beginGameButton;

    private List<UIPlayer> UIPlayers = new List<UIPlayer>();
    private GameObject playerLobbyUI;

    private bool _search = false;

    private void Start()
    {
        instance = this;
    }

    public void OnHostPrivateButton()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = true);

        Player.localPlayer.HostGame(false);
    } 
    
    public void OnHostPublicButton()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = true);

        Player.localPlayer.HostGame(true);
    }
     
    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyGameObject.SetActive(true);
            if (playerLobbyUI != null) { Destroy(playerLobbyUI); }
            playerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
            textMatchID.text = matchID;
            beginGameButton.SetActive(true);
        }
        else
        {
            joinMatchInput.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void OnJoinButton()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player.localPlayer.JoinGame(joinMatchInput.text.ToUpper() );    
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success) 
        {
            lobbyGameObject.SetActive(true);
            if (playerLobbyUI != null) { Destroy(playerLobbyUI); }
            playerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
            textMatchID.text = matchID;
        }
        else
        {
            joinMatchInput.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void SearchSuccess(bool success, string matchID)
    {
        if (success)
        {
            searchGameObject.SetActive(false);
            JoinSuccess(success, matchID);
            _search = false;
        }
    }

    public GameObject SpawnPlayerUIPrefab(Player player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        UIPlayers.Add(newUIPlayer.GetComponent<UIPlayer>());
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        return newUIPlayer;
    }

    public void ChangeReadyButton()
    {
        foreach(UIPlayer player in UIPlayers)
        {
            player.ChangeReadyButton();
        }
    }


    public void BeginGame()
    {
        Player.localPlayer.BeginGame();
    }

    public void SearchGame()
    {
        Debug.Log($"Searching for game");
        searchGameObject.SetActive(true);
        StartCoroutine(SearchingForGame());
    }

    public void OnCancelSearchGame()
    {
        searchGameObject.SetActive(false);
        _search = false;
        lobbySelectables.ForEach(x => x.interactable = true);
    }

    public void OnDisconnectGameButton()
    {
        if (playerLobbyUI != null) { Destroy(playerLobbyUI); }
        Player.localPlayer.DisconnectGame();

        lobbyGameObject.SetActive(false);
        lobbySelectables.ForEach(x => x.interactable = true);
    }

    IEnumerator SearchingForGame()
    {
        _search = true;
        WaitForSeconds checkEveryFewSeconds = new WaitForSeconds(1);
        while (_search)
        {
            Player.localPlayer.SearchGame();
            yield return checkEveryFewSeconds;
        }
    }
}
