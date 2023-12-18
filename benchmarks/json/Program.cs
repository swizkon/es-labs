// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using json;

Console.WriteLine("Hello, World!");


BenchmarkRunner.Run<JsonBenchMarks>();