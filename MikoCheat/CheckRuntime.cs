using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace MikoCheat
{
    public class CheckRuntime
    {
        public static async Task EnsureDotNet8RuntimeAsync()
        {
            if (!IsDotNet8Installed())
            {
                Console.WriteLine("The .NET 8.0 Runtime is not installed.");
                Console.WriteLine("Would you like to download it? (y/n)");

                var response = Console.ReadLine();
                if (response?.ToLower() == "y")
                {
                    await OpenDownloadPageAsync();
                }
                else
                {
                    Console.WriteLine("The application cannot run without the .NET 8.0 Runtime. Exiting...");
                    Environment.Exit(1);
                }
            }
        }

        private static bool IsDotNet8Installed()
        {
            try
            {
                const string keyPath = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost";
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null)
                    {
                        return false; // Key doesn't exist
                    }

                    string? version = key.GetValue("Version") as string;
                    if (string.IsNullOrEmpty(version))
                    {
                        return false; // Version not found or empty
                    }

                    return version.StartsWith("8.") || version.StartsWith("9.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking .NET version: {ex.Message}");
                return false; // Assume not installed on error
            }
        }

        private static async Task OpenDownloadPageAsync()
        {
            try
            {
                const string downloadUrl = "https://download.visualstudio.microsoft.com/download/pr/27bcdd70-ce64-4049-ba24-2b14f9267729/d4a435e55182ce5424a7204c2cf2b3ea/windowsdesktop-runtime-8.0.11-win-x64.exe";
                string tempFilePath = Path.Combine(Path.GetTempPath(), "windowsdesktop-runtime-8.0.11-win-x64.exe");

                // Use HttpClient to download the installer
                using (var client = new HttpClient())
                {
                    Console.WriteLine("Downloading .NET 8.0 Runtime...");
                    var response = await client.GetAsync(downloadUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                        {
                            await response.Content.CopyToAsync(fs);
                        }

                        Console.WriteLine("Download completed.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to download the installer. HTTP Status Code: {response.StatusCode}");
                        return;
                    }
                }

                // Execute the installer silently
                Console.WriteLine("Installing .NET 8.0 Runtime silently...");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        Arguments = "/quiet /norestart",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start(); // Start the installation process
                process.WaitForExit(); // Wait for installation to complete

                if (process.ExitCode == 0)
                {
                    Console.WriteLine(".NET 8.0 Runtime installed successfully.");
                }
                else
                {
                    Console.WriteLine($"Installation failed with exit code {process.ExitCode}. Please try again manually.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download or install the .NET Runtime: {ex.Message}");
            }
        }
    }
}
