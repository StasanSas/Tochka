namespace Tochka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Перенесем все статические классы в начало файла

public static class Infrastructure
{
    public static string ChangeString(this string str, char c, int indexChange)
    {
        return str.Substring(0, indexChange) + c + str.Substring(indexChange + 1);
    }
}

public static class Chars
{
    private static readonly IReadOnlyDictionary<char, int> CostMap = new Dictionary<char, int>
    {
        ['A'] = 1,
        ['B'] = 10,
        ['C'] = 100,
        ['D'] = 1000
    };

    private static readonly IReadOnlyDictionary<char, int> RoomIndexMap = new Dictionary<char, int>
    {
        ['A'] = 0,
        ['B'] = 1,
        ['C'] = 2,
        ['D'] = 3
    };

    private static readonly IReadOnlyDictionary<int, char> DoorIndexToCharMap = new Dictionary<int, char>
    {
        [2] = 'A',
        [4] = 'B',
        [6] = 'C',
        [8] = 'D'
    };

    public static readonly IReadOnlySet<char> ValidChars = new HashSet<char> { 'A', 'B', 'C', 'D' };
    public static readonly IReadOnlySet<int> ValidDoorIndexes = new HashSet<int> { 2, 4, 6, 8 };

    public static int GetCost(this char c)
    {
        if (!ValidChars.Contains(c)) throw new ArgumentException();
        return CostMap[c];
    }

    public static int GetIndexRoom(this char c)
    {
        if (!ValidChars.Contains(c)) throw new ArgumentException();
        return RoomIndexMap[c];
    }

    public static char GetCharByIndexDoorForRoom(int index)
    {
        if (!ValidDoorIndexes.Contains(index)) throw new ArgumentException();
        return DoorIndexToCharMap[index];
    }
}

public static class StringExtensions
{
    public static bool HasOnlyCharFromIndex(this string str, int startIndex, char expected)
    {
        for (int i = startIndex; i < str.Length; i++)
        {
            if (str[i] != expected) return false;
        }
        return true;
    }
}

public static class MazeOperations
{
    public static Maze GetMazeAfterStepBetweenHallwayAndRoom(
        this Maze maze, int indexRoom, int indexInRoom, int indexInHallway)
    {
        var currRoom = maze.Rooms[indexRoom];
        var lastCharFromRoom = currRoom.RoomString[indexInRoom];
        var lastCharFromHall = maze.Hallway.HallString[indexInHallway];
        var newRoom = currRoom.GetChangedRoom(lastCharFromHall, indexInRoom);
        var newHall = maze.Hallway.GetChangedHall(lastCharFromRoom, indexInHallway);
        var newArrayRooms = (Room[])maze.Rooms.Clone();
        newArrayRooms[indexRoom] = newRoom;
        return new Maze(newArrayRooms, newHall);
    }

    public static Maze GetMazeAfterStepInHallway(
        this Maze maze, int indexInHall1, int indexInHall2)
    {
        var lastCharFromHall1 = maze.Hallway.HallString[indexInHall1];
        var lastCharFromHall2 = maze.Hallway.HallString[indexInHall2];
        var tempHall = maze.Hallway.GetChangedHall(lastCharFromHall2, indexInHall1);
        var newHall = tempHall.GetChangedHall(lastCharFromHall1, indexInHall2);
        return new Maze(maze.Rooms, newHall);
    }

    public static Maze GetMazeAfterStepBetweenRoomAndRoom(
        this Maze maze, int indexRoom1, int indexInRoom1, int indexRoom2, int indexInRoom2)
    {
        var currRoom1 = maze.Rooms[indexRoom1];
        var currRoom2 = maze.Rooms[indexRoom2];
        var lastCharFromRoom1 = currRoom1.RoomString[indexInRoom1];
        var lastCharFromRoom2 = currRoom2.RoomString[indexInRoom2];
        var newRoom1 = currRoom1.GetChangedRoom(lastCharFromRoom2, indexInRoom1);
        var newRoom2 = currRoom2.GetChangedRoom(lastCharFromRoom1, indexInRoom2);
        var newArrayRooms = (Room[])maze.Rooms.Clone();
        newArrayRooms[indexRoom1] = newRoom1;
        newArrayRooms[indexRoom2] = newRoom2;
        return new Maze(newArrayRooms, maze.Hallway);
    }

