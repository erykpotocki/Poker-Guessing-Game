using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SeatDisconnectWatcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoundLogUI roundLogUI;

    private readonly HashSet<int> knownPlayerActorNumbers = new HashSet<int>();
    private readonly Dictionary<int, SeatUIView> seatViewsByActorNumber = new Dictionary<int, SeatUIView>();

    private IEnumerator Start()
    {
        yield return null;
        ResolveRefs();
        RefreshAllFromRoomState();
    }

    public override void OnJoinedRoom()
    {
        ResolveRefs();
        RefreshAllFromRoomState();
    }

    public override void OnLeftRoom()
    {
        knownPlayerActorNumbers.Clear();
        seatViewsByActorNumber.Clear();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == null)
            return;

        knownPlayerActorNumbers.Add(otherPlayer.ActorNumber);
        ApplyDisconnectedToActor(otherPlayer.ActorNumber, true);

        if (roundLogUI != null)
            roundLogUI.AddPlayerLeftGame(otherPlayer.NickName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer == null)
            return;

        knownPlayerActorNumbers.Add(newPlayer.ActorNumber);
        ApplyDisconnectedToActor(newPlayer.ActorNumber, false);

        if (roundLogUI != null)
            roundLogUI.AddPlayerRejoinedGame(newPlayer.NickName);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        RefreshAllFromRoomState();
    }

    public void RefreshAllFromRoomState()
    {
        ResolveRefs();
        CacheSeatViews();

        HashSet<int> activeActorNumbers = new HashSet<int>();

        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Players != null)
        {
            foreach (KeyValuePair<int, Player> pair in PhotonNetwork.CurrentRoom.Players)
            {
                Player player = pair.Value;
                if (player == null)
                    continue;

                knownPlayerActorNumbers.Add(player.ActorNumber);

                if (!player.IsInactive)
                    activeActorNumbers.Add(player.ActorNumber);
            }
        }

        foreach (KeyValuePair<int, SeatUIView> pair in seatViewsByActorNumber)
        {
            int actorNumber = pair.Key;
            SeatUIView seat = pair.Value;

            if (seat == null)
                continue;

            bool isKnownPlayer = knownPlayerActorNumbers.Contains(actorNumber);
            bool isDisconnected = isKnownPlayer && !activeActorNumbers.Contains(actorNumber);

            seat.SetDisconnectedVisual(isDisconnected);
        }
    }

    private void ApplyDisconnectedToActor(int actorNumber, bool disconnected)
    {
        CacheSeatViews();

        if (seatViewsByActorNumber.TryGetValue(actorNumber, out SeatUIView seat) && seat != null)
        {
            seat.SetDisconnectedVisual(disconnected);
        }
    }

    private void ResolveRefs()
    {
        if (roundLogUI == null)
            roundLogUI = FindFirstObjectByType<RoundLogUI>(FindObjectsInactive.Include);
    }

    private void CacheSeatViews()
    {
        seatViewsByActorNumber.Clear();

        SeatUIView[] allSeatViews = FindObjectsByType<SeatUIView>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < allSeatViews.Length; i++)
        {
            SeatUIView seatView = allSeatViews[i];
            if (seatView == null)
                continue;

            if (!TryGetActorNumberFromSeatName(seatView.name, out int actorNumber))
                continue;

            if (!seatViewsByActorNumber.ContainsKey(actorNumber))
                seatViewsByActorNumber.Add(actorNumber, seatView);
        }
    }

    private bool TryGetActorNumberFromSeatName(string seatName, out int actorNumber)
    {
        actorNumber = -1;

        if (string.IsNullOrWhiteSpace(seatName))
            return false;

        if (seatName.Contains("Dealer"))
            return false;

        string[] parts = seatName.Split('_');
        if (parts.Length < 3)
            return false;

        return int.TryParse(parts[2], out actorNumber);
    }
}