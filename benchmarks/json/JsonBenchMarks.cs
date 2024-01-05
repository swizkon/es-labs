using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;

namespace json;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class JsonBenchMarks
{
    private static readonly SerializeNewtonsoft _newtonsoft = new();
    private static readonly SerializeSystemText _systemText = new();
    private readonly Person _small;
    private readonly Organization _org;
    private readonly Department _dep;

    public JsonBenchMarks()
    {
        var fixture = new Fixture();

        _small = fixture.Create<Person>();

        _dep = fixture.Create<Department>();

        _org = fixture.Create<Organization>();
    }

    //[Benchmark(Baseline = true)]
    //public void SmallSerializeNewtonsoft()
    //{
    //    _newtonsoft.DoIt(_small);
    //}

    //[Benchmark]
    //public void SmallSerializeSystemText()
    //{
    //    _systemText.DoIt(_small);
    //}

    [Benchmark(Baseline = true)]
    public void MediumSerializeNewtonsoft()
    {
        _newtonsoft.DoIt(_dep);
    }

    [Benchmark]
    public void MediumSerializeSystemText()
    {
        _systemText.DoIt(_dep);
    }

    //[Benchmark]
    //public void BigSerializeNewtonsoft()
    //{
    //    _newtonsoft.DoIt(_org);
    //}

    //[Benchmark]
    //public void BigSerializeSystemText()
    //{
    //    _systemText.DoIt(_org);
    //}
}