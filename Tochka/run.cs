using System;
using System.Collections.Generic;
using Tochka;
using Tochka.Models;

class run
{
    static int Solve(List<string> lines)
    {
        // TODO: Реализация алгоритма
        Room.RoomSize = Parser.GetRoomSize(lines);
        var startMaze = new Maze(lines);
        var algorithm = new Algorithm(startMaze);
        var result = algorithm.Start();
        return result;
    }

    static void Main()
    {
        var lines = new List<string>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }

        int result = Solve(lines);
        Console.WriteLine(result);
    }
}