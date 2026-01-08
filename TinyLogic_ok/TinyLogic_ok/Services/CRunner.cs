using System.Diagnostics;
using System.Text;

namespace TinyLogic_ok.Services
{
    public class CRunner : ICRunner
    {
        public string Run(string code)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "c_runner_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            string sourceFile = Path.Combine(tempDir, "main.c");
            string exeFile = Path.Combine(tempDir, "program");

            File.WriteAllText(sourceFile, code, Encoding.UTF8);

            // 1️⃣ COMPILARE
            var compile = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gcc",
                    Arguments = $"\"{sourceFile}\" -o \"{exeFile}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            compile.Start();
            string compileErrors = compile.StandardError.ReadToEnd();
            compile.WaitForExit();

            if (!string.IsNullOrWhiteSpace(compileErrors))
            {
                return compileErrors.Trim();
            }

     
            var run = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exeFile,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            run.Start();
            string output = run.StandardOutput.ReadToEnd();
            string runtimeError = run.StandardError.ReadToEnd();
            run.WaitForExit();

            return !string.IsNullOrWhiteSpace(runtimeError)
                ? runtimeError.Trim()
                : output.Trim();
        }
    }
}
