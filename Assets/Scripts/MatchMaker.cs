using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

[System.Serializable]
public class Match
{
    public string matchID;
    public bool isPublicMatch;
    public bool inMatch;
    public bool isMatchFull;
    public SyncListGameObject players = new SyncListGameObject();

    public Match(string matchID, GameObject player)
    {
        this.matchID = matchID;
        players.Add(player);
    }

    public Match()
    {

    }
}

[System.Serializable]
public class SyncListMatch : SyncList<Match> {}

[System.Serializable]
public class SyncListGameObject : SyncList<GameObject> {}

public class MatchMaker : NetworkBehaviour
{
    [SerializeField] private static int lenghtID = 6;
    [SerializeField] private GameObject turnMangerPrefab;

    public static MatchMaker instance;
    public SyncListMatch matches = new SyncListMatch();
    public SyncListString matchIDs = new SyncListString();

    private void Start()
    {
        instance = this;
    }

    public bool HostGame(string _matchID, GameObject _player, bool isPublicMatch, out int playerIndex)
    {
        playerIndex = -1;

        if (!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            Match match = new Match(_matchID, _player);
            match.isPublicMatch = isPublicMatch;
            matches.Add(match);

            Debug.Log($"Match generated");
            playerIndex = 1;
        }
        else 
        {
            Debug.Log($"Match ID already exists");
            return false;
        }
        return true;
    }
    
    public bool JoinGame(string _matchID, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;

        if (matchIDs.Contains(_matchID))
        {
            for (int i = 0; i < matches.Count; ++i)
            {
                if (matches[i].matchID == _matchID)
                {
                    matches[i].players.Add(_player);
                    playerIndex = matches[i].players.Count;
                    break;
                }
            }

            Debug.Log($"Match joined");
        }
        else 
        {
            Debug.Log($"Match ID does not exist");
            return false;
        }
        return true;
    }

    public bool SearchGame(GameObject _player, out int playerIndex, out string matchID)
    {
        playerIndex = -1;
        matchID = string.Empty;

        for (int i = 0; i < matches.Count; ++i)
        {
            if (matches[i].isPublicMatch && !matches[i].isMatchFull && !matches[i].inMatch)
            {
                matchID = matches[i].matchID;
                if (JoinGame(matchID, _player, out playerIndex))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void DisconnectPlayer(Player player, string _matchID)
    {
        for(int i = 0; i < matches.Count; ++i)
        {
            if (matches[i].matchID == _matchID)
            {
                int indexPlayer = matches[i].players.IndexOf(player.gameObject);
                matches[i].players.RemoveAt(indexPlayer);
                Debug.Log($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining.");

                if (matches[i].players.Count == 0)
                {
                    Debug.Log($"No more player in match. Terminating {_matchID}");
                    matchIDs.Remove(_matchID);
                    matches.RemoveAt(i);
                }

                break;
            }

            
        }
    }

    public void BeginGame(string _matchID)
    {
        GameObject newTurnManager = Instantiate(turnMangerPrefab);
        NetworkServer.Spawn(newTurnManager);
        newTurnManager.GetComponent<NetworkMatchChecker>().matchId = _matchID.ToGuid();
        TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();

        for (int i = 0; i < matches.Count; ++i)
        {
            if(matches[i].matchID == _matchID)
            {
                foreach (var player in matches[i].players)
                {
                    Player _player = player.GetComponent<Player>();
                    turnManager.AddPlayer(_player);
                    _player.StartGame();
                }
            }
        }
    }

    public void ChangeReadyPlayer(Player player, bool isReady)
    {
        player.GetComponent<Player>().isReady = isReady;
        for (int i = 0; i < matches.Count; ++i)
        {
            if (matches[i].matchID == player.matchID)
            {              
                 UILobby.instance.ChangeReadyButton();             
            }
        }
    }



    public static string GetRandomMatchID()
    {
        string _id = string.Empty;

        for (int i = 0; i < lenghtID; ++i)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if (random < 26)
            {
                _id += (char)(random + 65);
            }else
            {
                _id += (random - 26).ToString();
            }
        }

        Debug.Log($"Random Match ID: {_id}");
        return _id;
    }

}

public static class MatchExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new Guid(hashBytes);
    }
}