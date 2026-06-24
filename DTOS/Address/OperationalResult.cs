namespace Minimart_Api.DTOS.Address
{
    public class OperationResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public object Data { get; }

        private OperationResult(bool success, string message, object data = null)
        {
            IsSuccess = success;
            Message = message;
            Data = data;
        }

        public static OperationResult Success(object data = null, string message = "")
            => new OperationResult(true, message, data);

        public static OperationResult Failure(string message)
            => new OperationResult(false, message);
    }
}
