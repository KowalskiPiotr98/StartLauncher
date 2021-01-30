namespace StartLauncher.Utilities.Updater
{

    [System.Serializable]
    public class UpdateException : System.Exception
    {
        public UpdateException() { }
        public UpdateException(string message) : base(message) { }
        public UpdateException(string message, System.Exception inner) : base(message, inner) { }
        protected UpdateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
