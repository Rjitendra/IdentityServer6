using Newtonsoft.Json;

namespace TechBox.API.Extentions
{
    public class ErrorDetails
    {
        public int Status { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}