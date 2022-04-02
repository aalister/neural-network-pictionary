namespace PictionaryAI
{
    public class RoomManager
    {
        private readonly Dictionary<string, Room> _rooms;
        private readonly Dictionary<string, Room> _connIdToRoom;

        public RoomManager()
        {
            _rooms = new Dictionary<string, Room>();
            _connIdToRoom = new Dictionary<string, Room>();
        }

        public class UserChangeEventArgs : EventArgs
        {
            public UserChangeEventArgs(Room room, User user)
            {
                Room = room;
                User = user;
            }

            public Room Room { get; }
            public User User { get; }
        }

        public event EventHandler<UserChangeEventArgs>? OnUserAdded;
        public event EventHandler<UserChangeEventArgs>? OnUserRemoved;

        public bool RoomIdExists(string id)
        {
            return _rooms.ContainsKey(id);
        }
        public bool ConnectionIdExists(string connectionId)
        {
            return _connIdToRoom.ContainsKey(connectionId);
        }

        public Room GetRoomFromRoomId(string id)
        {
            if (!RoomIdExists(id))
            {
                throw new ArgumentException("Room id does not exist");
            }
            return _rooms[id];
        }

        public Room GetRoomFromConnectionid(string connectionId)
        {
            if (!ConnectionIdExists(connectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            return _rooms[connectionId];
        }

        public Room CreateRoom()
        {
            Room room = new Room();
            _rooms.Add(room.Id, room);
            return room;
        }

        public User AddUser(string roomId, string connectionId)
        {
            Room room = GetRoomFromRoomId(roomId);
            User user = room.AddUser(connectionId);
            _connIdToRoom.Add(user.ConnectionId, room);
            OnUserAdded?.Invoke(user, new UserChangeEventArgs(room, user));
            return user;
        }

        public void RemoveUser(string connectionId)
        {
            Room room = GetRoomFromConnectionid(connectionId);
            User user = room.RemoveUser(connectionId);
            OnUserRemoved?.Invoke(room, new UserChangeEventArgs(room, user));
        }
    }
}
