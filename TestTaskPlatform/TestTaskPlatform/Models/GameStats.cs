using System.ComponentModel.DataAnnotations.Schema;

namespace TestTaskPlatform.Models;

public class GameStats
{
    public int Id { get; set; }
    
    public bool IsFirstUserWon { get; set; }
    
    public int FirstUserId { get; set; }
    [ForeignKey("FirstUserId")]
    public User FirstUser { get; set; }
    
    public int SecondUserId { get; set; }
    [ForeignKey("SecondUserId")]
    public User SecondUser { get; set; }
}