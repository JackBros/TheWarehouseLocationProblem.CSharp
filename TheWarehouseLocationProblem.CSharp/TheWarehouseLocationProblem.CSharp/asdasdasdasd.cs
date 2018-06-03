//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TheWarehouseLocationProblem.CSharp
//{
//    class Program
//    {

//        class Warehouse
//        {
//            public int id;
//            public int capacity;
//            public int remainingCapacity;
//            public double setupCost;
//            public bool isSelected;
//        }

//        class Customer
//        {
//            public int id;
//            public int demand;
//            public int? assignedWarehouseId;
//            public double[] warehouseCosts;
//        }

//        class Individual
//        {
//            public int[] genotype;
//            public double fitness;
//            public double selectionProbability;
//            public int expectedCopies;
//            public bool[] warehouseList;
//            public double cost;
//        }

//        class Result
//        {
//            public int generation;
//            public Individual individual;
//        }

//        class WarehouseCustomerCost
//        {
//            public int warehouseId;
//            public double totalCost;
//            public double selectionProbability;
//            public int expectedCopies;
//        }

//        static Warehouse[] Warehouses;
//        static Customer[] Customers;
//        static Individual[] Population;
//        static Individual[] MatingPool;
//        static Individual[] NewGeneration;

//        static double[,] CustomerWarehouseCosts;

//        //const int RANDOM_SEED = 49, MAX_POPULATION_SIZE = 25000, MAX_GENERATION_COUNT = 400;

//        // 816 
//        //        const int RANDOM_SEED = 316269, MAX_POPULATION_SIZE = 20000, MAX_GENERATION_COUNT = 300;

//        // 801
//        //const int RANDOM_SEED = 6728122, MAX_POPULATION_SIZE = 19652, MAX_GENERATION_COUNT = 350;
//        // last const int RANDOM_SEED = 666, MAX_POPULATION_SIZE = 35125, MAX_GENERATION_COUNT = 250;
//        //800
//        //const int RANDOM_SEED = 6728122, MAX_POPULATION_SIZE = 19652, MAX_GENERATION_COUNT = 350;
//        const int RANDOM_SEED = 666, MAX_POPULATION_SIZE = 27312, MAX_GENERATION_COUNT = 350;
//        const double XOVER_RATE = 1;
//        static double MUTATION_RATE = 0;
//        static Random rnd = new Random(RANDOM_SEED);

//        static int warehouseCount, customerCount, populationSize, generationCount = 0;
//        static double fitnessCoefficient = 1;
//        static double totalFitness = 0;

//        static bool debug = false;
//        static string filePath = "";
//        static List<Result> results;

//        static void Main(string[] args)
//        {
//            try
//            {
//                if (args.Count() == 1)
//                {
//                    filePath = args[0];
//                    File.WriteAllText("out.txt", filePath);
//                }
//                else if (args.Count() == 2)
//                {
//                    filePath = args[0];
//                    debug = true;
//                }

//                if (!string.IsNullOrWhiteSpace(filePath))
//                {
//                    ReadFile(filePath);
//                    GA();
//                }
//            }
//            catch (Exception exc)
//            {
//                File.WriteAllText("exc.txt", exc.Message);
//            }
//        }

//        static double ConvertToDouble(object obj)
//        {
//            try
//            {
//                string str = (obj?.ToString() ?? "0");
//                double deger = 0;

//                str = str.Replace(".", ",");
//                if (str.Contains(","))
//                {
//                    string[] split = str.Split(',');
//                    if (split.Length == 2)
//                    {
//                        deger = System.Convert.ToDouble(split[0]) + (System.Convert.ToDouble(split[1]) / System.Math.Pow(10, split[1].Length));
//                    }
//                }
//                else
//                {
//                    if (str != "") deger = Convert.ToDouble(str);
//                }

//                return deger;
//            }
//            catch
//            {
//                return 0;
//            }
//        }

//        static void ReadFile(string fileName)
//        {
//            int i = 0, customerIndexCounter = 0;
//            bool customerLineChecker = false;
//            foreach (string line in File.ReadLines(fileName, Encoding.UTF8))
//            {
//                var items = line.Split(' ');
//                if (i == 0)
//                {
//                    if (items.Count() == 2)
//                    {
//                        warehouseCount = Convert.ToInt32(items[0]);
//                        customerCount = Convert.ToInt32(items[1]);

