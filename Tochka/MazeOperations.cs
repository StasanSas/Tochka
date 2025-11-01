using Tochka.Models;

namespace Tochka;

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
            if (maze.Hallway.HallString[i] != '.')
                return false;
        }
        return true;
    }
    

    public static IEnumerable<DataAboutChar> GetDataLettersWhichCanMoveInHall(this Maze maze)
    {
        for(var i = 0; i < maze.Hallway.HallString.Length; i++)
        {
            var currChar = maze.Hallway.HallString[i];
            if(currChar == '.')
                continue;
            var haveSpaceInLeft = i != 0 && maze.Hallway.HallString[i - 1] == '.';
            var haveSpaceInRight = i != (maze.Hallway.HallString.Length - 1) && maze.Hallway.HallString[i + 1] == '.';
            if (haveSpaceInLeft || haveSpaceInRight)
                yield return new DataAboutChar(i, currChar);
        }
    }
    
    public static DataAboutChar? GetDataLetterWhichFirstInRoom(this Maze maze, int indexRoom)
    {
        var currRoom = maze.Rooms[indexRoom];
        for(var i = 0; i < currRoom.RoomString.Length; i++)
        {
            var currChar = currRoom.RoomString[i];
            if (currChar != '.')
                return new DataAboutChar(i, currChar);
        }
        return null;
    }
    
    public static int? GetIndexWhereCanMoveLetterInRoom(
        this Maze maze, int indexRoom, char mustBeThisChar)
    {
        var currRoom = maze.Rooms[indexRoom];
        var haveSpace = false;
        for(var i = 0; i < currRoom.RoomString.Length; i++)
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
    
    public static bool HasOnlyCharFromIndex(this string str, int startIndex, char expected)
    {
        for (int i = startIndex; i < str.Length; i++)
        {
            if (str[i] != expected)
                return false;
        }
        return true;
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