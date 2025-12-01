using System.ComponentModel.DataAnnotations;

namespace Daibitx.Grpc.Server.Exceptions
{
    public class ValidationException : Exception
    {
        public List<ValidationResult> Errors { get; }

        public ValidationException(List<ValidationResult> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }
}
