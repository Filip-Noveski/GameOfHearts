using GameOfHearts.ArtificialPlayer.Services;
using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Interfaces;
using GameOfHearts.Game.Models;
using GameOfHearts.Game.Services;
using GameOfHearts.LoggingProvider.Extensions;
using GameOfHearts.LoggingProvider.Services;
using System.Diagnostics;

namespace GameOfHearts.ArtificialPlayer;

/// <summary>
/// AI Player implementation. 
/// </summary>
public sealed class AiPlayer : IPlayer
{
    private static readonly float HeartsRememberProb = 0.95f;
    private static readonly float SameSuitRememberProb = 0.80f;
    private static readonly float OtherSuitRememberProb = 0.35f;

    private readonly List<Card> _hand;
    private readonly List<Card> _taken;
    private readonly List<Card> _memory; // cards known to have been thrown
    private readonly Random _random;

    private readonly LaterThrowOtherSuitService _laterThrowOtherSuitService;
    private readonly LaterThrowSameSuitService _laterThrowSameSuitService;
    private readonly InitialThrowService _initialThrowService;
    private readonly Logger _logger;

    public string Name { get; init; }
    public int Score { get; set; } = 0; // set initial score to 0

    public AiPlayer(Logger logger)
    {
        _logger = logger;
        _hand = new();
        _taken = new();
        _memory = new();
        _random = new();
        _laterThrowOtherSuitService = new(_logger);
        _laterThrowSameSuitService = new(_logger);
        _initialThrowService = new(_logger);
        Score = 0;
        Name = Guid.NewGuid().ToString();
    }

    public AiPlayer(string name, Logger logger)
    {
        _logger = logger;
        _hand = new();
        _taken = new();
        _memory = new();
        _random = new();
        _laterThrowOtherSuitService = new(_logger);
        _laterThrowSameSuitService = new(_logger);
        _initialThrowService = new(_logger);
        Score = 0;
        Name = name;
    }

    public IEnumerable<Card> GetTakenCards()
    {
        _logger.LogInformation($"AI Player {Name} returning taken cards: '{_taken.Stringify()}'");
        return _taken;
    }

    public Task GrabDealtCard(Card card)
    {
        _logger.LogInformation($"AI Player {Name} adding card: '{card}' to hand");
        _hand.Add(card);
        return Task.CompletedTask;
    }

    public bool HasCards()
    {
        return _hand.Count > 0;
    }

    public Task TakeTrick(IEnumerable<Card> cards)
    {
        _logger.LogInformation($"AI Player {Name} taking trick: '{cards.Stringify()}'");
        _taken.AddRange(cards);
        return Task.CompletedTask;
    }

    public Task<Card> ThrowCard()
    {
        // log in service
        _logger.LogInformation($"AI Player {Name} making decision for initial throw...");
        Card card = _initialThrowService.MakeDecision(_hand, _memory);
        _hand.Remove(card);
        _logger.LogInformation($"AI Player {Name} throwing initial card: '{card}'");
        return Task.FromResult(card);
    }

    public Task<Card> ThrowCard(Card initial, ReadOnlySpan<Card> trick)
    {
        _logger.LogInformation($"AI Player {Name} making decision...");
        CardSuit suit = initial.Suit;
        int countSuit = _hand.Count(x => x.Suit == suit);
        Card card = countSuit switch
        {
            0 => _laterThrowOtherSuitService.MakeDecision(_hand),
            1 => _hand.Find(x => x.Suit == suit) ?? throw new UnreachableException(
                $"Count of Suit: {suit} was 1, yet suit itself was not found"),
            > 1 => _laterThrowSameSuitService.MakeDecision(_hand, suit, trick),
            _ => throw new UnreachableException($"Count was negative with value: {countSuit}")
        };
        _hand.Remove(card);
        _logger.LogInformation($"AI Player {Name} throwing card: '{card}'");
        return Task.FromResult(card);
    }

    public void ClearTakenCards()
    {
        _taken.Clear();
        _memory.Clear();
    }

    private void TryRememberCard(Card card, bool initialSuit)
    {
        float rn = (float)_random.NextDouble();
        float compare;
        // get compare value
        if (card.Suit is CardSuit.Heart)
        {
            compare = HeartsRememberProb;
        }
        else if (initialSuit)
        {
            compare = SameSuitRememberProb;
        }
        else
        {
            compare = OtherSuitRememberProb;
        }

        // check whether to remember and act
        if (rn <= compare)
        {
            _logger.LogInformation($"AI Player {Name} remembering card: '{card}'");
            _memory.Add(card);
        }
    }

    public Task TryRemember(Dictionary<int, Card> trick)
    {
        CardSuit initialSuit = trick[0].Suit;
        foreach (var pair in trick)
        {
            Card card = pair.Value;
            TryRememberCard(card, initialSuit == card.Suit);
        }
        return Task.CompletedTask;
    }
}
