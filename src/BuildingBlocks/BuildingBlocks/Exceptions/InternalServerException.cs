

namespace BuildingBlocks.Exceptions
{
    public class InternalServerException : Exception
    {
        public string? Details { get;  }
        public InternalServerException(string name): base(name)
        {
        }

        public InternalServerException(string message, string details): base(message)
        {
            Details = details;
        }
    }
}
