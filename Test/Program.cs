using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Compare("ThingDef", "PawnKindDef", true));
            Console.WriteLine("ThingDef".CompareTo("PawnKindDef"));
        }
    }
}
