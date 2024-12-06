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
            bool allOffsetsValid = true;

            try
            {
                IntPtr localPlayerPawn = swed.ReadPointer(clientBase, Offsets.dwLocalPlayerPawn);
                if (localPlayerPawn == IntPtr.Zero)
                {
                    Console.WriteLine($"[ERROR] Invalid offset: dwLocalPlayerPawn (0x{Offsets.dwLocalPlayerPawn:X})");
                    allOffsetsValid = false;
                }
                else
                {
                    Console.WriteLine($"[OK] dwLocalPlayerPawn: 0x{Offsets.dwLocalPlayerPawn:X}");
                }

                IntPtr entityList = swed.ReadPointer(clientBase, Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                {
                    Console.WriteLine($"[ERROR] Invalid offset: dwEntityList (0x{Offsets.dwEntityList:X})");
                    allOffsetsValid = false;
                }
                else
                {
                    Console.WriteLine($"[OK] dwEntityList: 0x{Offsets.dwEntityList:X}");
                }

                float[] viewMatrix = swed.ReadMatrix(clientBase + Offsets.dwViewMatrix);
                if (viewMatrix == null || viewMatrix.Length < 16)
                {
                    Console.WriteLine($"[ERROR] Invalid offset: dwViewMatrix (0x{Offsets.dwViewMatrix:X})");
                    allOffsetsValid = false;
                }
                else
                {
                    Console.WriteLine($"[OK] dwViewMatrix: 0x{Offsets.dwViewMatrix:X}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Error validating offsets: {ex.Message}");
                allOffsetsValid = false;
            }

            if (!allOffsetsValid)
            {
                // Note: This block is reached if any of the offsets were invalid.
                // Ensure you're in a game (match) when validating offsets.
                // Offsets like dwLocalPlayerPawn or dwEntityList may return invalid
                // results if no game context is active.
                Console.WriteLine("\nNote: If you see invalid offsets, ensure you are in an active game (match).");
                Console.WriteLine("Offsets like dwLocalPlayerPawn or dwEntityList may return invalid results if no game context is active.");

                Console.WriteLine("\nIt seems some offsets are invalid.");
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine("\nTo update the offsets:");
                Console.WriteLine("1. Visit the CS2 Dumper repository to get the latest offsets:");
                Console.WriteLine("   https://github.com/a2x/cs2-dumper");

                Console.WriteLine("2. Replace the outdated offsets (Offsets.cs) in this project. (repo below)");
                Console.WriteLine("3. Recompile the project from the source code:");
                Console.WriteLine("   https://github.com/vorlie/MikoCheat");

                Console.ResetColor();
                Console.WriteLine("\nYou'll need to manually locate and update offsets using tools like Cheat Engine if necessary.");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0); // Exit the application
            }
            else
            {
                Console.WriteLine("\nAll offsets are valid! Continuing...");
            }
        }
    }
}