//                        double populationConstant = Math.Pow(customerCount, warehouseCount) / 2;
//                        try
//                        {
//                            populationSize = Convert.ToInt32(Math.Min(populationConstant, MAX_POPULATION_SIZE));
//                        }
//                        catch
//                        {
//                            populationSize = MAX_POPULATION_SIZE;
//                        }

//                        if (populationSize % 2 == 1) populationSize++;
//                        fitnessCoefficient = Math.Pow(populationSize, 2);

//                        Warehouses = new Warehouse[warehouseCount];
//                        Customers = new Customer[customerCount];
//                        CustomerWarehouseCosts = new double[customerCount, warehouseCount];
//                    }
//                }
//                else if (i > 0 && i <= warehouseCount)
//                {
//                    if (items.Count() == 2)
//                    {
//                        Warehouses[i - 1] = new Warehouse
//                        {
//                            id = i - 1,
//                            capacity = Convert.ToInt32(items[0]),
//                            remainingCapacity = Convert.ToInt32(items[0]),
//                            setupCost = ConvertToDouble(items[1]),
//                            isSelected = false
//                        };
//                    }
//                }
//                else
//                {
//                    if (!customerLineChecker)
//                    {
//                        if (items.Count() == 1)
//                        {
//                            Customers[customerIndexCounter] = new Customer
//                            {
//                                id = customerIndexCounter,
//                                demand = Convert.ToInt32(items[0]),
//                                assignedWarehouseId = null,
//                                warehouseCosts = new double[warehouseCount]
//                            };
//                        }
//                    }
//                    else
//                    {
//                        if (items.Count() == warehouseCount)
//                        {
//                            for (int j = 0; j < warehouseCount; j++)
//                            {
//                                CustomerWarehouseCosts[customerIndexCounter, j] = ConvertToDouble(items[j]);
//                                Customers[customerIndexCounter].warehouseCosts[j] = ConvertToDouble(items[j]);
//                            }
//                            customerIndexCounter++;
//                        }
//                    }

//                    customerLineChecker = !customerLineChecker;
//                }

//                i++;
//            }
//        }

//        static void Output()
//        {
//            /*
//             * ofstream output;
//	         * output.open("sol.txt");
//             * 
//	         * output << ("%f", population[0].cost);
//	         * output << ("\n");
//             * 
//	         * printf("%f", population[0].cost);
//	         * printf("\n");
//	         * for (int i = 0; i < customerCount; i++)
//	         * {
//		     *     printf("%d", population[0].genotype[i]);
//		     *     printf(" ");
//		     *     output << ("%d", population[0].genotype[i]); output << (" ");
//	         * }
//             * 
//	         * output.close();
//             */
//            //var bestIndividual = Population.OrderByDescending(o => o.fitness).FirstOrDefault();

//            var bestRun = results.OrderByDescending(o => o.individual.fitness).FirstOrDefault();
//            Console.WriteLine(bestRun.individual.cost.ToString().Replace(',', '.'));

//            string assignments = "";
//            for (int i = 0; i < customerCount; i++)
//            {
//                assignments += bestRun.individual.genotype[i] + " ";
//            }

//            Console.Write(assignments.TrimEnd());
//            if (debug) Console.ReadKey();
//        }

//        static void GA()
//        {
//            results = new List<Result>();
//            GA_InitializePopulation();
//            GA_CalculateFitness();
//            while (generationCount < MAX_GENERATION_COUNT)
//            {

//                var orderedPop = Population.OrderByDescending(o => o.fitness);
//                var bestIndividual = orderedPop.FirstOrDefault();
//                var worstIndividual = orderedPop.LastOrDefault();

//                //if (bestIndividual.fitness - worstIndividual.fitness < 0.001)
//                //{
//                //    try
//                //    {
//                //        MUTATION_RATE = MUTATION_RATE * 1.1;
//                //    }
//                //    catch
//                //    {

//                //    }
//                //}

//                results.Add(new Result
//                {
//                    generation = generationCount,
//                    individual = bestIndividual
//                });

//                //if(generationCount > 50)
//                //{
//                //    if((Math.Abs(results.Where(w => w.generation > (generationCount - 50)).Average(a => a.individual.cost) - bestIndividual.cost) < 1000))
//                //    {
//                //        if (MUTATION_RATE * 1.1 < 0.9)
//                //        {
//                //            MUTATION_RATE = MUTATION_RATE * 1.1;
//                //        }
//                //    }
//                //    else
//                //    {
//                //        MUTATION_RATE = 0.01;
//                //    }
//                //}

