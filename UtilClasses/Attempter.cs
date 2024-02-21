using System;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses
{
    public class Attempter
    {
        public bool IsFaulted { get; private set; }
        public event Action<Exception, string> CaughtException;

        public void Do(bool gate, Action a, string when)
        {
            if (!gate) return;
            Do(a, when);
        }
        public void Do(Action a, string when) => Do(()=> 
        {
            a();
            return true;
        }, when);

        public void Do(bool gate, Func<bool> f, string when)
        {
            if (!gate) return;
            Do(f, when);
        }

        public void Do(Func<StringBuilder, bool> f, string when)
        {
            try
            {
                if (IsFaulted) return;
                var sb= new StringBuilder();
                if (f(sb)) return;
            }
            catch (Exception e)
            {
                HandleException(e, when);
            }   
        }

        public void Do(Func<Task<bool>> f, string when) => Do(() => Task.Run(f).Result, when);
        public void Do(Func<bool> f, string when)
        {
            try
            {
                if (IsFaulted) return;
                if (f()) return;
                IsFaulted = true;
            }
            catch (Exception ex)
            {
               HandleException(ex, when);
            }
        }

        private void HandleException(Exception ex, string when)
        {
            IsFaulted = true;
            CaughtException?.Invoke(ex, when);

        }
    }
}
