using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button readyButtonText;

    private Player _player;

    public void SetPlayer(Player player)
    {
        this._player = player;
        text.text = "Player " + player.playerIndex.ToString();
    }

    public void ChangeReadyButton()
    {
            if (_player.isReady)
            {
                readyButtonText.GetComponentInChildren<TMP_Text>().text = "Not Ready";
                readyButtonText.GetComponent<Image>().color = Color.red;
            }
            else
            {
                readyButtonText.GetComponentInChildren<TMP_Text>().text = "Ready";
                readyButtonText.GetComponent<Image>().color = Color.green;
            }
    }

    public void OnReadyButton()
    {
        if (_player == Player.localPlayer)
        {
            MatchMaker.instance.ChangeReadyPlayer(_player, !_player.isReady);
        }
    }
}
