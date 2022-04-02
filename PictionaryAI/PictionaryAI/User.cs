namespace PictionaryAI
{
    public class User
    {
        public User(string connectionId, Room room, bool isHost, string? name = null)
        {
            Id = RandomContainer.GetRandomUint();
            ConnectionId = connectionId;
            Room = room;
            IsHost = isHost;
            Name = name ?? $"Player {Id}";
            Score = 0;
        }

        public uint Id { get; }
        public string ConnectionId { get; }
        public Room Room { get; }
        public bool IsHost { get; private set; }
        public string Name { get; private set; }
        public uint Score { get; private set; }

        public void ChangeName(string newName)
        {
            Name = newName;
        }

        public void ChangeIsHost(bool isHost)
        {
            IsHost = isHost;
        }
    }
}