    public static bool CanGetStepInHall(
        this Maze maze, int indexInHall1, int indexInHall2)
    {
        var start = Math.Min(indexInHall1, indexInHall2);
        var end = Math.Max(indexInHall1, indexInHall2);
        for (int i = start + 1; i < end; i++)
        {
            if (maze.Hallway.HallString[i] != '.') return false;
        }
        return true;
    }

    public static IEnumerable<DataAboutChar> GetDataLettersWhichCanMoveInHall(this Maze maze)
    {
        for (var i = 0; i < maze.Hallway.HallString.Length; i++)
        {
            var currChar = maze.Hallway.HallString[i];
            if (currChar == '.') continue;
            var haveSpaceInLeft = i != 0 && maze.Hallway.HallString[i - 1] == '.';
            var haveSpaceInRight = i != (maze.Hallway.HallString.Length - 1) && maze.Hallway.HallString[i + 1] == '.';
            if (haveSpaceInLeft || haveSpaceInRight)
                yield return new DataAboutChar(i, currChar);
        }
    }

    public static DataAboutChar? GetDataLetterWhichFirstInRoom(this Maze maze, int indexRoom)
    {
        var currRoom = maze.Rooms[indexRoom];
        for (var i = 0; i < currRoom.RoomString.Length; i++)
        {
            var currChar = currRoom.RoomString[i];
            if (currChar != '.') return new DataAboutChar(i, currChar);
        }
        return null;
    }

    public static int? GetIndexWhereCanMoveLetterInRoom(
        this Maze maze, int indexRoom, char mustBeThisChar)
    {
        var currRoom = maze.Rooms[indexRoom];
        var haveSpace = false;
        for (var i = 0; i < currRoom.RoomString.Length; i++)
        {
            var currChar = currRoom.RoomString[i];
            if (currChar == '.')
            {
                haveSpace = true;
                continue;
            }

            // если появилась буква, то все буквы после неё должны быть одинаковы и соответствовать комноте
            // наче комната не подходит
            var haveOnlyOneChar = currRoom.RoomString.HasOnlyCharFromIndex(i, mustBeThisChar);
            var roomIsForThisChar = mustBeThisChar.GetIndexRoom() == indexRoom;
            if (haveSpace && haveOnlyOneChar && roomIsForThisChar)
                return i - 1;
            else
                return null;
        }

        // если не было никаких букв и было свободное место
        if (haveSpace)
            return currRoom.RoomString.Length - 1;
        else
            return null;
    }

    public static IEnumerable<int> GetIndexesWhereCanMoveLetterInHall(this Maze maze)
    {
        var hall = maze.Hallway.HallString;
        for (int i = 0; i < hall.Length; i++)
        {
            if (hall[i] == '.' && !Maze.IndexDoors.Contains(i))
                yield return i;
        }
    }
}

public static class Parser
{
    public static Hallway GetHallWay(List<string> input)
    {
        var hall = input[1];
        return new Hallway(hall.Substring(1, hall.Length - 2));
    }

    public static int GetRoomSize(List<string> input)
    {
        return input.Count - 3;
    }

    public static List<Room> GetRooms(List<string> input)
    {
        var result = new List<Room>();
        for (var indexRoom = 3; indexRoom < input[0].Length - 3; indexRoom += 2)
        {
            var roomString = new StringBuilder();
            var indexWithoutWall = indexRoom - 1;
            for (int row = 2; row < input.Count - 1; row++)
            {
                roomString.Append(input[row][indexRoom]);
            }
            result.Add(new Room(indexWithoutWall, roomString.ToString()));
        }
        return result;
    }
}

public record DataAboutChar(int index, char c);

public class Hallway
{
    public readonly string HallString;

    public Hallway(string hallString)
    {
        HallString = hallString;
    }

    public Hallway GetChangedHall(char newChar, int index)
    {
        var newHallString = HallString.ChangeString(newChar, index);
        return new Hallway(newHallString);
    }

    public override bool Equals(object obj)
    {
        if (obj is not Hallway other) return false;
        return string.Equals(HallString, other.HallString);
    }

