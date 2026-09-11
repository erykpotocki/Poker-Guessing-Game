using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public partial class TurnManager
{
    private const string ReviewRoundKey = "reviewRoundV1";
    private const string ReadyKey = "readyAfterRoundV1";
    private const string ContinueKey = "continueAfterRoundV1";
    private const string DealReadyKey = "dealReadyRoundV1";
    private const string DealOpenKey = "dealOpenRoundV1";
    private const string NextDealLoserKey = "nextDealLoserV1";
    private RoundReviewUI roundReview;
    private Coroutine dealBarrierRoutine;
    private bool initializationRunning;
    private bool applyingReviewResult;
    private bool localReadyRequested;
    private int reviewRound;
    private int nextDealLoser = -1;
    public bool IsAwaitingRoundReady { get; private set; }
    private string RoundToken => sharedGameSeed + ":" + currentRoundNumber;

    private void EnsureRoundReview()
    {
        if (roundReview != null) return;
        roundReview = gameObject.AddComponent<RoundReviewUI>();
        roundReview.Initialize(this);
    }

    public void MarkRoundReady()
    {
        if (!IsAwaitingRoundReady || PhotonNetwork.LocalPlayer == null || localReadyRequested) return;
        localReadyRequested = PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { ReadyKey, RoundToken } });
        roundReview.SetReviewVisible(true, localReadyRequested);
    }

    private bool PlayerHasToken(Player player, string key, string token)
    {
        return player != null && player.CustomProperties.TryGetValue(key, out object value) &&
            value is string stored && stored == token;
    }

    private bool RoomHasToken(string key, string token)
    {
        return PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object value) &&
            value is string stored && stored == token;
    }

    private IEnumerator WaitForRoundReady(List<int> participants)
    {
        IsAwaitingRoundReady = true;
        localReadyRequested = PlayerHasToken(PhotonNetwork.LocalPlayer, ReadyKey, RoundToken);
        RefreshActiveHighlight(-1);
        float started = Time.unscaledTime;
        bool requestedContinue = false;
        int requestedMaster = -1;
        string token = RoundToken;
        while (PhotonNetwork.InRoom && !RoomHasToken(ContinueKey, token))
        {
            bool botsReady = Time.unscaledTime - started >= 2f;
            bool everyoneReady = true;
            foreach (int actor in participants)
            {
                if(!activePlayerOrder.Contains(actor))
                {
                    if(seatViewsByActorNumber.TryGetValue(actor,out SeatUIView eliminatedSeat)&&eliminatedSeat!=null)eliminatedSeat.SetReadyIndicator(false);
                    continue;
                }
                bool ready;
                if (LobbyBotRegistry.IsBot(actor)) ready = botsReady;
                else if (!PhotonNetwork.CurrentRoom.Players.TryGetValue(actor, out Player player))
                    ready = true; // A player who explicitly left cannot acknowledge.
                else
                    ready = PlayerHasToken(player, ReadyKey, token) ||
                        (player.IsInactive && Time.unscaledTime - started >= disconnectedRemovalSeconds);
                if (!ready) everyoneReady = false;
                if (seatViewsByActorNumber.TryGetValue(actor, out SeatUIView seat) && seat != null)
                    seat.SetReadyIndicator(ready);
            }
            bool participating = PhotonNetwork.LocalPlayer != null && activePlayerOrder.Contains(PhotonNetwork.LocalPlayer.ActorNumber);
            roundReview.SetReviewVisible(participating,
                localReadyRequested || PlayerHasToken(PhotonNetwork.LocalPlayer, ReadyKey, token));
            int master = PhotonNetwork.CurrentRoom.MasterClientId;
            if (PhotonNetwork.IsMasterClient && everyoneReady && (!requestedContinue || master != requestedMaster))
            {
                requestedContinue = PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { ContinueKey, token } });
                requestedMaster = master;
            }
            yield return null;
        }
        IsAwaitingRoundReady = false;
        localReadyRequested = false;
        roundReview.SetReviewVisible(false);
        foreach (SeatUIView seat in seatViewsByActorNumber.Values)
            if (seat != null) seat.SetReadyIndicator(false);
    }

    private void HighlightReviewedCards()
    {
        if (cardDealTest == null) return;
        cardDealTest.RevealAllDealtCards();
        List<CardSpriteEntry> matches = MultiplayerHandRules.MatchingCards(
            GetHandIdFromOptionText(currentDeclaredRankText), cardDealTest.GetAllRoundCardsForEvaluation(), out bool complete);
        cardDealTest.HighlightMatchingCards(matches, complete);
        RefreshActiveHighlight(-1);
    }

    private IEnumerator ResumeRoundReview()
    {
        TryResolveCardDealTest();
        while (PhotonNetwork.InRoom && cardDealTest != null && !cardDealTest.HasFinishedDealing)
            yield return null;
        if (!PhotonNetwork.InRoom) yield break;
        roundReview.LoadHistory();
        TryGetRoomIntProp(ReviewRoundKey, out reviewRound);
        RoundReviewUI.Entry entry = roundReview.Latest;
        if (reviewRound == currentRoundNumber && entry != null && entry.round == currentRoundNumber)
        {
            // The penalty and history were saved together: never apply the penalty twice on rejoin.
            HighlightReviewedCards();
            yield return WaitForRoundReady(new List<int>(entry.participants));
            ContinueAfterReviewedRound(entry.loser, new List<int>(entry.participants));
        }
        else
        {
            isRoundTransitionInProgress = false;
            yield return ResolveCheckAndStartNextRound();
        }
    }

    private IEnumerator ResumePendingDeal()
    {
        TryResolveCardDealTest();
        while (PhotonNetwork.InRoom && cardDealTest != null && !cardDealTest.HasSeats)
            yield return null;
        if (!PhotonNetwork.InRoom || cardDealTest == null) yield break;
        TryGetRoomIntProp(NextDealLoserKey, out nextDealLoser);
        if (cardDealTest.HasFinishedDealing)
            NotifyRoundDealFinished(starterPlayerId);
        else
            cardDealTest.StartConfiguredRoundDeal(BuildRoundCardCountMap(), nextDealLoser,
                starterPlayerId, ComputeNextRoundSeed(currentRoundNumber, nextDealLoser));
    }

    private IEnumerator WaitForDealBarrier(int starter)
    {
        // Every client acknowledges after its last card reaches the table, not when the deal starts.
        string token = RoundToken;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { DealReadyKey, token } });
        bool requestedOpen = false;
        int requestedMaster = -1;
        float started = Time.unscaledTime;
        yield return null;
        while (PhotonNetwork.InRoom && !RoomHasToken(DealOpenKey, token))
        {
            bool everyoneDealt = true;
            foreach (int actor in activePlayerOrder)
            {
                if (LobbyBotRegistry.IsBot(actor)) continue;
                if (!PhotonNetwork.CurrentRoom.Players.TryGetValue(actor, out Player player)) continue;
                if (player.IsInactive && Time.unscaledTime - started >= disconnectedRemovalSeconds) continue;
                if (!PlayerHasToken(player, DealReadyKey, token)) everyoneDealt = false;
            }
            int master = PhotonNetwork.CurrentRoom.MasterClientId;
            if (PhotonNetwork.IsMasterClient && everyoneDealt && (!requestedOpen || requestedMaster != master))
            {
                requestedOpen = PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {
                    { DealOpenKey, token }, { "dealStartTimeV1", PhotonNetwork.Time }
                });
                requestedMaster = master;
            }
            yield return null;
        }
        dealBarrierRoutine = null;
        if (!PhotonNetwork.InRoom) yield break;
        CompleteRoundDeal(starter);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("dealStartTimeV1", out object raw) && raw is double time)
            currentTurnTimeLeft -= Mathf.Max(0f, (float)(PhotonNetwork.Time - time));
    }
}
