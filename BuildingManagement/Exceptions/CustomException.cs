namespace BuildingManagement.Exceptions
{
    public class CustomException : Exception
    {
        public virtual string ErrorType { get { return "CUSTOM"; } }

        public string ErrorCode { get; }

        private string _customMessage = "";

        public string CustomMessage
        {
            get
            {
                if (string.IsNullOrEmpty(_customMessage))
                {
                    return $"Generic error";
                }

                return _customMessage;
            }
            set
            {
                _customMessage = value;
            }
        }

        public CustomException() : this("0")
        {
        }

        public CustomException(string errorCode)
        {
            ErrorCode = errorCode;
        }

        public CustomException(string errorCode, Exception? innerException) : base("", innerException)
        {
            ErrorCode = errorCode;
        }

        public CustomException(string errorCode, string message)
        {
            ErrorCode = errorCode;
            CustomMessage = message;
        }

        public CustomException(string errorCode, string message, Exception? innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
            CustomMessage = message;
        }

        public string ErrorId
        {
            get
            {
                return $"[{ErrorType}][{ErrorCode}]";
            }
        }

        public override string Message
        {
            get
            {
                if (InnerException != null)
                {
                    return $"{ErrorId} {CustomMessage} ({InnerException.Message})";
                }

                return $"{ErrorId} {CustomMessage}";
            }
        }
    }
}
