using GameOfHearts.ArtificialPlayer;
using GameOfHearts.ConsoleApp.Services;
using GameOfHearts.Game.Interfaces;
using GameOfHearts.Game.Services;
using GameOfHearts.HumanPlayer;
using GameOfHearts.LoggingProvider.Services;
using System.Text;

namespace GameOfHearts.ConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        using Logger logger = new();

        try
        {
            int playToScore = args.Length >= 1 ? int.Parse(args[0]) : 25;

            logger.LogInformation($"Starting game with target score '{playToScore}'");

            Console.OutputEncoding = Encoding.UTF8;
            GameService service = new(new ConsoleOutputService());
            for (int i = 0; i <= 2; i++)
            {
                AiPlayer player = new(i.ToString(), logger);
                service.RegisterPlayer(player);
            }
            ConsolePlayer human = new("You", logger);
            service.RegisterPlayer(human);

            int maxScored = -1;
            do
            {
                await service.PlayGame();
                foreach (IPlayer player in service.GetResults())
                {
                    Console.WriteLine($"Player {player.Name} took {player.Score}");
                    if (maxScored < player.Score)
                    {
                        maxScored = player.Score;
                    }
                }

                logger.LogInformation("Finished round, awaiting input...");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            } while (maxScored < playToScore);

            logger.LogInformation("Finished game, exit due...");
        }
        catch (Exception ex)
        {
            logger.LogError($"Unknown exception of type [{ex}]: {ex.Message}\nTrace: {ex.StackTrace}");
        }
    }
}
