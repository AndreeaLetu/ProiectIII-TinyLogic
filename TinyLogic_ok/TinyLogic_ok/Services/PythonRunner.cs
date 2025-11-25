using System.Diagnostics;
using System.Text;
using TinyLogic_ok.Services;

public class PythonRunner : IPythonRunner
{
    public string Run(string code)
    {
        var tempFile = Path.GetTempFileName() + ".py";

       
        File.WriteAllText(tempFile, code, Encoding.UTF8);

        var process = new Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.Arguments = $"\"{tempFile}\"";

     
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

      
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return !string.IsNullOrEmpty(error) ? error : output;
    }
}
