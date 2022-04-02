using Microsoft.AspNetCore.SignalR;

namespace PictionaryAI
{
    public class PictionaryHub : Hub
    {
        private readonly RoomManager _roomManager;
        private readonly IHubContext<PictionaryHub> _context;

        public PictionaryHub(RoomManager roomManager, IHubContext<PictionaryHub> context)
        {
            _roomManager = roomManager;
            _context = context;
        }

        public async Task Ping(string test)
        {
            await Clients.Caller.SendAsync("Pong", test);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            await base.OnDisconnectedAsync(exception);
            if (_roomManager.ConnectionIdExists(connectionId))
            {
                Room room = _roomManager.GetRoomFromConnectionId(connectionId);
                await _roomManager.RemoveUser(_context, connectionId);
            }
        }

        public async Task DrawingGuessed()
        {
            string connectionId = Context.ConnectionId;
            if (!_roomManager.ConnectionIdExists(connectionId))
            {
                throw new InvalidOperationException("Tried to call drawing guessed when the player is not in a room");
            }
            Room room = _roomManager.GetRoomFromConnectionId(connectionId);
            if (!room.IsStarted || !room.IsRoundInProgress)
            {
                throw new InvalidOperationException("Tried to call drawing guessed when a round is not in progress");
            }
            User user = room.GetUserFromConnectionId(connectionId);
            if (user.HasCompletedDrawing)
            {
                throw new InvalidOperationException("Tried to call drawing guessed when player has already has their drawing guessed");
            }
            await _roomManager.PlayerCompletedDrawing(_context, connectionId);
        }
    }
}
