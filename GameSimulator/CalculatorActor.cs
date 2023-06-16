using Akka.Actor;

namespace GameSimulator;

public class CalculatorActor : ReceiveActor
{
    private int result;

    public CalculatorActor()
    {
        // Set the initial behavior
        Become(Initial);

        // Define the initial behavior
        void Initial()
        {
            Receive<AddMessage>(message =>
            {
                result += message.Number;
                Console.WriteLine($"Current result: {result}");
            });

            Receive<SubtractMessage>(message =>
            {
                result -= message.Number;
                Console.WriteLine($"Current result: {result}");
            });

            Receive<string>(message =>
            {
                if (message == "switch")
                {
                    Become(Addition);
                    Console.WriteLine("Switched to addition mode");
                }
            });
        }

        // Define the addition behavior
        void Addition()
        {
            Receive<AddMessage>(message =>
            {
                result += message.Number;
                Console.WriteLine($"Current result: {result}");
            });

            Receive<SubtractMessage>(message =>
            {
                result -= message.Number;
                Console.WriteLine($"Current result: {result}");
            });

            Receive<string>(message =>
            {
                if (message == "switch")
                {
                    Become(Subtraction);
                    Console.WriteLine("Switched to subtraction mode");
                }
            });
        }

        // Define the subtraction behavior
        void Subtraction()
        {
            Receive<AddMessage>(message =>
            {
                result += message.Number;
                Console.WriteLine($"Current result: {result}");
            });

            Receive<SubtractMessage>(message =>
            {
                result -= message.Number;
                Console.WriteLine($"SubtractMessage result: {result}");
            });

            Receive<string>(message =>
            {
                if (message == "switch")
                {
                    Become(Initial);
                    Console.WriteLine("Switched to initial mode");
                }
            });
        }
    }
}