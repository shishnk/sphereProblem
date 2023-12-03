﻿using SphereProblem.SphereMeshContext;

namespace SphereProblem;

public static class PortraitBuilder
{
    public static void Build(SphereMesh mesh, out int[] ig, out int[] jg)
    {
        var connectivityList = new List<HashSet<int>>();

        for (int i = 0; i < mesh.Points.Count; i++)
        {
            connectivityList.Add(new());
        }

        var localSize = mesh.Elements[0].Nodes.Count;
        
        foreach (var element in mesh.Elements)
        {
            for (int i = 0; i < localSize; i++)
            {
                var posToInsert = element[i];

                for (int j = 0; j < localSize; j++)
                {
                    var nodeToInsert = element[j];
                    
                    if (posToInsert <= nodeToInsert) continue;

                    connectivityList[posToInsert].Add(nodeToInsert);
                }
            }
        }

        var orderedList = connectivityList.Select(list => list.OrderBy(val => val)).ToList();

        ig = new int[connectivityList.Count + 1];

        ig[0] = 0;
        ig[1] = 0;

        for (int i = 1; i < connectivityList.Count; i++)
        {
            ig[i + 1] = ig[i] + connectivityList[i].Count;
        }

        jg = new int[ig[^1]];

        for (int i = 1, j = 0; i < connectivityList.Count; i++)
        {
            foreach (var it in orderedList[i])
            {
                jg[j++] = it;
            }
        }
    }
}