
namespace UtilClasses.Winforms
{
    public interface IInputValidator
    {
        bool Validate(object obj);
        string ErrorMessage { get; }
    }
}
