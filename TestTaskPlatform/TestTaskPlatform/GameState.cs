using System.Collections.Concurrent;
using DAL.Models;
using Microsoft.AspNetCore.SignalR;

namespace TestTaskPlatform;

internal class GameState<THub> where THub : Hub
{
    private ConcurrentDictionary<string, Player> _players =
        new(StringComparer.OrdinalIgnoreCase);
        
    private ConcurrentDictionary<string, Game> _games =
        new(StringComparer.OrdinalIgnoreCase);
        
    private ConcurrentQueue<Player> _waitingPlayers = new();

    public GameState(IHubContext<THub> context)
    {
        Groups = context.Groups;
    }

    public IGroupManager Groups { get; set; }

    public Player CreatePlayer(string username, string connectionId)
    {
        var player = new Player(username, connectionId);
        _players[connectionId] = player;

        return player;
    }
        
    public Player GetPlayer(string playerId)
    {
        Player foundPlayer;
        if (!_players.TryGetValue(playerId, out foundPlayer))
        {
            return null;
        }

        return foundPlayer;
    }

    public Game GetGame(Player player, out Player opponent)
    {
        opponent = null;
        var foundGame = _games.Values.FirstOrDefault(g => g.Id == player.GameId);

        if (foundGame == null)
        {
            return null;
        }

        opponent = player.Id == foundGame.Player1.Id ?
            foundGame.Player2 :
            foundGame.Player1;

        return foundGame;
    }
        
    public Player GetWaitingOpponent()
    {
        Player foundPlayer;
        Console.WriteLine($"Players in queue {_waitingPlayers.Count}");
        if (!_waitingPlayers.TryDequeue(out foundPlayer))
        {
            return null;
        }

        return foundPlayer;
    }
        
    public void RemoveGame(string gameId)
    {
        Game foundGame;
        if (!_games.TryRemove(gameId, out foundGame))
        {
            Console.Write("Game not found while removing.");
        }

        if (foundGame is not null)
        {
            _players.TryRemove(foundGame.Player1.Id, out _);
            _players.TryRemove(foundGame.Player2.Id, out _);
        }
    }
        
    public void AddToWaitingPool(Player player)
    {
        _waitingPlayers.Enqueue(player);
        Console.WriteLine($"Added to waiting pool {player.Id}|{player.Username} - {_waitingPlayers.Count}");
    }
        
    public bool IsUsernameTaken(string username)
    {
        return _players.Values.FirstOrDefault(player => player.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)) != null;
    }
        
    public async Task<Game> CreateGame(Player firstPlayer, Player secondPlayer)
    {
        var game = new Game(firstPlayer, secondPlayer);
        _games[game.Id] = game;
        
        await Groups.AddToGroupAsync(firstPlayer.Id, groupName: game.Id);
        await Groups.AddToGroupAsync(secondPlayer.Id, groupName: game.Id);

        return game;
    }
}