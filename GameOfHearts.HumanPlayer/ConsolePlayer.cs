using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Interfaces;
using GameOfHearts.Game.Models;
using GameOfHearts.HumanPlayer.Services;
using GameOfHearts.LoggingProvider.Extensions;
using GameOfHearts.LoggingProvider.Services;

namespace GameOfHearts.HumanPlayer;

public sealed class ConsolePlayer : IPlayer
{
    private List<Card> _hand;
    private readonly List<Card> _taken;

    private readonly InputOutputService _ioService;
    private readonly Logger _logger;

    public string Name { get; init; }
    public int Score { get; set; }

    public ConsolePlayer(Logger logger)
    {
        _logger = logger;
        _hand = new();
        _taken = new();
        _ioService = new(); 
        Score = 0;
        Name = Guid.NewGuid().ToString();
    }

    public ConsolePlayer(string name, Logger logger)
    {
        _logger = logger;
        _hand = new();
        _taken = new();
        _ioService = new(); 
        Score = 0;
        Name = name;
    }

    public IEnumerable<Card> GetTakenCards()
    {
        _logger.LogInformation($"Human Player {Name} returning taken cards: '{_taken.Stringify()}'");
        return _taken;
    }

    public Task GrabDealtCard(Card card)
    {
        _logger.LogInformation($"Human Player {Name} adding card: '{card}' to hand");
        _hand.Add(card);
        SortHand();
        return Task.CompletedTask;
    }

    public bool HasCards()
    {
        return _hand.Count > 0;
    }

    public Task TakeTrick(IEnumerable<Card> cards)
    {
        _logger.LogInformation($"Human Player {Name} taking trick: '{cards.Stringify()}'");
        _taken.AddRange(cards);
        return Task.CompletedTask;
    }

    public Task<Card> ThrowCard()
    {
        _ioService.PrintHandAndRegisterViable(_hand);
        Card card = _ioService.ReadInputAndClearViable();
        _hand.Remove(card);
        _logger.LogInformation($"Human Player {Name} throwing initial card: '{card}'");
        return Task.FromResult(card);
    }

    public Task<Card> ThrowCard(Card initial, ReadOnlySpan<Card> trick)
    {
        CardSuit suit = initial.Suit;
        bool hasSuit = false;
        Dictionary<Card, bool> viabilityPairs = new();

        foreach (Card card in _hand)
        {
            viabilityPairs.Add(card, card.Suit == suit);
            hasSuit |= card.Suit == suit;
        }

        if (hasSuit)
        {
            _ioService.PrintHandAndRegisterViable(viabilityPairs);
        }
        else
        {
            _ioService.PrintHandAndRegisterViable(_hand);
        }

        Card thrown = _ioService.ReadInputAndClearViable();
        _hand.Remove(thrown);
        _logger.LogInformation($"Human Player {Name} throwing card: '{thrown}'");
        return Task.FromResult(thrown);
    }

    public void SortHand()
    {
        _hand = _hand.OrderBy(x => x.Suit)
            .ThenByDescending(x => x.Face)
            .ToList();
    }

    public void ClearTakenCards()
    {
        _taken.Clear();
    }

    public Task TryRemember(Dictionary<int, Card> trick)
    {
        return Task.CompletedTask;
    }
}
