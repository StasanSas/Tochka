using System.Text;
using Tochka.Models;

namespace Tochka;

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