using System.Text.Json;

namespace Blog.ExceptionHandling
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        
        public string response_status{get;set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