    public override int GetHashCode()
    {
        return HallString.GetHashCode();
    }
}

public class Maze
{
    public readonly Hallway Hallway;
    public readonly Room[] Rooms;
    public static readonly HashSet<int> IndexDoors = new HashSet<int> { 2, 4, 6, 8 };

    public Maze(List<string> input)
    {
        Hallway = Parser.GetHallWay(input);
        Rooms = Parser.GetRooms(input).ToArray();
    }

    public Maze(Room[] rooms, Hallway hallway)
    {
        Rooms = rooms;
        Hallway = hallway;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Maze other) return false;
        return Equals(Hallway, other.Hallway) && Rooms.SequenceEqual(other.Rooms);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Hallway.GetHashCode();
            foreach (var room in Rooms)
            {
                hash = hash * 23 + room.GetHashCode();
            }
            return hash;
        }
    }
}

public class Room
{
    public static int RoomSize { get; set; }
    public readonly int IndexDoor;
    public readonly string RoomString;

    public Room(int indexDoor, string roomString)
    {
        if (RoomSize == 0) throw new ArgumentException();
        IndexDoor = indexDoor;
        RoomString = roomString;
    }

    public Room GetChangedRoom(char newChar, int index)
    {
        var newRoomString = RoomString.ChangeString(newChar, index);
        return new Room(IndexDoor, newRoomString);
    }

    public override bool Equals(object obj)
    {
        if (obj is not Room other) return false;
        return IndexDoor == other.IndexDoor && string.Equals(RoomString, other.RoomString);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + IndexDoor.GetHashCode();
            hash = hash * 23 + RoomString.GetHashCode();
            return hash;
        }
    }

    public Room Clone()
    {
        return new Room(IndexDoor, RoomString);
    }
}

public class Algorithm
{
    public Maze StartMaze;
    public Maze EndMaze;

    public Algorithm(Maze startMaze)
    {
        StartMaze = startMaze;
        EndMaze = GetEndMaze();
    }

    public Maze GetEndMaze()
    {
        var hallWay = new Hallway(string.Concat(Enumerable.Repeat(".", 11)));
        var rooms = new List<Room>();
        for (var i = 2; i <= 8; i += 2)
        {
            var charForRepeat = Chars.GetCharByIndexDoorForRoom(i);
            var room = new Room(i, string.Concat(Enumerable.Repeat(charForRepeat, Room.RoomSize)));
            rooms.Add(room);
        }
        return new Maze(rooms.ToArray(), hallWay);
    }

    public int Start()
    {
        var open = new PriorityQueue<Maze, int>();
        var dictionaryCost = new Dictionary<Maze, int>();
        var cameFrom = new Dictionary<Maze, Tuple<Maze, int>?>(); // Для восстановления пути

        dictionaryCost[StartMaze] = 0;
        var h = GetHeuristic(StartMaze);
        open.Enqueue(StartMaze, h);
        cameFrom[StartMaze] = null;

        while (open.TryDequeue(out var currMaze, out var currF))
        {
            var currCost = dictionaryCost[currMaze];

            if (currMaze.Equals(EndMaze))
            {
                return currCost;
            }

            foreach (var (nextMaze, moveCost) in GetNextSteps(currMaze))
            {
                var nextCost = currCost + moveCost;
                if (!dictionaryCost.TryGetValue(nextMaze, out var oldCost) || nextCost < oldCost)
                {
                    dictionaryCost[nextMaze] = nextCost;
                    cameFrom[nextMaze] = new Tuple<Maze, int>(currMaze, currCost);
                    var valueForHeap = nextCost + GetHeuristic(nextMaze);
                    open.Enqueue(nextMaze, valueForHeap);
                }
            }
        }
        throw new Exception();
    }

