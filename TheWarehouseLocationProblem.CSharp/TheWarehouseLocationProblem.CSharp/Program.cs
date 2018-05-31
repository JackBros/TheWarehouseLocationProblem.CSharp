using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWarehouseLocationProblem.CSharp
{
    class Program
    {

        class Warehouse
        {
            public int id;
            public int capacity;
            public int remainingCapacity;
            public double setupCost;
            public bool isSelected;
        }

        class Customer
        {
            public int id;
            public int demand;
            public int? assignedWarehouseId;
        }

        class Individual
        {
            public int[] genotype;
            public double fitness;
            public double selectionProbability;
            public int expectedCopies;
            public bool[] warehouseList;
            public double cost;
        }

        static Warehouse[] Warehouses;
        static Customer[] Customers;
        static Individual[] Population;
        static Individual[] MatingPool;
        static Individual[] NewGeneration;

        static double[,] CustomerWarehouseCosts;

        const int RANDOM_SEED = 3961, MAX_POPULATION_SIZE = 2000, MAX_GENERATION_COUNT = 4000;
        const double XOVER_RATE = 0.8;
        static Random rnd = new Random(RANDOM_SEED);

        static int warehouseCount, customerCount, populationSize, generationCount = 0;
        static double totalFitness = 0;

        static void Main(string[] args)
        {
            if(args.Count() == 1)
            {
                ReadFile(args[0]);
                GA();
                Console.ReadKey();
            }
        }

        static double ConvertToDouble(object obj)
        {
            try
            {
                string str = (obj?.ToString() ?? "0");
                double deger = 0;

                str = str.Replace(".", ",");
                if (str.Contains(","))
                {
                    int tamsayi = Convert.ToInt32(new string(str.Where(c => char.IsDigit(c)).ToArray()));
                    string[] split = str.Split(',');
                    if (split.Length == 2)
                    {
                        deger = System.Convert.ToDouble(split[0]) + (System.Convert.ToDouble(split[1]) / System.Math.Pow(10, split[1].Length));
                    }
                }
                else
                {
                    if (str != "") deger = Convert.ToDouble(str);
                }

                return deger;
            }
            catch
            {
                return 0;
            }
        }

        static void ReadFile(string fileName)
        {
            int i = 0, customerIndexCounter = 0;
            bool customerLineChecker = false;
            foreach (string line in File.ReadLines(fileName, Encoding.UTF8))
            {
                var items = line.Split(' ');
                if (i == 0)
                {                    
                    if(items.Count() == 2)
                    {
                        warehouseCount = Convert.ToInt32(items[0]);
                        customerCount = Convert.ToInt32(items[1]);

                        try
                        {
                            populationSize = Convert.ToInt32(Math.Min(Math.Pow(customerCount, warehouseCount) / 2, MAX_POPULATION_SIZE));
                        }
                        catch
                        {
                            populationSize = MAX_POPULATION_SIZE;
                        }

                        if (populationSize % 2 == 1) populationSize++;

                        Warehouses = new Warehouse[warehouseCount];
                        Customers = new Customer[customerCount];
                        CustomerWarehouseCosts = new double[customerCount,warehouseCount];
                    }
                }
                else if (i > 0 && i <= warehouseCount)
                {
                    if (items.Count() == 2)
                    {
                        Warehouses[i - 1] = new Warehouse
                        {
                            id = i - 1,
                            capacity = Convert.ToInt32(items[0]),
                            remainingCapacity = Convert.ToInt32(items[0]),
                            setupCost = ConvertToDouble(items[1]),
                            isSelected = false
                        };
                    }
                }
                else
                {
                    if (!customerLineChecker)
                    {
                        if (items.Count() == 1)
                        {
                            Customers[customerIndexCounter] = new Customer
                            {
                                id = customerIndexCounter,
                                demand = Convert.ToInt32(items[0]),
                                assignedWarehouseId = null
                            };
                        }
                    }
                    else
                    {
                        if(items.Count() == warehouseCount)
                        {
                            for (int j = 0; j < warehouseCount; j++)
                            {
                                CustomerWarehouseCosts[customerIndexCounter, j] = ConvertToDouble(items[j]);
                            }
                            customerIndexCounter++;
                        }
                    }

                    customerLineChecker = !customerLineChecker;
                }

                i++;
            }
        }

        static void Output()
        {
            /*
             * ofstream output;
	         * output.open("sol.txt");
             * 
	         * output << ("%f", population[0].cost);
	         * output << ("\n");
             * 
	         * printf("%f", population[0].cost);
	         * printf("\n");
	         * for (int i = 0; i < customerCount; i++)
	         * {
		     *     printf("%d", population[0].genotype[i]);
		     *     printf(" ");
		     *     output << ("%d", population[0].genotype[i]); output << (" ");
	         * }
             * 
	         * output.close();
             */
            var bestIndividual = Population.OrderBy(o => o.cost).FirstOrDefault();
            Console.WriteLine(bestIndividual.cost);

            string assignments = "";
            for (int i = 0; i < customerCount; i++)
            {
                assignments += bestIndividual.genotype[i] + " ";
            }

            Console.Write(assignments.TrimEnd());
        }

        static void GA()
        {
            GA_InitializePopulation();
            GA_CalculateFitness();
            while (generationCount < MAX_GENERATION_COUNT)
            {
                var orderedPop = Population.OrderBy(o => o.cost);
                var bestIndividual = orderedPop.FirstOrDefault();
                var worstIndividual = orderedPop.LastOrDefault();

                Console.WriteLine(string.Format("Gen {0} : Min Cost ({1}) - Max Cost ({2}) // Best Fitness ({3}) - Worst Fitness ({4}) // Total Fitness ({5})", generationCount, bestIndividual.cost, worstIndividual.cost, bestIndividual.fitness, worstIndividual.fitness, totalFitness));

                GA_Reproduction();
                GA_CalculateFitness();
                generationCount++;
            }


            Console.WriteLine(" ");
            Console.WriteLine("FINISHED");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Output();
        }

        static void GA_InitializePopulation()
        {
            Population = new Individual[populationSize];
            MatingPool = new Individual[populationSize];
            NewGeneration = new Individual[populationSize];

            for (int i = 0; i < populationSize; i++)
            {
                Population[i] = new Individual
                {
                    genotype = new int[customerCount],
                    fitness = 0,
                    selectionProbability = 0,
                    expectedCopies = 0,
                    warehouseList = new bool[warehouseCount],
                    cost = 0
                };

                foreach (var customer in Customers)
                {
                    while (true)
                    {
                        int selectedWarehouseIndex = rnd.Next(warehouseCount);
                        Warehouse selectedWarehouse = Warehouses[selectedWarehouseIndex];

                        if (selectedWarehouse.remainingCapacity > customer.demand)
                        {
                            Population[i].genotype[customer.id] = selectedWarehouseIndex;
                            selectedWarehouse.remainingCapacity = selectedWarehouse.remainingCapacity - customer.demand;
                            break;
                        }
                    }
                }

                for (int j = 0; j < warehouseCount; j++)
                {
                    Population[i].warehouseList[j] = false;
                    Warehouses[j].remainingCapacity = Warehouses[j].capacity;
                }
            }
        }

        static void GA_CalculateFitness()
        {
            totalFitness = 0;

            for (int i = 0; i < populationSize; i++)
            {
                Individual ind = Population[i];
                ind.fitness = 0;

                for (int j = 0; j < customerCount; j++)
                {
                    int warehouseIndex = ind.genotype[j];

                    if(!ind.warehouseList[warehouseIndex])
                    {
                        ind.warehouseList[warehouseIndex] = true;
                        ind.cost += Warehouses[warehouseIndex].setupCost;
                    }

                    ind.cost += CustomerWarehouseCosts[j, warehouseIndex];
                }

                ind.fitness = Math.Pow(populationSize, 2) / ind.cost;

                totalFitness += ind.fitness;
            }
        }

        static void GA_Reproduction()
        {
            int totalCopies = 0;

            for (int i = 0; i < populationSize; i++)
            {
                Population[i].selectionProbability = Population[i].fitness / totalFitness;
                Population[i].expectedCopies = (int)Math.Ceiling(Population[i].selectionProbability * populationSize);
                totalCopies += Population[i].expectedCopies;
            }

            int[] matingCandidates = new int[totalCopies];
            int candidateCounter = 0;

            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < Population[i].expectedCopies; j++)
                {
                    matingCandidates[candidateCounter] = i;
                    candidateCounter++;
                }
            }

            for (int i = 0; i < populationSize; i++)
            {
                int randomParentIndex = rnd.Next(totalCopies);
                MatingPool[i] = Population[matingCandidates[randomParentIndex]];
            }

            for (int i = 0; i < populationSize / 2; i++)
            {
                int firstParentIndex = rnd.Next(populationSize);
                int secondParentIndex = rnd.Next(populationSize);

                var firstParent = MatingPool[firstParentIndex];
                var secondParent = MatingPool[secondParentIndex];

                double xoverRandom = rnd.NextDouble();

                if (xoverRandom > XOVER_RATE)
                {
                    var firstChild = new Individual
                    {
                        genotype = new int[customerCount],
                        fitness = 0,
                        selectionProbability = 0,
                        expectedCopies = 0,
                        warehouseList = new bool[warehouseCount],
                        cost = 0
                    };
                    var secondChild = new Individual
                    {
                        genotype = new int[customerCount],
                        fitness = 0,
                        selectionProbability = 0,
                        expectedCopies = 0,
                        warehouseList = new bool[warehouseCount],
                        cost = 0
                    };

                    int xoverPoint = rnd.Next(customerCount);

                    for (int j = 0; j < customerCount; j++)
                    {
                        if(j < xoverPoint)
                        {
                            firstChild.genotype[j] = firstParent.genotype[j];
                            secondChild.genotype[j] = secondParent.genotype[j];
                        }
                        else
                        {
                            firstChild.genotype[j] = secondParent.genotype[j];
                            secondChild.genotype[j] = firstParent.genotype[j];
                        }
                    }

                    NewGeneration[i * 2] = firstChild;
                    NewGeneration[(i * 2) + 1] = secondChild;
                }
                else
                {

                    NewGeneration[i * 2] = firstParent;
                    NewGeneration[(i * 2) + 1] = secondParent;
                }
            }

            Population = NewGeneration;
        }
    }
}
