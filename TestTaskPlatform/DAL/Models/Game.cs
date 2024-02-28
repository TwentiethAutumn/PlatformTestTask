namespace DAL.Models;

public class Game
{
    public bool IsFirstPlayersTurn;
    
    public Game(){}
    
    public Game(Player player1, Player player2)
    {
        Player1 = player1;
        Player2 = player2;
        Id = Guid.NewGuid().ToString("d");
        Board = new Board();

        IsFirstPlayersTurn = true;

        Player1.GameId = Id;
        Player2.GameId = Id;

        Player1.Piece = "X";
        Player2.Piece = "O";
    }

    public string Id { get; set; }
    public Player Player1 { get; set; }
    public Player Player2 { get; set; }
    public Board Board { get; set; }
    
    public Player WhoseTurn => IsFirstPlayersTurn ? Player1 : Player2;

    public bool IsOver => IsTie || Board.IsThreeInRow;

    public bool IsTie => !Board.AreSpacesLeft;

    public void PlacePiece(int row, int col)
    {
        string pieceToPlace = IsFirstPlayersTurn ? Player1.Piece : Player2.Piece;
        
        Board.PlacePiece(row, col, pieceToPlace);
        IsFirstPlayersTurn = !IsFirstPlayersTurn;
    }
    
    public bool IsValidMove(int row, int col)
    {
        return 
            row < Board.Pieces.GetLength(0) &&
            col < Board.Pieces.GetLength(1) &&
            string.IsNullOrWhiteSpace(Board.Pieces[row, col]);
    }

    public override string ToString()
    {
        return String.Format("(Id={0}, Player1={1}, Player2={2}, Board={3})",
            Id, Player1, Player2, Board);
    }
}