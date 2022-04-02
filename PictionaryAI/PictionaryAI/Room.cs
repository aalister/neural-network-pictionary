namespace PictionaryAI
{
    public class Room
    {
        private readonly Dictionary<string, User> _connIdToUser;

        public Room()
        {
            _connIdToUser = new Dictionary<string, User>();
            string newId = Guid.NewGuid().ToString();
            Id = newId.Substring(newId.IndexOf('-'));

        }

        public string Id { get; }

        public bool ConnectionIdExists(string connectionId)
        {
            return _connIdToUser.ContainsKey(connectionId);
        }

        public User GetUserFromConnectionid(string connectionId)
        {
            if (!ConnectionIdExists(connectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            return _connIdToUser[connectionId];
        }

        public User AddUser(string connectionId)
        {
            User user = new User(connectionId, this);
            _connIdToUser.Add(user.ConnectionId, user);
            return user;
        }

        public User RemoveUser(string connectionId)
        {
            User userToRemove = GetUserFromConnectionid(connectionId);
            _connIdToUser.Remove(connectionId);
            return userToRemove;
        }
    }
}
