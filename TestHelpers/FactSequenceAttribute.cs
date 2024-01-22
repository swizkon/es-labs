namespace TestHelpers;

[AttributeUsage(AttributeTargets.Method)]
public class FactSequenceAttribute : FactAttribute
{
    public FactSequenceAttribute(int sequence)
    {
        Sequence = sequence;
    }

    public int Sequence { get; private set; }
}