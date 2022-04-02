using Microsoft.AspNetCore.SignalR;

namespace PictionaryAI
{
    public class PictionaryHub : Hub
    {
        private readonly RoomManager _roomManager;

        public PictionaryHub(RoomManager roomManager)
        {
            _roomManager = roomManager;
            //_roomManager.OnUserAdded += async (object? sender, RoomManager.UserChangeEventArgs e) =>
            //{
            //    await Groups.AddToGroupAsync(e.User.ConnectionId, e.Room.Id);
            //};
            //_roomManager.OnUserRemoved += async (object? sender, RoomManager.UserChangeEventArgs e) =>
            //{
            //    await Groups.RemoveFromGroupAsync(e.User.ConnectionId, e.Room.Id);
            //};
        }

        public async Task Ping(string test)
        {
            await Clients.Caller.SendAsync("Pong", test);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            await base.OnDisconnectedAsync(exception);
            _roomManager.RemoveUser(connectionId);
        }
    }
}
