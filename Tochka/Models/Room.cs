namespace Tochka.Models;

public class Room 
{
    public static int RoomSize { get; set; }

    public readonly int IndexDoor;

    public readonly string RoomString;

    public Room(int indexDoor, string roomString)
    {
        if (RoomSize == 0)
            throw new ArgumentException();
        IndexDoor = indexDoor;
        RoomString = roomString;
    }

    public Room GetChangedRoom(Char newChar, int index)
    {
        var newRoomString = RoomString.ChangeString(newChar, index);
        return new Room(IndexDoor, newRoomString);
    }
    
    public override bool Equals(object obj)
    {
        if (obj is not Room other) return false;
        
        return IndexDoor == other.IndexDoor && 
               string.Equals(RoomString, other.RoomString);
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