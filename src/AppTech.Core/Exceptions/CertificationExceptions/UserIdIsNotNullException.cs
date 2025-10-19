namespace AppTech.Core.Exceptions.CertificationExceptions
{
    public class UserIdIsNotNullException : Exception
    {
        public string Username { get; set; }
        public UserIdIsNotNullException(string username)
        {
            Username = username;
        }
    }
}
