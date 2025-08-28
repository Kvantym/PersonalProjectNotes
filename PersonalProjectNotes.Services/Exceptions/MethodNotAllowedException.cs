namespace PersonalProjectNotes.Services.Exceptions
{
    public class MethodNotAllowedException : Exception
    {
        public MethodNotAllowedException(string message) : base(message) { }
        public MethodNotAllowedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
