// See https://aka.ms/new-console-template for more information

using NAudio.Wave;

Console.WriteLine("Hello! Press any key to continue");

Console.ReadKey();

// Provide the path to your MP3 file
var mp3FilePath = "You_Suffer.mp3";

// Check if the file exists
if (!File.Exists(mp3FilePath))
{
    Console.WriteLine("The specified MP3 file does not exist.");
    return;
}

using var outputDevice = new WaveOutEvent();
using var mp3Reader = new Mp3FileReader(mp3FilePath);
// Connect the MP3 reader to the output device
outputDevice.Init(mp3Reader);

ConsoleKeyInfo k;

do
{
    Console.WriteLine("Init and Playing MP3 file...");
    mp3Reader.Position = 0;
    outputDevice.Play();
    k = Console.ReadKey();
} while (k.Key != ConsoleKey.Q);

Task.Delay(2000).GetAwaiter().GetResult();

// Stop and dispose of the output device
outputDevice.Stop();