//                if (debug)
//                {
//                    Console.WriteLine(string.Format("Gen {0} : Min Cost ({1}) - Max Cost ({2}) // Best Fitness ({3}) - Worst Fitness ({4}) // Total Fitness ({5})", generationCount, bestIndividual.cost, worstIndividual.cost, bestIndividual.fitness, worstIndividual.fitness, totalFitness));
//                }

//                GA_Reproduction();
//                GA_Mutation();
//                GA_CalculateFitness();
//                generationCount++;
//            }


//            if (debug)
//            {
//                Console.WriteLine(" ");
//                Console.WriteLine("FINISHED");
//                Console.WriteLine(" ");
//                Console.WriteLine(" ");
//                Console.WriteLine(" ");
//            }
//            Output();
//        }

//        static void GA_InitializePopulation()
//        {
//            Population = new Individual[populationSize];
//            MatingPool = new Individual[populationSize];
//            NewGeneration = new Individual[populationSize];

//            for (int i = 0; i < populationSize; i++)
//            {
//                Population[i] = new Individual
//                {
//                    genotype = new int[customerCount],
//                    fitness = 0,
//                    selectionProbability = 0,
//                    expectedCopies = 0,
//                    warehouseList = new bool[warehouseCount],
//                    cost = 0
//                };

//                foreach (var customer in Customers)
//                {
//                    var tmpCosts = new WarehouseCustomerCost[warehouseCount];
//                    double totalCost = 0;
//                    int totalCopies = 0;

//                    for (int k = 0; k < warehouseCount; k++)
//                    {
//                        var cost = (customer.warehouseCosts[k] + Warehouses[k].setupCost);
//                        cost = (cost == 0) ? 1 : cost;
//                        tmpCosts[k] = new WarehouseCustomerCost
//                        {
//                            warehouseId = k,
//                            totalCost = 5 / cost
//                        };

//                        totalCost += tmpCosts[k].totalCost;
//                    }

//                    for (int k = 0; k < warehouseCount; k++)
//                    {
//                        tmpCosts[k].selectionProbability = tmpCosts[k].totalCost / totalCost;
//                        tmpCosts[k].expectedCopies = (int)Math.Ceiling(tmpCosts[k].selectionProbability * warehouseCount);
//                        totalCopies += tmpCosts[k].expectedCopies;
//                    }

//                    int[] warehouseCandidates = new int[totalCopies];
//                    int candidateCounter = 0;

//                    for (int k = 0; k < warehouseCount; k++)
//                    {
//                        for (int j = 0; j < tmpCosts[k].expectedCopies; j++)
//                        {
//                            warehouseCandidates[candidateCounter] = k;
//                            candidateCounter++;
//                        }
//                    }

//                    while (true)
//                    {
//                        int randomWarehouseIndex = rnd.Next(totalCopies);
//                        int selectedWarehouseIndex = warehouseCandidates[randomWarehouseIndex];
//                        Warehouse selectedWarehouse = Warehouses[selectedWarehouseIndex];



//                        if (selectedWarehouse.remainingCapacity > customer.demand)
//                        {
//                            Population[i].genotype[customer.id] = selectedWarehouseIndex;
//                            selectedWarehouse.remainingCapacity = selectedWarehouse.remainingCapacity - customer.demand;
//                            break;
//                        }

//                    }
//                }

//                for (int j = 0; j < warehouseCount; j++)
//                {
//                    Population[i].warehouseList[j] = false;
//                    Warehouses[j].remainingCapacity = Warehouses[j].capacity;
//                }
//            }
//        }

//        static void GA_CalculateFitness()
//        {
//            totalFitness = 0;

//            for (int i = 0; i < populationSize; i++)
//            {
//                Individual ind = Population[i];
//                ind.fitness = 0;

//                if (GA_IsIndividualFeasible(ind))
//                {
//                    for (int j = 0; j < customerCount; j++)
//                    {
//                        int warehouseIndex = ind.genotype[j];

//                        if (!ind.warehouseList[warehouseIndex])
//                        {
//                            ind.warehouseList[warehouseIndex] = true;
//                            ind.cost += Warehouses[warehouseIndex].setupCost;
//                        }

//                        ind.cost += CustomerWarehouseCosts[j, warehouseIndex];
//                    }

//                    ind.fitness = fitnessCoefficient / ind.cost;
//                    totalFitness += ind.fitness;
//                }
//                else
//                {
//                    ind.cost = 999999999999;
//                    ind.fitness = 0;
//                }


//            }
//        }

//        static bool GA_IsIndividualFeasible(Individual ind)
//        {
//            int i = 0;
//            bool isFeasible = true;

