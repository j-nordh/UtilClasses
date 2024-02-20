using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UtilClasses.Extensions.Objects;

namespace UtilClasses.Extensions.Processes
{
    public static class ProcessExtensions
    {
        public static ProcessStartInfo FileName(this ProcessStartInfo psi, string filename) =>
            psi.Do(() => psi.FileName = filename);
        public static ProcessStartInfo Arguments(this ProcessStartInfo psi, string args) =>
            psi.Do(() => psi.Arguments = args);

        public static ProcessStartInfo UseShellExec(this ProcessStartInfo psi, bool val) =>
            psi.Do(() => psi.UseShellExecute=val);

        public static ProcessStartInfo NoShellExec(this ProcessStartInfo psi) => psi.UseShellExec(false);

        public static ProcessStartInfo RedirectStdOut(this ProcessStartInfo psi, bool val = true) =>
            psi.Do(() => psi.RedirectStandardOutput = val);
    }
}
