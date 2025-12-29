using System.Diagnostics;

namespace Bureau.Frontend.DevRunner
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            string workingDirectory = FindRepoRoot();
            string arguments = "-C client/apps/bureau-bundles dev";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c pnpm {arguments}",
                WorkingDirectory = workingDirectory,

                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = false
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        // keep stderr visible and clearly marked
                        Console.Error.WriteLine(e.Data);
                    }
                };

                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    TryKillProcessTree(process);
                    Environment.Exit(0);
                };

                if (!process.Start())
                {
                    Console.WriteLine("Failed to start pnpm dev.");
                    return 1;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Console.WriteLine("Vite dev server started (pnpm).");
                Console.WriteLine("Press Ctrl+C to stop.");
                Console.WriteLine();

                // WAIT ALWAYS, even if Vite fails fast
                process.WaitForExit();

                Console.WriteLine();
                Console.WriteLine($"Vite process exited with code {process.ExitCode}.");
                Console.WriteLine("Press ENTER to close this window.");
                Console.ReadLine();

                return process.ExitCode;
            }
        }

        private static void TryKillProcessTree(Process process)
        {
            try
            {
                if (process == null)
                {
                    return;
                }
                if (process.HasExited)
                {
                    return;
                }
                // On Linux/macOS, best-effort kill
                process.Kill(entireProcessTree: true);
                process.WaitForExit(2000);
            }
            catch
            {
                Console.WriteLine("Process wasn't killed");
            }
        }

        private static string FindRepoRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory.Parent != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "client")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Repo root not found.");
        }
    }
}
