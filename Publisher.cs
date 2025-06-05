using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace WebPublisher;

internal sealed class Publisher
{
    private const string VALID_DEV = "Dev";
    private const string VALID_PROD = "Prod";

    public void Publish(
        string configurationInput,
        string userName,
        SecureString password,
        string projectFilePath
    )
    {
        if (string.IsNullOrWhiteSpace(configurationInput))
        {
            throw new ArgumentException(
                "Configuration must not be empty.",
                nameof(configurationInput)
            );
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name must not be empty.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(projectFilePath))
        {
            throw new ArgumentException(
                "Project file path must not be empty.",
                nameof(projectFilePath)
            );
        }

        // Normalize and validate configuration
        var normalizedConfig = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
            configurationInput.Trim().ToLowerInvariant()
        );

        if (normalizedConfig != VALID_DEV && normalizedConfig != VALID_PROD)
        {
            throw new InvalidOperationException(
                "Invalid Configuration. Valid options: DEV or PROD."
            );
        }

        // Determine build configuration
        var buildConfiguration = "Release";

        if (normalizedConfig == VALID_DEV)
        {
            buildConfiguration = "Debug";
        }

        // Decrypt SecureString to plain text (in memory)
        var decryptedPassword = ConvertToUnsecureString(password);

        // Build the 'dotnet publish' arguments
        var publishArguments = BuildPublishArguments(
            projectFilePath,
            buildConfiguration,
            normalizedConfig,
            userName,
            decryptedPassword
        );

        // Invoke 'dotnet publish'
        ExecuteDotNetPublish(publishArguments);
    }

    private static string ConvertToUnsecureString(SecureString secure)
    {
        if (secure == null)
        {
            throw new ArgumentNullException(nameof(secure));
        }

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToBSTR(secure);
            return Marshal.PtrToStringAuto(ptr) ?? string.Empty;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }
    }

    private static string BuildPublishArguments(
        string projectPath,
        string buildConfig,
        string publishProfile,
        string userName,
        string userPassword
    )
    {
        userPassword = userPassword.Replace(";", "%3B");
        return $"publish \"{projectPath}\" -c {buildConfig} /p:PublishProfile={publishProfile} /p:UserName={userName} /p:Password={userPassword} /p:AllowUntrustedCertificate=true";
    }

    private static void ExecuteDotNetPublish(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            throw new ArgumentException("Publish arguments cannot be empty.", nameof(arguments));
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.Error.WriteLine(e.Data);
                }
            };

            var started = process.Start();
            if (started == false)
            {
                throw new InvalidOperationException("Failed to start 'dotnet publish' process.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"'dotnet publish' exited with code {process.ExitCode}."
                );
            }
        }
    }
}
