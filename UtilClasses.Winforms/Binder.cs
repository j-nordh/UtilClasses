using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilClasses.Extensions.Integers;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Winforms
{
    public interface IBinder
    {
        void LoadFrom(object obj);
        void SaveTo(object obj);
        void SetListViewItem(ListViewItem item);
    }
    public class Binder<T>:IBinder
    {
        private List<MemberAccessor<T>> _accessors;

        public Func<T, IEnumerable<string>> _setListViewItem;
        public Func<T, int> _lstvStateImageIndexSelector;

        public Binder( params (Control c, Expression<Func<object>> exp)[] defs)
        {
            _accessors = new List<MemberAccessor<T>>(defs.Select(d=> new MemberAccessor<T>(d.c,d.exp)));
        }
        public void LoadFrom(T obj)=>  _accessors.ForEach(a => a.Load(obj));

        public void LoadFrom(object obj) => LoadFrom((T)obj);

        public void SaveTo(T obj) => _accessors.ForEach(a => a.Save(obj));

        public void SaveTo(object obj) => SaveTo((T)obj);

        public void SetListViewItem(T obj, ListViewItem item)
        {
            item.SubItems.AddRange(_setListViewItem(obj).ToArray());
            if (item.ListView.StateImageList != null)
                item.StateImageIndex = _lstvStateImageIndexSelector(obj);
        }

        public Binder<T> List(Func<T, IEnumerable<string>> f)
        {
            _setListViewItem = f;
            return this;
        }

        public Binder<T> ListStateImage(Func<T, int> f)
        {
            _lstvStateImageIndexSelector = f;
            return this;
        }

        public Binder<T> Bind(Control c, Expression<Func<object>> exp)
        {
            _accessors.Add(new MemberAccessor<T>(c, exp));
            return this;
        }

        public void SetListViewItem(ListViewItem item) => SetListViewItem((T)item.Tag, item);

        private class MemberAccessor<TDto> 
        {
            public Control Control { get; }
            public Action<TDto> Load { get; }
            public  Action<TDto> Save { get; }
            public MemberAccessor(Control c, Expression<Func<object>> f)
            {
                Control = c;
                var lambda = f as LambdaExpression;
                if (lambda == null)
                    throw new ArgumentNullException("expression", @"That's not even a lambda expression!");

                MemberExpression me = null;

                switch (lambda.Body.NodeType)
                {
                    case ExpressionType.Convert:
                        me = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                        break;
                    case ExpressionType.MemberAccess:
                        me = lambda.Body as MemberExpression;
                        break;
                }
                if (me == null) throw new ArgumentException("Expressions must be on the form ()=>object.Property");
                Member m;
                switch (me.Member.MemberType)
                {
                    case MemberTypes.Field:
                        var field = me.Member.As<FieldInfo>();
                        m = new Member(field.FieldType, field.GetValue, field.SetValue);
                        break;
                    case MemberTypes.Property:
                        var prop = me.Member.As<PropertyInfo>();
                        m = new Member(prop.PropertyType, prop.GetValue, prop.SetValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                (var saver, var loader) = LoaderSaverFactory.Get(Control.GetType(), m.Type);
                Save = obj => m.Set(obj, saver(Control));
                Load = obj => loader(Control, m.Get(obj));
            }
            private class Member
            {
                public Member(Type type, Func<object, object> get, Action<object, object> set)
                {
                    Type = type;
                    Get = get;
                    Set = set;
                }

                public Type Type { get; }
                public Func<object, object> Get { get; }
                public Action<object, object> Set { get; }
            }
        }

    }
    static class LoaderSaverFactory
    {
        static Dictionary<(Type cType, Type vType), Action<Control, object>> _loaders;
        static Dictionary<(Type cType, Type vType), Func<Control, object>> _savers;
        static LoaderSaverFactory()
        {
            _loaders = new Dictionary<(Type cType, Type vType), Action<Control, object>>();
            _savers = new Dictionary<(Type cType, Type vType), Func<Control, object>>();
            Loader<CheckBox, bool>((cb, b) => cb.Checked =b);
            Loader<TextBox, string>((tb, s) => tb.Text = s);
            Loader<LabelledTextbox, string>((tb, s) => tb.Text = s);
            Loader<LabelledLabel, long>((ll, n) => ll.Text = n.ToString());
            Saver<CheckBox, bool>(cb => cb.Checked);
            Saver<LabelledLabel, long>(ll => ll.Text.AsLong());
            Saver<LabelledTextbox, string>(lt => lt.Text);
        }
        public static Func<Control, object> GetSaver(Type cType, Type vType) => _savers[(cType, vType)];
        
        public static Action<Control, object> GetLoader(Type cType, Type vType) => _loaders[(cType, vType)];
        public static (Func<Control, object> saver, Action<Control, object> loader) Get(Type cType, Type vType) => (GetSaver(cType, vType), GetLoader(cType, vType));

        static void Loader<TControl, TValue>(Action<TControl, TValue> a) where TControl : Control
        {
            _loaders[(typeof(TControl), typeof(TValue))] = (c, val) => a((TControl)c, (TValue)val);
        }
        static void Saver<TControl, TValue>(Func<TControl, TValue>f) where TControl:Control
        {
            _savers[(typeof(TControl), typeof(TValue))] = c => f((TControl)c);
        }
    }
}
