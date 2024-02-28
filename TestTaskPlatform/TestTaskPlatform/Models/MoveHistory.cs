namespace TestTaskPlatform.Models;

public class MoveHistory
{
    public int Id { get; set; }

    public int MoveTurn { get; set; }
    public int MoveX { get; set; }
    public int MoveY { get; set; }
    
    public int GameStatsId { get; set; }
    public GameStats GameStats { get; set; }
}