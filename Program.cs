using System;
using System.Security;

namespace WebPublisher;

public class Program
{
    private const string DEFAULT_CONFIGURATION = "DEV";
    private const string DEFAULT_USER = "DLeinweber";
    private const string DEFAULT_PROJECT_PATH = @"C:\DEV\Web\Web.csproj";

    private static int Main(string[] args)
    {
        try
        {
            // Parse command‐line arguments (Configuration, User, ProjectDir). Password is prompted securely
            var configuration = DEFAULT_CONFIGURATION;
            var user = DEFAULT_USER;
            var projectPath = DEFAULT_PROJECT_PATH;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (
                    arg.Equals("--Configuration", StringComparison.OrdinalIgnoreCase)
                    || arg.Equals("-c", StringComparison.OrdinalIgnoreCase)
                )
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("ERROR: Missing value for -Configuration.");
                        return 1;
                    }

                    configuration = args[++i];
                    continue;
                }

                if (
                    arg.Equals("--User", StringComparison.OrdinalIgnoreCase)
                    || arg.Equals("-u", StringComparison.OrdinalIgnoreCase)
                )
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("ERROR: Missing value for -User.");
                        return 1;
                    }

                    user = args[++i];
                    continue;
                }

                if (
                    arg.Equals("--ProjectDir", StringComparison.OrdinalIgnoreCase)
                    || arg.Equals("-p", StringComparison.OrdinalIgnoreCase)
                )
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("ERROR: Missing value for -ProjectDir.");
                        return 1;
                    }

                    projectPath = args[++i];
                    continue;
                }

                ShowUsageAndExit();
            }

            // Prompt for password securely
            var password = ReadPassword($"Enter password for user '{user}': ");
            if (password == null || password.Length == 0)
            {
                Console.Error.WriteLine("ERROR: Password cannot be empty.");
                return 1;
            }

            // Delegate to publisher
            var publisher = new Publisher();
            publisher.Publish(configuration, user, password, projectPath);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }

    private static SecureString ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var securePassword = new SecureString();
        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (securePassword.Length > 0)
                {
                    securePassword.RemoveAt(securePassword.Length - 1);
                    Console.Write("\b \b");
                }

                continue;
            }

            securePassword.AppendChar(keyInfo.KeyChar);
            Console.Write('*');
        }

        securePassword.MakeReadOnly();
        return securePassword;
    }

    private static void ShowUsageAndExit()
    {
        var usage = $"""
            Usage:
                Publish-Web.exe [--Configuration | -c <DEV|PROD>] [--User | -u <username>] [--ProjectDir | -p <path>]

                --Configuration | -c  : Optional; default is "DEV". Valid values: DEV or PROD (case‐insensitive).
                --User          | -u  : Optional; default is "DLeinweber".
                --ProjectDir    | -p  : Optional; default is "{DEFAULT_PROJECT_PATH}".

                At runtime, you will be prompted to enter the password securely.
            """;

        Console.WriteLine(usage);
        Environment.Exit(1);
    }
}