    public int GetHeuristic(Maze current)
    {
        var total = 0;

        // Проверяем буквы в коридоре
        for (var i = 0; i < current.Hallway.HallString.Length; i++)
        {
            var c = current.Hallway.HallString[i];
            if (!char.IsLetter(c)) continue;

            var targetDoor = c.GetIndexRoom() * 2 + 2;
            var steps = Math.Abs(targetDoor - i) + 1;
            total += steps * c.GetCost();
        }

        // Проверяем буквы в комнатах
        for (var i = 0; i < current.Rooms.Length; i++)
        {
            var room = current.Rooms[i];
            for (var j = 0; j < room.RoomString.Length; j++)
            {
                var c = room.RoomString[j];
                if (!char.IsLetter(c)) continue;

                var targetRoom = c.GetIndexRoom();
                if (targetRoom == i)
                {
                    bool belowWrong = room.RoomString.Skip(j + 1).Any(x => x != c);
                    if (!belowWrong) continue;
                }

                var doorFrom = room.IndexDoor;
                var doorTo = current.Rooms[targetRoom].IndexDoor;
                var steps = j + 1 + Math.Abs(doorFrom - doorTo) + 1;
                total += steps * c.GetCost();
            }
        }

        return total;
    }

    public IEnumerable<(Maze maze, int cost)> GetNextSteps(Maze maze)
    {
        var results = new List<(Maze, int)>();

        for (int i = 0; i < maze.Rooms.Length; i++)
            results.AddRange(TryGetStepFromRoomToHall(maze, i));

        results.AddRange(TryGetStepInHall(maze));

        for (int i = 0; i < maze.Rooms.Length; i++)
            results.AddRange(TryGetStepFromHallToRoom(maze, i));

        return results;
    }

    public IEnumerable<(Maze maze, int cost)> TryGetStepFromRoomToHall(Maze maze, int indexRoom)
    {
        var dataAboutLetterForMove = maze.GetDataLetterWhichFirstInRoom(indexRoom);
        if (dataAboutLetterForMove == null) yield break;

        var placesForMaybeMove = maze.GetIndexesWhereCanMoveLetterInHall();
        foreach (var targetIndex in placesForMaybeMove)
        {
            if (maze.CanGetStepInHall(maze.Rooms[indexRoom].IndexDoor, targetIndex))
            {
                var newMaze = maze.GetMazeAfterStepBetweenHallwayAndRoom(indexRoom, dataAboutLetterForMove.index, targetIndex);
                var stepsInHall = Math.Abs(targetIndex - maze.Rooms[indexRoom].IndexDoor);
                var stepsInRoom = dataAboutLetterForMove.index + 1;
                var cost = dataAboutLetterForMove.c.GetCost() * (stepsInRoom + stepsInHall);
                yield return (newMaze, cost);
            }
        }
    }

    public IEnumerable<(Maze maze, int cost)> TryGetStepInHall(Maze maze)
    {
        var dataAboutLetterForMove = maze.GetDataLettersWhichCanMoveInHall();
        foreach (var start in dataAboutLetterForMove)
        {
            var placesForMaybeMove = maze.GetIndexesWhereCanMoveLetterInHall();
            foreach (var targetIndex in placesForMaybeMove)
            {
                if (maze.CanGetStepInHall(start.index, targetIndex))
                {
                    var newMaze = maze.GetMazeAfterStepInHallway(start.index, targetIndex);
                    var stepsInHall = Math.Abs(targetIndex - start.index);
                    var cost = start.c.GetCost() * stepsInHall;
                    yield return (newMaze, cost);
                }
            }
        }
    }

    public IEnumerable<(Maze maze, int cost)> TryGetStepFromHallToRoom(Maze maze, int indexRoom)
    {
        var dataAboutLetterForMove = maze.GetDataLettersWhichCanMoveInHall();
        foreach (var start in dataAboutLetterForMove)
        {
            var placesForMaybeMove = maze.GetIndexWhereCanMoveLetterInRoom(indexRoom, start.c);
            if (!placesForMaybeMove.HasValue) continue;

            if (maze.CanGetStepInHall(maze.Rooms[indexRoom].IndexDoor, start.index))
            {
                var newMaze = maze.GetMazeAfterStepBetweenHallwayAndRoom(indexRoom, placesForMaybeMove.Value, start.index);
                var stepsInHall = Math.Abs(maze.Rooms[indexRoom].IndexDoor - start.index);
                var stepsInRoom = placesForMaybeMove.Value + 1;
                var cost = start.c.GetCost() * (stepsInRoom + stepsInHall);
                yield return (newMaze, cost);
            }
        }
    }
}

class Run  
{
    static int Solve(List<string> lines)
    {
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