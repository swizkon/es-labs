// See https://aka.ms/new-console-template for more information

using NAudio.Wave;

using StackExchange.Redis;

Console.WriteLine("Hello! Press any key to continue");

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

var connection = ConnectionMultiplexer.Connect("localhost:6379");

do
{
    Console.WriteLine("Press any key to play or Q to exit...");
    k = Console.ReadKey();
    mp3Reader.Position = 500;
    outputDevice.Play();
} while (k.Key != ConsoleKey.Q);

// Stop and dispose of the output device
outputDevice.Stop();
