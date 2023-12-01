using System.Reflection;

namespace AoC2023;

public static class Program
{
    public static void Main(string[] args)
    {

        var dayNumber = -1;

        if(args.Length is 0)
        {
            dayNumber = DateTime.Now.Day;
        }
        else
        {
            if(!int.TryParse(args[0], out dayNumber))
            {
                Console.WriteLine("Not a number");
                return;
            }    
        }

        var type = Type.GetType($"AoC2023.Days.Day{dayNumber:D2}");
            
        if(type is null)
        {
            Console.WriteLine("Such day does not exist.");
            return;
        }

        dynamic instance = Activator.CreateInstance(type, []) 
        ?? throw new Exception($"Could not create an instance of type {type.Name}");

        var part1MethodInfo = type.GetMethod(nameof(IDay.Part1)) 
        ?? throw new Exception($"Could not get method info of Part1."); 

        var part2MethodInfo = type.GetMethod(nameof(IDay.Part2)) 
        ?? throw new Exception($"Could not get method info of Part2."); 

        object[] methodMiniArgs = [$"Inputs/Day{dayNumber:D2}-mini.txt"];
        object[] methodArgs = [$"Inputs/Day{dayNumber:D2}.txt"];

        var mini1Answer = part1MethodInfo.Invoke(instance, methodMiniArgs);

        var backupOut = Console.Out;

        Console.SetOut(TextWriter.Null);
        var part1Answer = part1MethodInfo.Invoke(instance, methodArgs);
        Console.SetOut(backupOut);


        var mini2Answer = part2MethodInfo.Invoke(instance, methodMiniArgs);

        Console.SetOut(TextWriter.Null);
        var part2Answer = part2MethodInfo.Invoke(instance, methodArgs);
        Console.SetOut(backupOut);

        Console.WriteLine($"Day {dayNumber:D2}");
        Console.WriteLine($"Mini 1: {mini1Answer}");
        Console.WriteLine($"Part 1: {part1Answer}");
        Console.WriteLine("");
        Console.WriteLine($"Mini 2: {mini2Answer}");
        Console.WriteLine($"Part 2: {part2Answer}");
    }
}