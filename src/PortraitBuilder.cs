namespace SphereProblem;

public static class PortraitBuilder
{
    public static void Build(TestMesh mesh, out int[] ig, out int[] jg)
    {
        var connectivityList = new List<HashSet<int>>();

        for (int i = 0; i < mesh.Points.Count; i++)
        {
            connectivityList.Add(new());
        }

        foreach (var element in mesh.Elements)
        {
            foreach (var node in element.Nodes)
            {
                var connectivitySet = connectivityList[node];

                foreach (var otherNode in element.Nodes)
                {
                    if (otherNode > node)
                    {
                        connectivitySet.Add(otherNode);
                    }
                }
            }

            // for (int i = 0; i < localSize - 1; i++)
            // {
            //     int nodeToInsert = element[i];
            //
            //     for (int j = i + 1; j < localSize; j++)
            //     {
            //         int posToInsert = element[j];
            //
            //         connectivityList[posToInsert].Add(nodeToInsert);
            //     }
            // }
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
            foreach (var value in orderedList[i])
            {
                jg[j++] = value;
            }
        }
    }
}