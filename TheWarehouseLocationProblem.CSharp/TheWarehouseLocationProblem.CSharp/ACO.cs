using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWarehouseLocationProblem.CSharp
{
    public class ACO
    {
        internal int customerCount;
        internal bool debug;
        internal int warehouseCount;
        internal int MAX_GENERATION_COUNT;

        int antCount = 0;
        int pheremoneTrail = 10;

        class Res
        {
            public double cost;
            public int[] assignments;

        }

        class History
        {
            public double totalCost;
            public double count;

            public double avgCost
            {
                get
                {
                    return (count == 0) ? 0 : (totalCost / count);
                }
            }

            public double reverseCost
            {
                get
                {
                    return (avgCost == 0) ? 0 : (1000 / avgCost);
                }
            }
        }

        History[,] historyList;

        List<History[,]> historyListList = new List<History[,]>();

        List<Res> results = new List<Res>();
        const int RANDOM_SEED = 2785221;
        Random rnd = new Random(RANDOM_SEED);
        internal void Start()
        {
            historyList = new History[customerCount, warehouseCount];

            for (int i = 0; i < customerCount; i++)
            {
                for (int j = 0; j < warehouseCount; j++)
                {
                    historyList[i, j] = new History();
                }
            }

            antCount = 1000;//10* (int)Math.Sqrt(warehouseCount);

            for (int i = 0; i < MAX_GENERATION_COUNT; i++)
            {
                var tmpHistoryList = new History[customerCount, warehouseCount];

                for (int j = 0; j < customerCount; j++)
                {
                    for (int k = 0; k < warehouseCount; k++)
                    {
                        tmpHistoryList[j, k] = new History();
                    }
                }

                for (int j = 0; j < antCount; j++)
                {
                    double totalCost = 0;
                    var path = new int[customerCount];
                    var selected = new bool[warehouseCount];
                    for (int k = 0; k < customerCount; k++)
                    {
                        int warehouseIndex = GetWarehouseIndex(k);

                        totalCost += Program.CustomerWarehouseCosts[k, warehouseIndex];
                        selected[warehouseIndex] = true;
                        path[k] = warehouseIndex;
                    }

                    for (int k = 0; k < warehouseCount; k++)
                    {
                        if (selected[k]) totalCost += Program.Warehouses[k].setupCost;
                    }

                    for (int k = 0; k < customerCount; k++)
                    {
                        historyList[k, path[k]].totalCost += totalCost;
                        historyList[k, path[k]].count++;
                        tmpHistoryList[k, path[k]].totalCost += totalCost;
                        tmpHistoryList[k, path[k]].count++;
                    }
                }

                historyListList.Add(tmpHistoryList);

                var result = new int[customerCount];
                for (int j = 0; j < customerCount; j++)
                {
                    var mincost = double.MaxValue;
                    for (int k = 0; k < warehouseCount; k++)
                    {
                        if (mincost > historyList[j, k].avgCost)
                        {
                            mincost = historyList[j, k].avgCost;
                            result[j] = k;
                            // Total resulttan costları hesapla
                        }
                    }
                }

                double bestCost = 0;
                var selectedWar = new bool[warehouseCount];
                for (int t = 0; t < customerCount; t++)
                {

                    bestCost += Program.CustomerWarehouseCosts[t, result[t]];
                    selectedWar[result[t]] = true;
                }

                for (int t = 0; t < warehouseCount; t++)
                {
                    if (selectedWar[t]) bestCost += Program.Warehouses[t].setupCost;
                }

                var res = new Res
                {
                    cost = bestCost,
                    assignments = result
                };

                results.Add(res);

                Console.WriteLine("gen : " + i + " Cost : " + (int)bestCost + " // Best gen cost : " + (int)(results.OrderBy(o => o.cost).FirstOrDefault()?.cost ?? 0));

                //if(i >=  pheremoneTrail)
                //{
                //    var deletedItem = historyListList.First();
                //    for (int j = 0; j < customerCount; j++)
                //    {
                //        for (int k = 0; k < warehouseCount; k++)
                //        {
                //            historyList[j, k].totalCost -= deletedItem[j, k].totalCost;
                //            historyList[j, k].count -= deletedItem[j, k].count;
                //        }
                //    }

                //    historyListList.Remove(deletedItem);
                //}
            }
        }

        private int GetWarehouseIndex(int customerIndex)
        {
            double totalCost = 0;
            int nonZeroCounter = 0;
            var probs = new int[warehouseCount];

            for (int i = 0; i < warehouseCount; i++)
            {
                if (historyList[customerIndex, i].reverseCost > 0)
                {
                    nonZeroCounter++;
                }
                totalCost += historyList[customerIndex, i].reverseCost;
            }

            for (int i = 0; i < warehouseCount; i++)
            {
                if (historyList[customerIndex, i].reverseCost == 0)
                {
                    probs[i] = (int)(100.0 / warehouseCount); //((warehouseCount  * 100) / (nonZeroCounter * ()));
                }
            }

            var remainingProb = 100 - probs.Sum(s => s);

            for (int i = 0; i < warehouseCount; i++)
            {
                if (historyList[customerIndex, i].reverseCost > 0)
                {
                    probs[i] = (int)((remainingProb * historyList[customerIndex, i].reverseCost) / totalCost);
                }
            }

            var totalProb = 0;
            int[] expectedProb = new int[1];
            totalProb = probs.Sum(s => s);
            expectedProb = new int[totalProb];

            int probCounter = 0;

            for (int i = 0; i < warehouseCount; i++)
            {
                for (int j = 0; j < probs[i]; j++)
                {
                    expectedProb[probCounter] = i;
                    probCounter++;
                }
            }
            return expectedProb[rnd.Next(totalProb)];
        }
    }
}
