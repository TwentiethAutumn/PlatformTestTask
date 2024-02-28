using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using TestTaskPlatform.Context;

namespace TestTaskPlatform.Hubs;

[SignalRHub("/lobby")]
[AllowAnonymous]
internal class Lobby : Hub
{
    private GameState<Lobby> _gameState;
    private readonly ApiContext _context;

    public Lobby(
        GameState<Lobby> gameState,
        ApiContext context
    )
    {
        _context = context;
        _gameState = gameState;
    }
    
    [AllowAnonymous] // TODO: change to auth when fix on front | works on console app
    public async Task FindGame(string username)
    {
        if (_gameState.IsUsernameTaken(username))
        {
            await Clients.Caller.SendAsync(MethodContract.UsernameTaken);
            return;
        }

        var joiningPlayer = _gameState.CreatePlayer(username, Context.ConnectionId);
        await Clients.Caller.SendAsync(MethodContract.JoiningPlayer);
            
        var opponent = _gameState.GetWaitingOpponent();
        Console.WriteLine($"Opponent state {opponent is null}");
        if (opponent == null)
        {
            _gameState.AddToWaitingPool(joiningPlayer);
            await Clients.Caller.SendAsync(MethodContract.WaitingList);
        }
        else
        {
            Console.WriteLine("Game should start now");
            var newGame = await _gameState.CreateGame(opponent, joiningPlayer);
            await Clients.Group(newGame.Id).SendAsync(MethodContract.Start, newGame);
        }
    }
    
    [AllowAnonymous] // TODO: change to auth when fix on front | works on console app
    public async void PlacePiece(int row, int col)
    {
        var playerMakingTurn = _gameState.GetPlayer(playerId: Context.ConnectionId);
        var game = _gameState.GetGame(playerMakingTurn, out _);
        
        if (game == null || !game.WhoseTurn.Equals(playerMakingTurn)) // TODO: maybe to remove this due to lack of need for now
        {
            await Clients.Caller.SendAsync(MethodContract.NotPlayersTurn);
            return;
        }

        if (!game.IsValidMove(row, col)) // TODO: maybe to remove this due to lack of need for now
        {
            await Clients.Caller.SendAsync(MethodContract.NotValidMove);
            return;
        }
        
        game.PlacePiece(row, col);
        
        await Clients.Group(game.Id).SendAsync(MethodContract.PiecePlaced, row, col, playerMakingTurn.Piece);

        if (!game.IsOver)
        {
            await Clients.Group(game.Id).SendAsync(MethodContract.UpdateTurn, game);
        }
        else
        {
            if (game.IsTie)
            {
                await Clients.Group(game.Id).SendAsync(MethodContract.UpdateTurn, game);
                await Clients.Group(game.Id).SendAsync(MethodContract.TieGame);
            }
            else
            {
                await Clients.Group(game.Id).SendAsync(MethodContract.UpdateTurn, game);
                await Clients.Group(game.Id).SendAsync(MethodContract.Winner, playerMakingTurn.Username);
            }
            
            _gameState.RemoveGame(game.Id);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var leavingPlayer = _gameState.GetPlayer(playerId: Context.ConnectionId);
        
        if (leavingPlayer != null)
        {
            var ongoingGame = _gameState.GetGame(leavingPlayer, out _);
            if (ongoingGame != null)
            {
                await Clients.Group(ongoingGame.Id).SendAsync(MethodContract.OpponentLeft);
                _gameState.RemoveGame(ongoingGame.Id);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}