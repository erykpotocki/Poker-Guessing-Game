using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class FakePlayers : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI playersListText;   // PlayersListText
    [SerializeField] private TextMeshProUGUI playersCountText;  // PlayerCountText (Gracze: x/6)

    [Header("Debug (Editor/Host only)")]
    [SerializeField] private int maxPlayers = 6;

    private readonly List<string> bots = new List<string>();

    private void Update()
    {
        if (!PhotonNetwork.InRoom) return;

#if UNITY_EDITOR
        if (!PhotonNetwork.IsMasterClient) return;

        if (Input.GetKeyDown(KeyCode.F1)) AddBot();
        if (Input.GetKeyDown(KeyCode.F2)) RemoveBot();

        RefreshUI();
#endif
    }

    private void AddBot()
    {
        int real = PhotonNetwork.CurrentRoom.PlayerCount;
        int freeSlotsForBots = Mathf.Max(0, maxPlayers - real);
        if (bots.Count >= freeSlotsForBots) return;

        bots.Add("Bot_" + Random.Range(100, 999));
    }

    private void RemoveBot()
    {
        if (bots.Count == 0) return;
        bots.RemoveAt(bots.Count - 1);
    }

    private void RefreshUI()
    {
        // COUNT
        if (playersCountText != null)
        {
            int real = PhotonNetwork.CurrentRoom.PlayerCount;
            int shown = Mathf.Clamp(real + bots.Count, 0, maxPlayers);
            playersCountText.text = $"Gracze: {shown}/{maxPlayers}";
        }

        // LISTA
        if (playersListText != null)
        {
            var sb = new StringBuilder();
            int i = 1;

            foreach (var p in PhotonNetwork.PlayerList)
                sb.AppendLine($"{i++}. {p.NickName}");

            foreach (var b in bots)
                sb.AppendLine($"{i++}. {b}");

            if (i == 1) sb.AppendLine("(brak graczy)");

            playersListText.text = sb.ToString();
        }
    }
}