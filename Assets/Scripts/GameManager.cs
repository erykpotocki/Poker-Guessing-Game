using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private float waitForRoomSeconds = 8f;

    private IEnumerator Start()
    {
        float timer = 0f;

        while (!PhotonNetwork.InRoom && timer < waitForRoomSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[Game] Not in room!");
            yield break;
        }

        Debug.Log("[Game] === GAME START ===");
        Debug.Log($"[Game] Players count: {PhotonNetwork.PlayerList.Length}");

        foreach (Player p in PhotonNetwork.PlayerList)
            Debug.Log("[Game] Player: " + p.NickName);
    }
}