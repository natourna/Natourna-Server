namespace BuildingManagement.Exceptions
{
    /// <summary>
    /// Custom exception for API Manager operations with error codes and detailed messages
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Error code that identifies the specific operation that failed (e.g., BILL-API-001)
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// User-friendly message describing what went wrong
        /// </summary>
        public string UserMessage { get; }

        /// <summary>
        /// Technical details for logging/debugging
        /// </summary>
        public string TechnicalDetails { get; }

        public ApiException(
            string errorCode, 
            string userMessage, 
            string technicalDetails, 
            Exception? innerException = null) 
            : base($"[{errorCode}] {userMessage}", innerException)
        {
            ErrorCode = errorCode;
            UserMessage = userMessage;
            TechnicalDetails = technicalDetails;
        }

        /// <summary>
        /// Returns a formatted string with all exception details
        /// </summary>
        public string GetFullDetails()
        {
            var details = $"Error Code: {ErrorCode}\n" +
                         $"Message: {UserMessage}\n" +
                         $"Technical Details: {TechnicalDetails}";

            if (InnerException != null)
            {
                details += $"\nInner Exception: {InnerException.Message}";
            }

            return details;
        }
    }
}
