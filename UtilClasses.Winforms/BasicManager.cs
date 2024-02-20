using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class BasicManager : UserControl
    {
        private object _current;
        protected IBinder _binder;
        protected IBasicManagerCommandProvider _provider;

        public BasicManager()
        {
            InitializeComponent();
        }

        protected void SetImageList(ImageList il) => lstvItems.StateImageList = il;

        protected void SetProvider<T>(IBasicManagerCommandProvider<T> provider) => _provider = new BasicManagerCommandProvider<T>(provider);

        private void tsbSave_Click(object sender, EventArgs e)
        {
            _binder.SaveTo(_current);
            _provider.Save(_current);
        }

        private void tsbReload_Click(object sender, EventArgs e) => _binder.LoadFrom(_current);
        private void LstvItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            _current = lstvItems.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.Tag;
            if (null == _current) return;
            _binder.LoadFrom(_current);
        }

        protected Orientation Orientation { get => split.Orientation; set => split.Orientation = value; }

        private void BasicManager_Load(object sender, EventArgs e)
        {
            
        }
        private async Task BasicManager_LoadAsync()
        {
            foreach (var x in await _provider.GetAll())
            {
                var itm = new ListViewItem();
                itm.Tag = x;
                _binder.SetListViewItem(itm);
                lstvItems.Items.Add(itm);
            }
        }
    }
    public interface IBasicManagerCommandProvider
    {
        Task Save(object item);
        Task<List<object>> GetAll();
        Task Delete(object obj);
        Task<object> New();
    }

    public interface IBasicManagerCommandProvider<T>
    {
        Task Save(T item);
        Task<List<T>> GetAll();
        Task Delete(T item);
        Task<T> New();
    }

    public class BasicManagerCommandProvider<T> : IBasicManagerCommandProvider, IBasicManagerCommandProvider<T>
    {
        private readonly Func<T, Task> _save;
        private readonly Func<Task<List<T>>> _getAll;
        private readonly Func<T, Task> _delete;
        private readonly Func<Task<T>> _new;


        public BasicManagerCommandProvider(IBasicManagerCommandProvider<T> provider)
        {
            _save = provider.Save;
            _getAll = provider.GetAll;
            _delete = provider.Delete;
            _new = provider.New;
        }

        public BasicManagerCommandProvider(Func<T, Task> save, Func<Task<List<T>>> getAll, Func<T, Task> delete, Func<Task<T>> @new)
        {
            _save = save;
            _getAll = getAll;
            _delete = delete;
            _new = @new;
        }

        public Task Delete(object obj) => _delete((T)obj);

        public Task Delete(T item) => _delete(item);

        async Task<List<object>> IBasicManagerCommandProvider.GetAll() => (await _getAll()).Cast<object>().ToList();

        async Task<object> IBasicManagerCommandProvider.New() => await _new();

        public Task Save(object item) => _save((T)item);

        public Task Save(T item) => _save(item);

        public Task<List<T>> GetAll() => _getAll();

        public Task<T> New() => _new();
    }


    public static class BasicManagerExtensions
    {
        public static ListViewItem WithTag(this ListViewItem itm, object tag)
        {
            itm.Tag = tag;
            return itm;
        }
    }

}
