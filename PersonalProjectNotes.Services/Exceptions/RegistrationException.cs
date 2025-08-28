namespace PersonalProjectNotes.Services.Exceptions
{
    public class RegistrationException : Exception
    {
        public RegistrationException(string message) : base(message) { }
        public RegistrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
