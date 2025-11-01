namespace Tochka.Models;

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
        if (!ValidChars.Contains(c))
            throw new ArgumentException();
            
        return CostMap[c];
    }
    
    public static int GetIndexRoom(this char c)
    {
        if (!ValidChars.Contains(c))
            throw new ArgumentException();
            
        return RoomIndexMap[c];
    }
    
    public static char GetCharByIndexDoorForRoom(int index)
    {
        if (!ValidDoorIndexes.Contains(index))
            throw new ArgumentException();
            
        return DoorIndexToCharMap[index];
    }
    
}