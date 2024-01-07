using GameOfHearts.Game.DataStructures;
using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Exceptions;
using GameOfHearts.Game.Interfaces;
using GameOfHearts.Game.Models;

namespace GameOfHearts.Game.Services;

public sealed class GameService
{
    private readonly LoopedList<IPlayer> _players;
    private readonly Dictionary<int, Card> _trick;
    private readonly IOutputService _outputService;

    public GameService(IOutputService outputService)
    {
        _players = new();
        _trick = new();
        _outputService = outputService;
    }

    public void RegisterPlayer(IPlayer player)
    {
        _players.Add(player);
    }

    private async Task RemaningPlayersThrow(Card initial)
    {
        // move on from intial thrower
        _players.MoveNext();

        for (int i = 1; i <= _players.Count - 1; i++)
        {
            Card[] trickArray = _trick.Values.ToArray();
            Console.Clear();
            _outputService.DisplayTrick(trickArray);
            await Task.Delay(1500);

            Card card = await _players.Selected!.ThrowCard(initial, trickArray);
            _trick.Add(_players.SelectedId, card);
            _players.MoveNext();
        }
    }

    private static void TestTrickCard(
        KeyValuePair<int, Card> trickPair,
        CardSuit initialSuit, 
        ref KeyValuePair<int, Card>? taker)
    {
        Card card = trickPair.Value;
        int playerId = trickPair.Key;

        // not the initial suit, ignore
        if (card.Suit != initialSuit)
        {
            return;
        }

        // initial suit, but no taker selected yet; set first as taker
        if (taker == null)
        {
            taker = new KeyValuePair<int, Card>(playerId, card);
            return;
        }

        // initial suit and taker selected; compare
        int currentWeight = CardService.GetCardWeight(card);
        int oldWeight = CardService.GetCardWeight(taker?.Value!);

        // old is still taker; move on
        if (oldWeight > currentWeight)
        {
            return;
        }

        // current should be taker, swap
        taker = new KeyValuePair<int, Card>(playerId, card);
    }

    private async Task FindTaker(CardSuit initialSuit)
    {
        KeyValuePair<int, Card>? taker = null;
        foreach (KeyValuePair<int, Card> trickPair in _trick)
        {
            TestTrickCard(trickPair, initialSuit, ref taker);
        }

        // move to taker, give trick and clear it
        int takerId = (int)taker?.Key!;
        _players.MoveTo(takerId);
        await _players.Selected!.TakeTrick(_trick.Values.ToArray());
    }

    private void CalculateScores()
    {
        for (int i = 0; i <= _players.Count - 1; i++) 
        {
            IPlayer current = _players.Selected!;
            int score = CardService.GetTotalScore(current.GetTakenCards());
            current.Score += score;
            current.ClearTakenCards();
            _players.MoveNext();
        }
    }

    private void DealCards()
    {
        Deck deck = new();
        deck.Shuffle();
        while (!deck.IsEmpty())
        {
            _players.Selected!.GrabDealtCard(deck.GetCard());
            _players.MoveNext();
        }
    }

    private async Task PlayersRemember()
    {
        List<Task> tasks = new();
        for (int i = 0; i <= _players.Count - 1; i++)
        {
            tasks.Add(_players.Selected!.TryRemember(_trick));
            _players.MoveNext();
        }

        await Task.WhenAll(tasks);
    }

    private async Task Flush()
    {
        Console.Clear();
        _outputService.DisplayTrick(_trick.Values.ToArray());
        _outputService.DisplayTaker(_players.Selected!.Name);

        await Task.Delay(3500);
        Console.Clear();
        _trick.Clear();
    }

    public async Task PlayGame()
    {
        if (_players.Count == 0)
        {
            throw new NoPlayersException();
        }

        DealCards();

        while (_players.Selected!.HasCards())
        {
            Card initial = await _players.Selected.ThrowCard(); // throw initial card
            _trick.Add(_players.SelectedId, initial); 
            await RemaningPlayersThrow(initial);
            await FindTaker(initial.Suit);
            await PlayersRemember();
            await Flush();
        }

        CalculateScores();
    }

    private static void SortByScore(ref IPlayer[] players)
    {
        for (int i = 0; i <= players.Length - 1; i++)
        {
            for (int j = i + 1; j <= players.Length - 1; j++)
            {
                if (players[i].Score < players[j].Score)
                {
                    (players[i], players[j]) = (players[j], players[i]);
                }
            }
        }
    }

    public IEnumerable<IPlayer> GetResults()
    {
        IPlayer[] players = _players.ToArray();
        SortByScore(ref players);
        return players;
    }
}
