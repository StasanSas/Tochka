namespace Tochka.Models;

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
        
        return Equals(Hallway, other.Hallway) && 
               Rooms.SequenceEqual(other.Rooms);
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
    
    public override string ToString()
    {
        var lines = new List<string>();

        // первая строка — "крыша"
        lines.Add("#############");

        // вторая строка — коридор
        lines.Add("#" + Hallway.HallString + "#");

        // комнаты имеют одинаковую длину, добавим их построчно
        int roomHeight = Rooms[0].RoomString.Length;

        for (int depth = 0; depth < roomHeight; depth++)
        {
            string prefix = depth == 0 ? "###" : "  #";
            string suffix = depth == 0 ? "###" : "  #";

            // собираем строку вида: "#A#B#C#D#"
            string middle = string.Join("#", Rooms.Select(r => r.RoomString[depth]));
            lines.Add(prefix + middle + suffix);
        }

        // нижняя граница
        lines.Add("  #########  ");

        return string.Join(Environment.NewLine, lines);
    }
}