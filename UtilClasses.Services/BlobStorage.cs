//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using UtilClasses.Extensions.Dictionaries;
//using UtilClasses.Extensions.Enumerables;
//using UtilClasses.Extensions.Strings;
//namespace UtilClasses.Services
//{

//    public class GuidStorageNode
//    {
//        public string Name { get; }
//        public string Dir { get; }
//        private Dictionary<string, GuidStorageNode> _subDirs;
//        private Dictionary<Guid, string> _files;
//        private readonly GuidStorageNode _parent;
//        protected GuidStorageRoot _root;
//        protected TaskCompletionSource<int> _tcs;
//        public static int MaxFilesPerDir { get; set; }

//        public int FileCount { get; }
//        public GuidStorageNode(string path, GuidStorageNode parent, GuidStorageRoot root)
//        {
//            Name = Path.GetFileName(path);
//            Dir = path;
//            _parent = parent;
//            _root = root;
//            _files = new Dictionary<Guid, string>();
//            _subDirs = new Dictionary<string, GuidStorageNode>();
//            _tcs = new TaskCompletionSource<int>();
//            Task.Run(() =>
//            {
//                var dir = new DirectoryInfo(path);
//                foreach (var f in dir.GetFiles())
//                {
//                    if (!Guid.TryParse(f.Name.SubstringBefore("_"), out var id)) continue;
//                    _files[id] = f.Name;
//                }
//                foreach (var d in dir.GetDirectories())
//                {
//                    _subDirs[d.Name] = new GuidStorageNode(d.FullName, this, _root);
//                }
//                _tcs.SetResult(1);
//            });
//        }
//        protected virtual Task<string> GetFilePath(Guid id) => Task.FromResult(_files.TryGetValue(id, out var name) ? Path.Combine(Dir, name) : null);

//        protected async Task<Guid> Create(Guid id, byte[] blob, string filename)
//        {
//            await _tcs.Task;
//            if (_files.ContainsKey(id)) throw new ArgumentException($"There already exists a file with id:{id:N}");
//            if (_files.Count >= MaxFilesPerDir)
//            {
//                await Split();
//                return await _root.Create(id, blob, filename);
//            }
//            var name = $"{id:N}_{filename}";
//            var path = Path.Combine(Dir, name);
//            File.WriteAllBytes(path, blob);
//            _files[id] = name;
//            return id;
//        }

//        protected async Task<byte[]> Read(Guid id)
//        {
//            await _tcs.Task;
//            var name = _files.Maybe(id);
//            if (null == name) return null;
//            return await Task.Run(()=>File.ReadAllBytes(Path.Combine(Dir, name)));   
//        }

//        public async Task Delete(Guid id)
//        {
//            await _tcs.Task;
//            File.Delete(_files[id]);
//            _files.Remove(id);
            
//        }
//        public async Task Split()
//        {
//            for (int i = 0; i <= 255; i++)
//            {
//                var name = i.ToString("x2");
//                var path = Path.Combine(Dir, name);
//                Directory.CreateDirectory(path);
//                _subDirs[name] = new GuidStorageNode(path, this, _root);
//            }

//            foreach (var f in _files)
//            {
//                var newDir = await _root.GetDir(f.Key);
//                newDir.Assume(Path.Combine(Dir, f.Value));
//            }
//            _files.Clear();
//        }
//        private void Assume(string path)
//        {
//            var name = Path.GetFileName(path);
//            if (!Guid.TryParse(name.SubstringBefore("_"), out var id)) return;
//            if (_files.ContainsKey(id)) throw new ArgumentException($"Could not assume {path}, file with that ID already exists.");
//            File.Move(path, Path.Combine(Dir, name));
//            _files[id] = name;
//        }
//    }
//    public class GuidStorageRoot : GuidStorageNode
//    {
//        public GuidStorageRoot(string path) : base(path, null, null)
//        {
//            _root = this;
//        }

//        private Queue<string> GetQueue(Guid id) => Regex.Matches(id.ToString("N"), "..").Cast<Match>().Select(m => m.Value).ToQueue();
//        private Task<T> WithQueue<T>(Guid id, Func<GuidStorageNode, Guid, T> f) => Recurse(GetQueue(id),n=>f(n,id));
//        private Task<T> WithQueue<T>(Guid id, Func<Queue<string>, Task<T>> f) => f(GetQueue(id));
//        private Task WithQueue(Guid id, Func<Queue<string>, Task> f) => f(GetQueue(id));
//        private Task WithQueue(Guid id, Func<Queue<string>, Guid, Task> f) => f(GetQueue(id), id);
//        protected override async Task<string> GetFilePath(Guid id) => WithQueue(id, GetFilePath);
//        private Task<string> GetFilePath(GuidStorageNode n, Guid id) => n.GetFilePath(id);
//        public Task<Guid> Create(Guid id, byte[] blob, string filename) => WithQueue(id, q => Create(q, id, blob, filename));
//        public Task<Guid> Create(byte[] blob, string filename) => Create(Guid.NewGuid(), blob, filename);
//        public byte[] Read(Guid id) => WithQueue(Read);
//        public async Task Update(Guid id, byte[] blob)
//        {
//            var path = await GetFilePath(id);
//            if (null == path) throw new FileNotFoundException($"Could not update file with id: {id:N} since it does not exist");
//            File.Delete(path);
//            File.WriteAllBytes(path, blob);
//        }

//        public Task Delete(Guid id) => WithQueue(id, Delete);
//        public Task<GuidStorageNode> GetDir(Guid id) => WithQueue(id, GetDir);
//        public bool Exists(Guid id) => GetFilePath(id) != null;
//        protected async Task<T> Recurse<T>(Queue<string> q, Func<GuidStorageNode, T> f)
//        {
//            await _tcs.Task;
//            if (_subDirs.Any())
//            {
//                return await _subDirs[q.Dequeue()].Recurse(q, f);
//            }
//            return f(this);
//        }
//    }
//}
