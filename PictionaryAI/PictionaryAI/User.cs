namespace PictionaryAI
{
    public class User
    {
        public User(string connectionId, Room room)
        {
            Id = RandomContainer.GetRandomUint();
            ConnectionId = connectionId;
            Room = room;
            Name = $"Player {Id}";
        }

        public uint Id { get; }
        public string ConnectionId { get; }
        public Room Room { get; }
        public string Name { get; set; }

        public void ChangeName(string newName)
        {
            Name = newName;
        }
    }
}
