

namespace TechBox.Service.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TechBox.Model.Enum;
    public class Result
    {
        private Result(ResultStatusType status, string message = null)
        {
            this.Status = status;
            this.Message = message;
        }

        public string Message { get; set; }

        public ResultStatusType Status { get; }

        public static Result Failure(string message = null)
        {
            return new Result(ResultStatusType.Failure, message);
        }

        public static Result NotFound()
        {
            return new Result(ResultStatusType.NotFound);
        }

        public static Result Success()
        {
            return new Result(ResultStatusType.Success);
        }
    }
}
