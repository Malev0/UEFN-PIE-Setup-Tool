using System;
using System.IO;


public class Program
{
    static void Main()
    {
        Console.WriteLine(@"
=========================================================================
|     ___ ___ ___   ___ ___ _____ _   _ ___   _____ ___   ___  _        |
|    | _ \_ _| __| / __| __|_   _| | | | _ \ |_   _/ _ \ / _ \| |       |
|    |  _/| || _|  \__ \ _|  | | | |_| |  _/   | || (_) | (_) | |__     |
|    |_| |___|___| |___/___| |_|  \___/|_|     |_| \___/ \___/|____|    |
|                                                                       |
=========================================================================");

        Console.WriteLine("\nAutomatically locate Fortnite installation (press enter) or manually set:");
        string? fnPath = Console.ReadLine();

        if (string.IsNullOrEmpty(fnPath))
        {
            Console.WriteLine("Locating..");
        }
        else
        {
            Console.WriteLine(fnPath);
        }
    }
}