using static System.Console;
using System.Runtime.CompilerServices;
using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ObjectOriented
{
    public interface IShape { int CalcArea(); }

    public class Rectangle : IShape { public int length; public int width; public int CalcArea() => width * length; }
    public class Circle : IShape { public int radius; public int CalcArea() => (int)((float)radius * 3.14 * 3.14); }

    public class Calculator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalcArea<T>(T s) where T : IShape
        {
            var area = s.CalcArea();
            return area;
        }
    }
}

namespace ObjectOrientedStruct
{
    public interface IShape { int CalcArea(); }

    public struct Rectangle : IShape { public int length; public int width; public int CalcArea() => width * length; }
    public struct Circle : IShape { public int radius; public int CalcArea() => (int)((float)radius * 3.14 * 3.14); }

    public class Calculator
    {
        IShape s = new Rectangle { length = 10, width = 10 };

        //[SharpLab.Runtime.JitGeneric(typeof(ObjectOrientedStruct.Rectangle))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalcArea<T>(ref T s) where T : IShape
        {
            var area = s.CalcArea();
            return area;
        }
    }
}

namespace FunctionalNice
{
    public interface IShape { }

    public struct Rectangle : IShape { public int length; public int width; }
    public struct Circle : IShape { public int radius; }

    public class Calculator
    {
        IShape s = new Rectangle { length = 10, width = 10 };

        //[SharpLab.Runtime.JitGeneric(typeof(FunctionalNice.Rectangle))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalcArea<Shape>(ref Shape s)
        {
            switch (s)
            {
                case Rectangle r:
                    return r.length * r.width;
                case Circle c:
                    return (int)((float)c.radius * 3.14 * 3.14);
                default:
                    return 0;
            }
        }
    }
}

namespace FunctionalUgly
{
    public interface IShape {}

    public struct Rectangle : IShape { public int length; public int width; }
    public struct Circle : IShape { public int radius; }

    public class Calculator
    {
        //[SharpLab.Runtime.JitGeneric(typeof(Functional.Rectangle))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalcArea<Shape>(ref Shape s)
        {
            if(typeof(Shape) ==  typeof(Rectangle))
            {
                ref Rectangle r = ref Unsafe.As<Shape, Rectangle>(ref s);
                return r.length * r.width;
            }

            if (typeof(Shape) == typeof(Circle))
            {
                ref Circle c = ref Unsafe.As<Shape, Circle>(ref s);
                return (int)((float)c.radius * 3.14 * 3.14);
            }

            return 0;
        }
    }
}

public  class MainClass
{
    ObjectOriented.Rectangle oc = new ObjectOriented.Rectangle { length = 10, width = 10 };
    ObjectOrientedStruct.Rectangle os = new ObjectOrientedStruct.Rectangle { length = 10, width = 10 };
    FunctionalNice.Rectangle fs = new FunctionalNice.Rectangle { length = 10, width = 10 };
    FunctionalUgly.Rectangle fu = new FunctionalUgly.Rectangle { length = 10, width = 10 };

    static int InnerIterationCount = 100000;

    [Benchmark]
    public int Test_OOClasss() {
        var sum = 0;
        for (int i = 0; i<InnerIterationCount; i++)
            sum += ObjectOriented.Calculator.CalcArea(oc);
        return sum;
    }

    [Benchmark]
    public int Test_OOStruct()
    {
        var sum = 0;
        for (int i = 0; i < InnerIterationCount; i++)
            sum += ObjectOrientedStruct.Calculator.CalcArea(ref os);
        return sum;
    }
    [Benchmark]
    public int Test_FuncNice()
    {
        var sum = 0;
        for (int i = 0; i < InnerIterationCount; i++)
            sum += FunctionalNice.Calculator.CalcArea(ref fs);
        return sum;
    }
    [Benchmark]
    public int Test_FuncUgly()
    {
        var sum = 0;
        for (int i = 0; i<InnerIterationCount; i++)
            sum += FunctionalUgly.Calculator.CalcArea(ref fu);
        return sum;
    }


public static void Main()
    {

        BenchmarkRunner.Run<MainClass>();

    }
}

