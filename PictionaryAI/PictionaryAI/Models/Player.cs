namespace PictionaryAI.Models
{
    public class Player
    {
        public Player(uint id, bool isHost, string name, uint score, bool hasGuessed)
        {
            Id = id;
            IsHost = isHost;
            Name = name;
            Score = score;
        }

        public uint Id { get; set; }
        public bool IsHost { get; set; }
        public string Name { get; set; }
        public uint Score { get; set; }
        public bool HasGuessed { get; set; }
    }
}
