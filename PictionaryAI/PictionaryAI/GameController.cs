using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PictionaryAI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly RoomManager _roomManager;
        private readonly PictionaryHub _pictionaryHub;

        public GameController(RoomManager roomManager, PictionaryHub pictionaryHub)
        {
            _roomManager = roomManager;
            _pictionaryHub = pictionaryHub;
        }

        [HttpPost]
        [Route("host")]
        [Route("host/{name}")]
        public async Task<ActionResult<string>> Host([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "name")] string? name = null)
        {
            Room room = _roomManager.CreateRoom();
            await _roomManager.AddUser(_pictionaryHub, room.Id, connectionId, name);
            return Ok(room.Id);
        }

        [HttpPost]
        [Route("join/{code}")]
        [Route("join/{code}/{name}")]
        public async Task<ActionResult> Join([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "code")] string roomId, [FromRoute(Name = "name")] string? name = null)
        {
            if (!_roomManager.RoomIdExists(roomId))
            {
                return NotFound();
            }
            await _roomManager.AddUser(_pictionaryHub, roomId, connectionId, name);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> SetName([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "name")] string name)
        {
            if (!_roomManager.ConnectionIdExists(connectionId))
            {
                return NotFound();
            }
            await _roomManager.ChangeUserName(_pictionaryHub, connectionId, name);
            return Ok();
        }
    }
}
