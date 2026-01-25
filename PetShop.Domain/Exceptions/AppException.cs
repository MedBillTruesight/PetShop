using System.Globalization;

namespace PetShop.Domain.Exceptions;
public class AppException: Exception
{
    public ValidationCode ValidationCode {get; set;} = ValidationCode.None;

    public AppException():base() {}

    public AppException(string message): base(message) {}
    public AppException(string message, params object[] args): base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
        
    }

    public AppException(string message, ValidationCode code): base(message)
    {
     ValidationCode Code = code;
    }
}