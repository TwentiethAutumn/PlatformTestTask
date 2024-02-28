namespace DAL.Models;

public class Player
{
    public Player(string username, string id)
    {
        Username = username;
        Id = id;
    }

    public string Id { get; set; }
    public string Username { get; set; }
    public string GameId { get; set; }
    public string Piece { get; set; }

    public override bool Equals(object obj)
    {
        Player other = obj as Player;

        if (other is null) return false;

        return Id.Equals(other.Id) && Username.Equals(other.Username);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode() * Username.GetHashCode();
    }
}