using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swed64;

namespace MikoCheat
{
    public class ValidateOffsets
    {
        public static void Validate(Swed swed, IntPtr clientBase)
        {
            Console.WriteLine("Validating offsets...");
            bool criticalOffsetsValid = true; // Tracks validity of critical offsets
            bool localPlayerPawnValid = true; // Tracks dwLocalPlayerPawn validity separately

            try
            {
                // Validate dwLocalPlayerPawn (non-critical)
                IntPtr localPlayerPawn = swed.ReadPointer(clientBase, Offsets.dwLocalPlayerPawn);
                if (localPlayerPawn == IntPtr.Zero)
                {
                    Console.WriteLine($"[WARNING] Invalid offset: dwLocalPlayerPawn (0x{Offsets.dwLocalPlayerPawn:X})");
                    localPlayerPawnValid = false;
                }
                else
                {
                    Console.WriteLine($"[OK] dwLocalPlayerPawn: 0x{Offsets.dwLocalPlayerPawn:X}");
                }

                // Validate critical offsets
                IntPtr entityList = swed.ReadPointer(clientBase, Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                {
                    Console.WriteLine($"[ERROR] Invalid offset: dwEntityList (0x{Offsets.dwEntityList:X})");
                    criticalOffsetsValid = false;
                }
                else
                {
                    Console.WriteLine($"[OK] dwEntityList: 0x{Offsets.dwEntityList:X}");
                }

                float[] viewMatrix = swed.ReadMatrix(clientBase + Offsets.dwViewMatrix);
                if (viewMatrix == null || viewMatrix.Length < 16)
                {
                    Console.WriteLine($"[ERROR] Invalid offset: dwViewMatrix (0x{Offsets.dwViewMatrix:X})");
                    criticalOffsetsValid = false;
                }
                else
                {
                    Console.WriteLine($"[OK] dwViewMatrix: 0x{Offsets.dwViewMatrix:X}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Error validating offsets: {ex.Message}");
                criticalOffsetsValid = false;
            }

            // Handle results
            if (!criticalOffsetsValid)
            {
                // Critical offsets invalid; stop execution
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR] Critical offsets are invalid!");
                Console.ResetColor();

                Console.WriteLine("Please ensure you are in an active game (match) when validating offsets.");
                Console.WriteLine("Note: The offset for dwLocalPlayerPawn requires an active match to return valid data.");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nSteps to update offsets:");
                Console.WriteLine("1. Visit the CS2 Dumper repository to get the latest offsets:");
                Console.WriteLine("   https://github.com/a2x/cs2-dumper");
                Console.WriteLine("2. Replace the outdated offsets (Offsets.cs) in this project.");
                Console.WriteLine("3. Recompile the project from source code:");
                Console.WriteLine("   https://github.com/vorlie/MikoCheat");
                Console.ResetColor();

                Console.WriteLine("\nYou’ll need to manually locate and update offsets using tools like Cheat Engine if necessary.");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("\nCritical offsets are valid!");

                if (!localPlayerPawnValid)
                {
                    // Non-critical offset dwLocalPlayerPawn invalid
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[WARNING] dwLocalPlayerPawn is invalid. Some features may not work until you join a match.");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Continuing the program in 5 seconds...");
                    Console.ResetColor();

                    // Add delay to allow user to read the message
                    Task.Delay(5000).Wait();
                }

                
            }
        }
    }
}
