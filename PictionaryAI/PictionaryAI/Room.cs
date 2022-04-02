namespace PictionaryAI
{
    public class Room
    {
        private readonly Dictionary<string, User> _connIdToUser;

        public Room()
        {
            _connIdToUser = new Dictionary<string, User>();
            string newId = Guid.NewGuid().ToString();
            Id = newId.Substring(0, newId.IndexOf('-'));
        }

        public string Id { get; }

        public bool ConnectionIdExists(string connectionId)
        {
            return _connIdToUser.ContainsKey(connectionId);
        }

        public User GetUserFromConnectionId(string connectionId)
        {
            if (!ConnectionIdExists(connectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            return _connIdToUser[connectionId];
        }

        public User[] GetUsers() => _connIdToUser.Values.ToArray();

        public User AddUser(string connectionId, string? name = null)
        {
            User user = new User(connectionId, this, _connIdToUser.Count == 0, name);
            _connIdToUser.Add(user.ConnectionId, user);
            return user;
        }

        public User RemoveUser(string connectionId)
        {
            User userToRemove = GetUserFromConnectionId(connectionId);
            if (_connIdToUser.Count > 1)
            {
                //Change host to someone else before we leave
                ChangeHost(_connIdToUser.Values.First(user => user.ConnectionId != userToRemove.ConnectionId));
            }
            _connIdToUser.Remove(connectionId);
            return userToRemove;
        }

        public void ChangeHost(User userToChange)
        {
            if (!ConnectionIdExists(userToChange.ConnectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            foreach (User user in _connIdToUser.Values)
            {
                user.ChangeIsHost(false);
            }
            userToChange.ChangeIsHost(true);
        }
    }
}
