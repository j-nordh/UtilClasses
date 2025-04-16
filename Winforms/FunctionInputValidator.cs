using System;

namespace UtilClasses.Winforms
{
    public class FunctionInputValidator :IInputValidator
    {
        private readonly Func<string, bool> _f; 
        public FunctionInputValidator(Func<string,bool> f, string errorMessage)
        {
            _f = f;
            ErrorMessage = errorMessage;
        }

        public bool Validate(object obj)
        {
            return _f(obj.ToString());
        }
        public string  ErrorMessage { get; }
        
    }
}
