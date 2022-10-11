


namespace TechBox.Service.Utility
{
    using TechBox.Model.Enum;
    public class Result<T>
    {
        private Result(ResultStatusType status, string message = null)
        {
            this.Status = status;
            this.Message = message;
        }

        private Result(ResultStatusType status, T entity, string message = null)
        {
            this.Status = status;
            this.Entity = entity;
            this.Message = message;
        }

        public T Entity { get; }

        public bool IsSuccess => this.Status == ResultStatusType.Success;

        public string Message { get; }

        public ResultStatusType Status { get; }

        public static Result<T> Failure(string message = null)
        {
            return new Result<T>(ResultStatusType.Failure, message);
        }

        public static Result<T> Failure(T entity)
        {
            return new Result<T>(ResultStatusType.Failure, entity);
        }

        public static Result<T> NotFound()
        {
            return new Result<T>(ResultStatusType.NotFound);
        }

        public static Result<T> Success(T entity)
        {
            return new Result<T>(ResultStatusType.Success, entity);
        }
    }
}
