namespace Tochka.Models;

public class Hallway
{
    public readonly string HallString;

    public Hallway(string hallString)
    {
        HallString = hallString;
    }
    
    public Hallway GetChangedHall(Char newChar, int index)
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