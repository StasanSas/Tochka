using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class run2
{
    

    public static (char, (char, char)) GetNextPositionAndCurrentAction(
        Dictionary<char, List<char>> graph, char currPosition)
    {
        var stack1 = new Stack<char>();
        stack1.Push(currPosition);
        var stack2 = new Stack<char>();
        
        var visited = new HashSet<char>();
        visited.Add(currPosition);
        
        var dictionaryForPath = new Dictionary<char, List<char>>();
        dictionaryForPath.Add(currPosition, new List<char>());
        var pathForSort = new List<List<char>>();
        while (true)
        {
            var newVisited = new HashSet<char>();
            // цикл отвечает заслой в бфс
            while (stack1.Count > 0)
            {
                currPosition = stack1.Pop();
                if (Char.IsUpper(currPosition))
                    pathForSort.AddRange(GetPathFromDict(dictionaryForPath, currPosition));
                
                foreach (var nextChar in graph[currPosition])
                {
                    if (visited.Contains(nextChar)) continue;
                    if (!dictionaryForPath.ContainsKey(nextChar))
                        dictionaryForPath[nextChar] = new List<char>();
                    dictionaryForPath[nextChar].Add(currPosition);
                    if (!newVisited.Contains(nextChar))
                        stack2.Push(nextChar);
                    newVisited.Add(nextChar);
                }
            }
            visited.UnionWith(newVisited);
            if (pathForSort.Count > 0) break;
            stack1 = stack2;
            stack2 = new Stack<char>();
        }

        return GetNextPositionAndCurrentActionFromListPaths(pathForSort);
    }

    public static List<List<char>> GetPathFromDict(Dictionary<char, List<char>> dictionaryForPath, char end)
    {
        var result = new List<List<char>>();
        var stack1 = new Stack<List<char>>();
        stack1.Push(new List<char>() {end});
        var stack2 = new Stack<List<char>>();
        while (true)
        {
            while (stack1.Count > 0)
            {
                var currPartPath = stack1.Pop();
                var currSymbol = currPartPath[currPartPath.Count - 1];
                if (dictionaryForPath[currSymbol].Count == 0)
                    result.Add(currPartPath);

                for(var i = 0; i < dictionaryForPath[currSymbol].Count; i++)
                {
                    var nextSymbol = dictionaryForPath[currSymbol][i];
                    var nextPath = currPartPath;
                    if (i < dictionaryForPath[currSymbol].Count - 1)
                        nextPath = nextPath.ToList(); 
                    nextPath.Add(nextSymbol);
                    stack2.Push(nextPath);
                }
            }
            
            if (result.Count > 0) break;
            stack1 = stack2;
            stack2 = new Stack<List<char>>();
        }

        return result;
    }

    public static (char, (char, char)) GetNextPositionAndCurrentActionFromListPaths(List<List<char>> pathForSort)
    {
        Comparer<List<char>> comparer = Comparer<List<char>>.Create((list1, list2) =>
        {
            var lastSymbol1 = list1[0];
            var lastSymbol2 = list2[0];
            if (lastSymbol1.CompareTo(lastSymbol2) != 0)
                return lastSymbol1.CompareTo(lastSymbol2);
            //мы работаем с перевёрнутым путём
            for(var i = list1.Count - 2; i > 0; i--)
            {
                if (list1[i] != list2[i])
                    return list1[i].CompareTo(list2[i]);
            }
            return 0;
        });
        pathForSort.Sort(comparer);
        var pathOnCurrentStep = pathForSort[0];
        var l = pathOnCurrentStep.Count;
        return (pathOnCurrentStep[l - 2], (pathOnCurrentStep[0], pathOnCurrentStep[1]));
    }
    
    public static Dictionary<char, List<char>> CreateGraph(List<(string, string)> edges)
    {
        var result = new Dictionary<char, List<char>>();
        foreach (var edge in edges)
        {
            if (!result.ContainsKey(edge.Item1[0]))
                result.Add(edge.Item1[0], new List<char>());
            result[edge.Item1[0]].Add(edge.Item2[0]);
            if (!result.ContainsKey(edge.Item2[0]))
                result.Add(edge.Item2[0], new List<char>());
            result[edge.Item2[0]].Add(edge.Item1[0]);
        }

        return result;
    }
    
    static int GetAmountSteps(List<(string, string)> edges)
    {
        var amountGateway = 0;
        foreach (var edge in edges)
        {
            if (Char.IsUpper(edge.Item1[0]))
                amountGateway += 1;
            if (Char.IsUpper(edge.Item2[0]))
                amountGateway += 1;
        }
        return amountGateway;
    }
    
    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        var graph = CreateGraph(edges);
        var amountSteps = GetAmountSteps(edges);
        var currentPositionVirus = 'a';
        for (var i = 0; i < amountSteps; i++)
        {
            var step = GetNextPositionAndCurrentAction(graph, currentPositionVirus);
            currentPositionVirus = step.Item1;
            var pairForDelete = step.Item2;
            graph[pairForDelete.Item1].Remove(pairForDelete.Item2);
            graph[pairForDelete.Item2].Remove(pairForDelete.Item1);
            Console.WriteLine($"{pairForDelete.Item1}-{pairForDelete.Item2}");
        }
        
        return result;
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}