using System.Text.RegularExpressions;
using ClamAV.Models;
using CliWrap;
using Serilog;
using System.Text;

namespace ClamAV.Services;

public class ClamavScanService : IClamavScanService
{
    /// <summary>
    /// clamscan is a command-line tool that scans files and directories for malware. It uses the ClamAV virus database to detect malware.
    /// </summary>
    /// <param name="workingDirPath"></param>
    /// <param name="userParams"></param>
    /// <returns></returns>
    public async Task<ClamScanResult> ClamavscanAsync(string workingDirPath, string[] userParams)
    {
        try
        {
            var commandArgs = new string[] {
                "--no-summary"
            };

            var command = Cli.Wrap("/usr/bin/clamscan")
                .WithArguments(commandArgs.Union(userParams), false);

            var debug = command.ToString();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(900));

            Log.Information($"Command: {workingDirPath}{Path.DirectorySeparatorChar}{debug}");

            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var cliResult = await command
                .WithWorkingDirectory(workingDirPath)
                .WithValidation(CommandResultValidation.None) // ignore any process errors
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync(cts.Token);

            if (cliResult.ExitCode != 0)
            {
                Log.Error($"Process exist with code {cliResult.ExitCode}");
                Log.Error(stdErrBuffer.ToString());
            }

            var output = stdOutBuffer.ToString();
            Log.Information(output);

            // Command:
            //    /usr/bin/clamscan --no-summary ./
            // Output:
            //    /app/test: Eicar-Signature FOUND
            return ParseScanResult(output);

        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return new ClamScanResult()
            {
                ErrorCode = 1,
                ErrorMessage = e.Message
            };
        }
    }

    /// <summary>
    /// clamdscan is much faster because it uses the ClamAV daemon (clamd), which keeps the virus database in memory rather than reloading it for every scan.
    /// </summary>
    /// <param name="workingDirPath"></param>
    /// <param name="userParams"></param>
    /// <returns></returns>
    public async Task<ClamScanResult> ClamdscanAsync(string workingDirPath, string[] userParams)
    {
        try
        {
            var commandArgs = new string[] {
                "--no-summary"
            };

            var command = Cli.Wrap("/usr/bin/clamdscan")
                .WithArguments(commandArgs.Union(userParams), false);

            var debug = command.ToString();

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(900));

            Log.Information($"Command: {workingDirPath}{Path.DirectorySeparatorChar}{debug}");

            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var cliResult = await command
                .WithWorkingDirectory(workingDirPath)
                .WithValidation(CommandResultValidation.None) // ignore any process errors
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync(cts.Token);

            if (cliResult.ExitCode != 0)
            {
                Log.Error($"Process exist with code {cliResult.ExitCode}");
                Log.Error(stdErrBuffer.ToString());
            }

            var output = stdOutBuffer.ToString();
            Log.Information(output);

            // Command:
            //    /usr/bin/clamdscan --no-summary ./
            // Output:
            //    /app/test: Eicar-Signature FOUND
            return ParseScanResult(output);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return new ClamScanResult()
            {
                ErrorCode = 1,
                ErrorMessage = e.Message
            };
        }
    }

    private static ClamScanResult ParseScanResult(string output)
    {
        var result = new ClamScanResult()
        {
            ErrorCode = 0
        };


        if (string.IsNullOrWhiteSpace(output))
        {
            return result;
        }

        using (var reader = new StringReader(output))
        {
            string? line;
            var regex = new Regex(@"(.*): (.*) FOUND");

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("FOUND"))
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        result.Files.Add(new ScanFile
                        {
                            FilePath = match.Groups[1].Value,
                            Virus = match.Groups[2].Value,
                            Status = "INFECTED"
                        });
                    }
                }
            }
        }

        result.InfectedFiles = result.Files.Count;

        // We can't easily determine total scanned files without parsing summary
        // This is a rough approximation, assuming any exit code other than 0 means infection
        result.ScannedFiles = result.InfectedFiles > 0 ? result.InfectedFiles : 0;

        return result;
    }
}