//            foreach (var warehouseIndex in ind.genotype)
//            {
//                var customer = Customers[i];
//                var warehouse = Warehouses[warehouseIndex];

//                if (warehouse.remainingCapacity > customer.demand)
//                {
//                    warehouse.remainingCapacity = warehouse.remainingCapacity - customer.demand;
//                }
//                else
//                {
//                    isFeasible = false;
//                }

//                i++;
//            }

//            for (i = 0; i < warehouseCount; i++)
//            {
//                Warehouses[i].remainingCapacity = Warehouses[i].capacity;
//            }
//            return isFeasible;
//        }

//        static void GA_Reproduction()
//        {
//            int totalCopies = 0;

//            for (int i = 0; i < populationSize; i++)
//            {
//                Population[i].selectionProbability = Population[i].fitness / totalFitness;
//                Population[i].expectedCopies = (int)Math.Ceiling(Population[i].selectionProbability * populationSize);
//                totalCopies += Population[i].expectedCopies;
//            }

//            int[] matingCandidates = new int[totalCopies];
//            int candidateCounter = 0;

//            for (int i = 0; i < populationSize; i++)
//            {
//                for (int j = 0; j < Population[i].expectedCopies; j++)
//                {
//                    matingCandidates[candidateCounter] = i;
//                    candidateCounter++;
//                }
//            }

//            for (int i = 0; i < populationSize; i++)
//            {
//                try
//                {
//                    int randomParentIndex = rnd.Next(totalCopies);
//                    MatingPool[i] = Population[matingCandidates[randomParentIndex]];
//                }
//                catch (Exception exc)
//                {
//                    string s = exc.ToString();
//                }
//            }

//            for (int i = 0; i < populationSize / 2; i++)
//            {
//                int firstParentIndex = rnd.Next(populationSize);
//                int secondParentIndex = rnd.Next(populationSize);

//                var firstParent = MatingPool[firstParentIndex];
//                var secondParent = MatingPool[secondParentIndex];

//                double xoverRandom = rnd.NextDouble();

//                if (xoverRandom > (1 - XOVER_RATE))
//                {
//                    var firstChild = new Individual
//                    {
//                        genotype = new int[customerCount],
//                        fitness = 0,
//                        selectionProbability = 0,
//                        expectedCopies = 0,
//                        warehouseList = new bool[warehouseCount],
//                        cost = 0
//                    };
//                    var secondChild = new Individual
//                    {
//                        genotype = new int[customerCount],
//                        fitness = 0,
//                        selectionProbability = 0,
//                        expectedCopies = 0,
//                        warehouseList = new bool[warehouseCount],
//                        cost = 0
//                    };

//                    for (int x = 0; x < customerCount / 2; x++)
//                    {
//                        int xoverPoint = rnd.Next(customerCount);


//                        for (int j = 0; j < customerCount; j++)
//                        {
//                            if (j < xoverPoint)
//                            {
//                                firstChild.genotype[j] = firstParent.genotype[j];
//                                secondChild.genotype[j] = secondParent.genotype[j];
//                            }
//                            else
//                            {
//                                firstChild.genotype[j] = secondParent.genotype[j];
//                                secondChild.genotype[j] = firstParent.genotype[j];
//                            }
//                        }
//                    }

//                    NewGeneration[i * 2] = firstChild;
//                    NewGeneration[(i * 2) + 1] = secondChild;
//                }
//                else
//                {

//                    NewGeneration[i * 2] = firstParent;
//                    NewGeneration[(i * 2) + 1] = secondParent;
//                }
//            }

//            Population = NewGeneration;

//            //var pop = Population.OrderByDescending(o => o.fitness).ToArray();
//            //var gen = NewGeneration.OrderByDescending(o => o.fitness).ToArray();

//            //Population = new Individual[populationSize];

//            //for (int l = 0; l < populationSize / 2; l++)
//            //{
//            //    Population[l * 2] = pop[l];
//            //    Population[(l * 2) + 1] = gen[l];
//            //}

//        }

//        static void GA_Mutation()
//        {
//            for (int i = 0; i < populationSize; i++)
//            {
//                var individual = Population[i];

//                for (int j = 0; j < customerCount; j++)
//                {
//                    double mutationRandom = rnd.NextDouble();

//                    if (mutationRandom > (1 - MUTATION_RATE))
//                    {
//                        individual.genotype[j] = rnd.Next(warehouseCount);
//                    }

//                }
//            }
//        }
//    }
//}
