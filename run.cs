using System;
using System.Collections.Generic;

public static class Chars
    {
        // чтобы влезли char такие массивы
        public static readonly int[] Cost = new int[128]; 
        public static readonly int[] RoomIndex = new int[128];
        public static readonly int[] DoorIndexToRoomIndex = new int[11];
        public static readonly bool[] IsDoor = new bool[11];

        static Chars()
        {
            Cost['A'] = 1; Cost['B'] = 10; Cost['C'] = 100; Cost['D'] = 1000;
            RoomIndex['A'] = 0; RoomIndex['B'] = 1; RoomIndex['C'] = 2; RoomIndex['D'] = 3;
            DoorIndexToRoomIndex[2] = 0; DoorIndexToRoomIndex[4] = 1; DoorIndexToRoomIndex[6] = 2; DoorIndexToRoomIndex[8] = 3;
            IsDoor[2] = IsDoor[4] = IsDoor[6] = IsDoor[8] = true;
        }

        public static bool IsLetter(char c) => c >= 'A' && c <= 'D';
    }

    public class State 
    {
        public readonly char[] Hall; 
        public readonly char[][] Rooms; 
        private readonly int hash;

        public State(char[] hall, char[][] rooms)
        {
            Hall = hall;
            Rooms = rooms;
            hash = CalcHash();
        }

        private int CalcHash()
        {
            unchecked
            {
                int h = 17;
                for (int i = 0; i < Hall.Length; i++) h = h * 31 + Hall[i];
                for (int r = 0; r < Rooms.Length; r++)
                    for (int i = 0; i < Rooms[r].Length; i++) h = h * 31 + Rooms[r][i];
                return h;
            }
        }

        public override int GetHashCode() => hash;

        public override bool Equals(object obj)
        {
            if (obj is not State other) return false;
            for (var i = 0; i < Hall.Length; i++) 
                if (Hall[i] != other.Hall[i]) return false;
            for (int roomIndex = 0; roomIndex < Rooms.Length; roomIndex++)
            {
                var roomObj = Rooms[roomIndex]; 
                var roomOther = other.Rooms[roomIndex];
                for (int i = 0; i < roomObj.Length; i++) 
                    if (roomObj[i] != roomOther[i]) 
                        return false;
            }
            return true;
        }

        public State WithHallSwap(int i, int j)
        {
            var newHall = (char[])Hall.Clone();
            var tmp = newHall[i]; newHall[i] = newHall[j]; newHall[j] = tmp;
            return new State(newHall, CloneRoomsShallow());
        }

        public State WithHallRoomSwap(int roomIndex, int posInRoom, int hallIndex)
        {
            var newHall = (char[])Hall.Clone();
            var newRooms = CloneRoomsShallow();
            var tmp = newRooms[roomIndex][posInRoom];
            newRooms[roomIndex][posInRoom] = newHall[hallIndex];
            newHall[hallIndex] = tmp;
            return new State(newHall, newRooms);
        }

        private char[][] CloneRoomsShallow()
        {
            var newRooms = new char[4][];
            for (int i = 0; i < 4; i++) 
                newRooms[i] = (char[])Rooms[i].Clone();
            return newRooms;
        }
    }

    public static class Parser
    {
        public static char[] ParseHall(string line)
        {
            var s = line.Substring(1, line.Length - 2);
            return s.ToCharArray();
        }

        public static char[][] ParseRooms(List<string> input)
        {
            var roomAmount = 4; 
            var rows = input.Count - 3; 
            var rooms = new char[roomAmount][];
            for (var indexRoom = 0; indexRoom < roomAmount; indexRoom++) 
                rooms[indexRoom] = new char[rows];
            for (var row = 0; row < rows; row++)
            {
                var line = input[2 + row];
                for (int col = 0; col < roomAmount; col++)
                {
                    var ch = line[3 + col * 2];
                    rooms[col][row] = ch;
                }
            }
            return rooms;
        }
    }

    public class Algorithm
    {
        private readonly State start;
        private readonly State end;
        private static readonly int[] doorPositions = { 2, 4, 6, 8 };

        public Algorithm(State start, int roomSize)
        {
            this.start = start; 
            end = GetEndState(roomSize);
        }

        private State GetEndState(int depth)
        {
            var hall = new char[11];
            for (int i = 0; i < 11; i++) 
                hall[i] = '.';
            var rooms = new char[4][];
            var chars = new char[]{ 'A', 'B', 'C', 'D' };
            for (int roomIndex = 0; roomIndex < 4; roomIndex++)
            {
                rooms[roomIndex] = new char[depth];
                for (int i = 0; i < depth; i++) 
                    rooms[roomIndex][i] = chars[roomIndex];
            }
            return new State(hall, rooms);
        }

        private int Heuristic(State s)
        {
            var result = 0;
            for (var i = 0; i < s.Hall.Length; i++)
            {
                var c = s.Hall[i];
                if (!Chars.IsLetter(c)) 
                    continue;
                var targetDoor = Chars.RoomIndex[c] * 2 + 2;
                var steps = Math.Abs(targetDoor - i) + 1; // минимум необходимых шагов
                result += steps * Chars.Cost[c];
            }
            
            for (var r = 0; r < s.Rooms.Length; r++)
            {
                var room = s.Rooms[r];
                for (int pos = 0; pos < room.Length; pos++)
                {
                    var c = room[pos];
                    if (!Chars.IsLetter(c)) continue;
                    var targetRoom = Chars.RoomIndex[c];
                    if (targetRoom == r)
                    {
                        // если есть импостер то Math.Abs(doorFrom - doorTo) и мы должны его выпустить а поэтому нужно pos + 1
                        var wrongBelow = false;
                        for (var k = pos + 1; k < room.Length; k++)
                            if (room[k] != c)
                            {
                                wrongBelow = true; 
                                break;
                            }
                        if (!wrongBelow) 
                            continue;
                    }
                    var doorFrom = doorPositions[r];
                    var doorTo = doorPositions[targetRoom];
                    var steps = pos + 1 + Math.Abs(doorFrom - doorTo) + 1;
                    result += steps * Chars.Cost[c];
                }
            }
            return result;
        }
        
        public IEnumerable<(State state, int cost)> GetNext(State s)
        {
            for (var r = 0; r < 4; r++)
            {
                var first = FirstInRoom(s, r);
                if (first.index == -1) continue;
                for (var i = 0; i < 11; i++)
                {
                    if (Chars.IsDoor[i]) 
                        continue; 
                    if (s.Hall[i] != '.') 
                        continue;
                    if (!CanMoveInHall(s.Hall, r, i)) 
                        continue;
                    var newState = s.WithHallRoomSwap(r, first.index, i);
                    var stepsInRoom = first.index + 1;
                    var stepsInHall = Math.Abs(i - doorPositions[r]);
                    var cost = Chars.Cost[first.c] * (stepsInRoom + stepsInHall);
                    yield return (newState, cost);
                }
            }
            
            for (int i = 0; i < 11; i++)
            {
                var c = s.Hall[i];
                if (!Chars.IsLetter(c)) 
                    continue;
                var r = Chars.RoomIndex[c];
                var posInRoom = IndexWhereCanMoveIntoRoom(s, r, c);
                if (posInRoom == -1) 
                    continue;
                if (!CanPathClear(s.Hall, i, doorPositions[r])) 
                    continue;
                var newState = s.WithHallRoomSwap(r, posInRoom, i);
                var stepsInHall = Math.Abs(doorPositions[r] - i);
                var stepsInRoom = posInRoom + 1;
                var cost = Chars.Cost[c] * (stepsInHall + stepsInRoom);
                yield return (newState, cost);
            }
            
            for (var i = 0; i < 11; i++)
            {
                var c = s.Hall[i];
                if (!Chars.IsLetter(c)) 
                    continue;
                for (var j = 0; j < 11; j++)
                {
                    if (i == j) 
                        continue;
                    if (Chars.IsDoor[j]) 
                        continue;
                    if (s.Hall[j] != '.') 
                        continue;
                    if (!CanPathClear(s.Hall, i, j)) 
                        continue;
                    var newState = s.WithHallSwap(i, j);
                    var steps = Math.Abs(i - j);
                    var cost = Chars.Cost[c] * steps;
                    yield return (newState, cost);
                }
            }
        }

        private (int index, char c) FirstInRoom(State s, int room)
        {
            var arr = s.Rooms[room];
            for (int i = 0; i < arr.Length; i++) 
                if (arr[i] != '.') 
                    return (i, arr[i]);
            return (-1, 'ы');
        }

        private bool CanMoveInHall(char[] hall, int roomIndex, int targetHallIndex)
        {
            var door = doorPositions[roomIndex];
            return CanPathClear(hall, door, targetHallIndex);
        }

        private bool CanPathClear(char[] hall, int from, int to)
        {
            var s = Math.Min(from, to);
            var e = Math.Max(from, to);
            for (var i = s + 1; i < e; i++) 
                if (hall[i] != '.') 
                    return false;
            return true;
        }

        private int IndexWhereCanMoveIntoRoom(State s, int indexRoom, char mustBe)
        {
            var room = s.Rooms[indexRoom];
            var haveSpace = false;
            for (int i = 0; i < room.Length; i++)
            {
                var ch = room[i];
                if (ch == '.')
                {
                    haveSpace = true; 
                    continue;
                }
                for (var k = i; k < room.Length; k++) 
                    if (room[k] != mustBe) 
                        return -1;
                if (haveSpace) 
                    return i - 1;
                return -1;
            }
            if (haveSpace) 
                return room.Length - 1;
            return -1;
        }

        public int Start()
        {
            var heap = new PriorityQueue<State, int>();
            var minCosts = new Dictionary<State, int>();

            minCosts[start] = 0;
            heap.Enqueue(start, Heuristic(start));

            while (heap.TryDequeue(out var curr, out var priority))
            {
                if (curr.Equals(end)) 
                    return minCosts[curr];
                var currCost = minCosts[curr];
                foreach (var (next, moveCost) in GetNext(curr))
                {
                    var allCost = currCost + moveCost;
                    if (!minCosts.TryGetValue(next, out var oldCost) || allCost < oldCost)
                    {
                        minCosts[next] = allCost;
                        var f = allCost + Heuristic(next);
                        heap.Enqueue(next, f);
                    }
                }
            }
            throw new Exception("ыыыы");
        }
    }

class Program  
{
    static int Solve(List<string> lines)
    {
        var roomSize = lines.Count - 3;
        var hall = Parser.ParseHall(lines[1]);
        var rooms = Parser.ParseRooms(lines);
        var state = new State(hall, rooms);
        var alg = new Algorithm(state, roomSize);
        return alg.Start();
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