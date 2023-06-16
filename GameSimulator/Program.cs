
// using Akka.Actor;

// Define the messages
// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using GameSimulator;

Console.WriteLine("Hello, World!");

// Create the actor system and actor
var system = ActorSystem.Create("CalculatorSystem");
var calculatorActor = system.ActorOf<CalculatorActor>("CalculatorActor");

var gateTick = TimeSpan.FromSeconds(0);

while (gateTick < TimeSpan.FromMinutes(15))
{
    gateTick = gateTick.Add(TimeSpan.FromSeconds(1));
    Console.WriteLine(gateTick.ToString());
    calculatorActor.Tell(new AddMessage(1));
}

/*
// Send messages to the actor
calculatorActor.Tell(new AddMessage(5));
calculatorActor.Tell(new SubtractMessage(3));
calculatorActor.Tell(new AddMessage(10));
calculatorActor.Tell(new SubtractMessage(7));

// Switch the behavior
calculatorActor.Tell("switch");

// Send messages to the actor with the new behavior
calculatorActor.Tell(new AddMessage(8));
calculatorActor.Tell(new SubtractMessage(4));

// Switch back to the initial behavior
calculatorActor.Tell("switch");

// Send messages to the actor with the initial behavior
calculatorActor.Tell(new AddMessage(15));
calculatorActor.Tell(new SubtractMessage(5));
*/

// Thread.Sleep(5000);

Console.ReadKey();

// Terminate the actor system
system.Terminate().Wait();

