namespace UtilClasses.Tests.Json;

interface IPrimate
{
    PrimateType Type { get; }
}
class Chimp : IPrimate
{
    public PrimateType Type => PrimateType.Chimp;
}
class Gorilla : IPrimate
{
    public PrimateType Type => PrimateType.Gorilla;
}
enum PrimateType
{
    Chimp,
    Gorilla,
    C
}

