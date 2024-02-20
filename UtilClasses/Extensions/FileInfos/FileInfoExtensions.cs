using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Extensions.FileInfos
{
    public static class FileInfoExtensions
    {
        public static bool IsInUse(this FileInfo fi)
        {
            FileStream? stream = null;
            try
            {
                stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                // - still being written to
                // - being processed by another thread
                // - does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static async Task<FileInfo> CopyToAsync(this FileInfo fi, string destFileName, bool overwrite) =>
            await Task.Run(() => fi.CopyTo(destFileName, overwrite));

        public static async Task DeleteAsync(this FileInfo fi) => await Task.Run(() => fi.Delete());

        public static async Task<T> WithErrHandler<T>(this Task<T> task, Func<Exception, T> errHandler)
            => await task.ContinueWith(t => t.IsFaulted ? errHandler(t.Exception?.InnerException) : t.Result);

        public static async Task<T> WithErrHandler<T>(this Task<T> task, Action<Exception> errHandler) where T:class
            => await task.WithErrHandler(e =>
            {
                errHandler(e);
                return null;
            });
        public static string ReadUtf8(this FileInfo fi)
        {
            var buff = new byte[fi.Length + 1];
            using(var fs = fi.OpenRead())
            {
                fs.Read(buff, 0, (int)fi.Length);
                return Encoding.UTF8.GetString(buff);
            }
        }
    }
}
