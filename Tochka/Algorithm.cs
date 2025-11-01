using System.Diagnostics;
using Tochka.Models;

namespace Tochka;

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
        var hallWay = new Hallway(String.Concat(Enumerable.Repeat(".", 11)));
        var rooms = new List<Room>();
        for (var i = 2; i <= 8; i+=2)
        {
            var charForRepeat = Chars.GetCharByIndexDoorForRoom(i);
            var room = new Room(i, String.Concat(Enumerable.Repeat(charForRepeat, Room.RoomSize)));
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
            if (!Char.IsLetter(c)) continue;

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
                if (!Char.IsLetter(c)) continue;
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

            //for (int i = 0; i < maze.Rooms.Length; i++)
            //results.AddRange(TryGetStepFromRoomToRoom(maze, i));
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
        if (dataAboutLetterForMove == null)
            yield break;
        var placesForMaybeMove = maze.GetIndexesWhereCanMoveLetterInHall();
        foreach (var targetIndex in placesForMaybeMove)
        {
            if (maze.CanGetStepInHall(maze.Rooms[indexRoom].IndexDoor, targetIndex))
            {
                var newMaze = maze.GetMazeAfterStepBetweenHallwayAndRoom(indexRoom,
                    dataAboutLetterForMove.index, targetIndex);
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
            if (!placesForMaybeMove.HasValue)
                continue;
            if (maze.CanGetStepInHall(maze.Rooms[indexRoom].IndexDoor, start.index))
            {
                var newMaze = maze.GetMazeAfterStepBetweenHallwayAndRoom(indexRoom,
                    placesForMaybeMove.Value, start.index);
                var stepsInHall = Math.Abs(maze.Rooms[indexRoom].IndexDoor - start.index);
                var stepsInRoom = placesForMaybeMove.Value + 1;
                var cost = start.c.GetCost() * (stepsInRoom + stepsInHall);
                yield return (newMaze, cost);
            }
        }
    }
    
    
}