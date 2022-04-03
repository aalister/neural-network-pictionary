namespace PictionaryAI
{
    public class User
    {
        public User(uint id, string connectionId, Room room, bool isHost, string? name = null)
        {
            Id = id;
            ConnectionId = connectionId;
            Room = room;
            IsHost = isHost;
            Name = name ?? $"Player {Id}";
            Score = 0;
            HasCompletedDrawing = false;
        }

        public uint Id { get; }
        public string ConnectionId { get; }
        public Room Room { get; }
        public bool IsHost { get; set; }
        public string Name { get; set; }
        public uint Score { get; set; }
        public bool HasCompletedDrawing { get; set; }

        public void AddScoreForCompletingDrawing(int timeTakenMillis, int roundLengthMillis)
        {
            HasCompletedDrawing = true;
            const uint baseValue = 25;
            const uint scaleValue = 75;
            Score += baseValue + (uint)((1 - ((double)timeTakenMillis / (double)roundLengthMillis)) * scaleValue);
        }
    }
}
