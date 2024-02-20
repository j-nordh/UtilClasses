using System.Collections.Generic;
using System.Data;

namespace UtilClasses.Db
{
    public class DbCall<T> : DbCall, IDbCall<T> where T:class  
    {
        public T NullValue { get; }
        public bool ReturnsEnumerable { get; set; }

        public DbCall(IStoredProcedure proc, T nullValue) : this(proc.Name, nullValue)
        {}

        public DbCall(IStoredProcedure proc, T nullValue, params ParameterHelper.Parameter[] ps) : this(proc, nullValue)
        {
            ParameterHelper.AddRange(ps);
        } 
        
        public DbCall(IStoredProcedure proc, params ParameterHelper.Parameter[] ps) : this(proc, null, ps)
        { }

        public DbCall(IStoredProcedure proc): this(proc, null, new ParameterHelper.Parameter[] {}) {}  

        public DbCall(string procedure, T nullValue) : base(procedure)
        {
            NullValue = nullValue;   
        }
    }

    public class DbCall:IDbCall
    {
        public string Procedure { get; }
        public ParameterHelper ParameterHelper { get;}

        public DbCall(string procedure)
        {
            Procedure = procedure;
            ParameterHelper=new ParameterHelper();
        }

        public DbCall(IStoredProcedure proc, params ParameterHelper.Parameter[] ps) : this(proc.Name)
        {
            ParameterHelper.AddRange(ps);
        }

    }

    public class ValDbCall<T>:DbCall, IValDbCall<T> where T:struct 
    {
        public ValDbCall(string procedure) : base(procedure)
        {
        }

        public ValDbCall(IStoredProcedure proc, params ParameterHelper.Parameter[] ps) : base(proc, ps)
        {
        }
    }

    public class JsonDbCall : DbCall<string>, IJsonDbCall
    {
        public JsonDbCall(IStoredProcedure proc) : base(proc)
        {
        }

        public JsonDbCall(string procedure, string nullValue) : base(procedure, nullValue)
        {
        }
    }

    public class JsonDbCall<T> : DbCall<T>, IJsonDbCall<T> where T : class
    {
        public JsonDbCall(IStoredProcedure proc) : base(proc)
        {
        }

        public JsonDbCall(string procedure, T nullValue) : base(procedure, nullValue)
        {
        }
    }    

    public static class DbCallExtensions
    {
        public static T Add<T>(this T call, ParameterHelper.Parameter p) where T:DbCall
        {
            call.ParameterHelper.Add(p);
            return call;
        }
    }
}
