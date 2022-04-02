using Microsoft.AspNetCore.SignalR;

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

        public Room GetRoomFromConnectionId(string connectionId)
        {
            if (!ConnectionIdExists(connectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            return _connIdToRoom[connectionId];
        }

        public Room CreateRoom()
        {
            Room room = new Room();
            _rooms.Add(room.Id, room);
            return room;
        }

        public async Task<User> AddUser(IHubContext<PictionaryHub> context, string roomId, string connectionId, string? name = null)
        {
            Room room = GetRoomFromRoomId(roomId);
            User user = room.AddUser(connectionId, name);
            _connIdToRoom.Add(user.ConnectionId, room);
            await context.Groups.AddToGroupAsync(connectionId, room.Id);
            await SendPlayerListChange(context, room);
            return user;
        }

        public async Task RemoveUser(IHubContext<PictionaryHub> context, string connectionId)
        {
            Room room = GetRoomFromConnectionId(connectionId);
            User user = room.RemoveUser(connectionId);
            _connIdToRoom.Remove(user.ConnectionId);
            await context.Groups.RemoveFromGroupAsync(user.ConnectionId, room.Id);
            await SendPlayerListChange(context, room);

            //If the room is now empty, delete the room
            if (room.IsEmpty())
            {
                _rooms.Remove(room.Id);
            }
        }

        public async Task ChangeUserName(IHubContext<PictionaryHub> context, string connectionId, string name)
        {
            Room room = GetRoomFromConnectionId(connectionId);
            User user = room.GetUserFromConnectionId(connectionId);
            user.ChangeName(name);
            await SendPlayerListChange(context, room);
        }

        private async Task SendPlayerListChange(IHubContext<PictionaryHub> context, Room room)
        {
            await context.Clients.Group(room.Id).SendAsync("PlayerListChange", room.GetUsers().Select(user => new Models.Player(user.Id, user.IsHost, user.Name, user.Score)));
        }
    }
}
