using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSP
{
    public class SetPartition
    {
        public static IEnumerable<List<List<T>>> GetAllPartitions<T>(T[] elements)
        {
            var lists = new List<List<T>>();
            var indexes = new int[elements.Length];
            lists.Add(new List<T>());
            lists[0].AddRange(elements);
            for (; ; )
            {
                yield return lists;
                int i, index;
                for (i = indexes.Length - 1; ; --i)
                {
                    if (i <= 0)
                        yield break;
                    index = indexes[i];
                    lists[index].RemoveAt(lists[index].Count - 1);
                    if (lists[index].Count > 0)
                        break;
                    lists.RemoveAt(index);
                }
                ++index;
                if (index >= lists.Count)
                    lists.Add(new List<T>());
                for (; i < indexes.Length; ++i)
                {
                    indexes[i] = index;
                    lists[index].Add(elements[i]);
                    index = 0;
                }
            }
        }
        public void testPartitions() {
            foreach (var part in GetAllPartitions(new[] { 1, 2, 3, 4 }))
            {
                Console.WriteLine(string.Join(",", part.Select(x => "(" + string.Join(",", x) + ")")));
            }
            //foreach (var partList in GetAllPartitions(new[] { 1, 2, 3, 4 })) {
            //    foreach (var part in partList)//每一个小子集
            //    { 
            //        Console.Write(string.Join(",",part));
            //    }
            //}
        }
    }
}
