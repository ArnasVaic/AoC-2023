using System.Reflection;
using System.Runtime.InteropServices;

namespace AoC2023;

public static class Program
{

    private enum PartNumber
    {
        First = 1,
        Second = 2
    };

    private static int ToInt(PartNumber number) => number switch
    {
        PartNumber.First => 1,
        PartNumber.Second => 2,
        _ => throw new Exception("Part number unavailable")
    };

    private static void HandleDayPart(
        IDay day, 
        Type type, 
        int dayNumber, 
        PartNumber partNumber, 
        bool mini)
    {
        var number = ToInt(partNumber);
        var methodName = $"Part{number}";
        var methodInfo = type.GetMethod(methodName);

        if(methodInfo is null)
        {
            Console.WriteLine($"Could not get method {methodName} info from type {type}.");
            return;
        } 

        var miniText = mini ? "-mini" : string.Empty;
        var inputFileName = $"Inputs/Day{dayNumber:D2}-part{number}{miniText}.txt";

        miniText = mini ? "mini" : "full";
        try
        {
            object[] methodArg = [File.ReadAllText(inputFileName)];


            TextWriter backup = Console.Out;
            if(!mini)
            {
                Console.SetOut(TextWriter.Null);
            }

            var answer = methodInfo.Invoke(day, methodArg);

            Console.SetOut(backup);

            Console.WriteLine($"[{miniText}, Part {number}]: {answer}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[{miniText}, Part {number}]: {ex.Message}");
        }
    }

    private static int? GetDayNumber(string[] args)
    {
        if (args.Length is 0)
            return DateTime.Now.Day;

        if (int.TryParse(args[0], out var dayNumber))
            return dayNumber;
       
        Console.WriteLine("Not a number");
        return null; 
    }

    public static void Main(string[] args)
    {
        var dayNumber = GetDayNumber(args);

        if(!dayNumber.HasValue)
            return;

        var typeName = $"AoC2023.Days.Day{dayNumber:D2}.Day{dayNumber:D2}";
        var type = Type.GetType(typeName);
            
        if(type is null)
        {
            Console.WriteLine($"Type '{typeName}' does not exist.");
            return;
        }

        dynamic instance = Activator.CreateInstance(type, []);

        if(instance is null)
        {
            Console.WriteLine($"Could not created instance of type '{typeName}'.");
            return;
        }

        HandleDayPart(instance, type, dayNumber.Value, PartNumber.First, true);
        HandleDayPart(instance, type, dayNumber.Value, PartNumber.First, false);
        HandleDayPart(instance, type, dayNumber.Value, PartNumber.Second, true);
        HandleDayPart(instance, type, dayNumber.Value, PartNumber.Second, false);
    }
}