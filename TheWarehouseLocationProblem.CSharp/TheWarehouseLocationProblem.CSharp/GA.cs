using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWarehouseLocationProblem.CSharp
{
    public class GA
    {
        public Warehouse[] Warehouses;
        public Customer[] Customers;
        public double[,] CustomerWarehouseCosts;
        Individual[] Population;
        Individual[] MatingPool;
        Individual[] NewGeneration;

        const int RANDOM_SEED = 313131;
        public double XOVER_RATE = 0;
        double MUTATION_RATE = 0;
        Random rnd = new Random(RANDOM_SEED);

        public int warehouseCount, customerCount, populationSize, generationCount = 0;
        public double fitnessCoefficient = 1;
        double totalFitness = 0;

        List<Result> results;

        public bool debug;
        public int MAX_POPULATION_SIZE = 0, MAX_GENERATION_COUNT = 0;

        void Output()
        {

            var bestRun = results.OrderByDescending(o => o.individual.fitness).FirstOrDefault();
            Console.WriteLine(bestRun.individual.cost.ToString().Replace(',', '.'));

            string assignments = "";
            for (int i = 0; i < customerCount; i++)
            {
                assignments += bestRun.individual.genotype[i] + " ";
            }

            Console.Write(assignments.TrimEnd());
            if (debug) Console.ReadKey();
        }

        public Result Start()
        {
            results = new List<Result>();
            InitializePopulation();
            CalculateFitness();
            while (generationCount < MAX_GENERATION_COUNT)
            {

                var orderedPop = Population.OrderByDescending(o => o.fitness);
                var bestIndividual = orderedPop.FirstOrDefault();
                var worstIndividual = orderedPop.LastOrDefault();

                //if (bestIndividual.fitness - worstIndividual.fitness < 0.001)
                //{
                //    try
                //    {
                //        MUTATION_RATE = MUTATION_RATE * 1.1;
                //    }
                //    catch
                //    {

                //    }
                //}

                results.Add(new Result
                {
                    generation = generationCount,
                    individual = bestIndividual
                });

                //if(generationCount > 50)
                //{
                //    if((Math.Abs(results.Where(w => w.generation > (generationCount - 50)).Average(a => a.individual.cost) - bestIndividual.cost) < 1000))
                //    {
                //        if (MUTATION_RATE * 1.1 < 0.9)
                //        {
                //            MUTATION_RATE = MUTATION_RATE * 1.1;
                //        }
                //    }
                //    else
                //    {
                //        MUTATION_RATE = 0.01;
                //    }
                //}

                if (debug)
                {
                    Console.WriteLine(string.Format("MAXG : {6} - POPS : {7} // Gen {0} : MinC({1}) - MaxC ({2}) // BF ({3}) - WF ({4}) // TF ({5})", generationCount, (int)bestIndividual.cost, (int)worstIndividual.cost, (int)bestIndividual.fitness, (int)worstIndividual.fitness, totalFitness, MAX_GENERATION_COUNT, MAX_POPULATION_SIZE));
                }

                Reproduction();
                if(MUTATION_RATE > 0) Mutation();
                CalculateFitness();
                generationCount++;
            }


            if (debug)
            {
                Console.WriteLine(" ");
                Console.WriteLine("FINISHED");
                Console.WriteLine(" ");
                Console.WriteLine(" ");
                Console.WriteLine(" ");
            }
            Output();

            return results.OrderByDescending(o => o.individual.fitness).FirstOrDefault();
        }

        void InitializePopulation()
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

        void InitializePopulation_Min()
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
                    var tmpCosts = new WarehouseCustomerCost[warehouseCount];
                    double totalCost = 0;
                    int totalCopies = 0;

                    for (int k = 0; k < warehouseCount; k++)
                    {
                        var cost = (customer.warehouseCosts[k] + Warehouses[k].setupCost);
                        cost = (cost == 0) ? 1 : cost;
                        tmpCosts[k] = new WarehouseCustomerCost
                        {
                            warehouseId = k,
                            totalCost = 5 / cost
                        };

                        totalCost += tmpCosts[k].totalCost;
                    }

                    for (int k = 0; k < warehouseCount; k++)
                    {
                        tmpCosts[k].selectionProbability = tmpCosts[k].totalCost / totalCost;
                        tmpCosts[k].expectedCopies = (int)Math.Ceiling(tmpCosts[k].selectionProbability * warehouseCount);
                        totalCopies += tmpCosts[k].expectedCopies;
                    }

                    int[] warehouseCandidates = new int[totalCopies];
                    int candidateCounter = 0;

                    for (int k = 0; k < warehouseCount; k++)
                    {
                        for (int j = 0; j < tmpCosts[k].expectedCopies; j++)
                        {
                            warehouseCandidates[candidateCounter] = k;
                            candidateCounter++;
                        }
                    }

                    while (true)
                    {
                        int randomWarehouseIndex = rnd.Next(totalCopies);
                        int selectedWarehouseIndex = warehouseCandidates[randomWarehouseIndex];
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

        void CalculateFitness()
        {
            totalFitness = 0;

            for (int i = 0; i < populationSize; i++)
            {
                Individual ind = Population[i];
                ind.fitness = 0;

                if (IsIndividualFeasible(ind))
                {
                    for (int j = 0; j < customerCount; j++)
                    {
                        int warehouseIndex = ind.genotype[j];

                        if (!ind.warehouseList[warehouseIndex])
                        {
                            ind.warehouseList[warehouseIndex] = true;
                            ind.cost += Warehouses[warehouseIndex].setupCost;
                        }

                        ind.cost += CustomerWarehouseCosts[j, warehouseIndex];
                    }

                    ind.fitness = fitnessCoefficient / ind.cost;
                    totalFitness += ind.fitness;
                }
                else
                {
                    ind.cost = 999999999999;
                    ind.fitness = 0;
                }


            }
        }

        bool IsIndividualFeasible(Individual ind)
        {
            int i = 0;
            bool isFeasible = true;

            foreach (var warehouseIndex in ind.genotype)
            {
                var customer = Customers[i];
                var warehouse = Warehouses[warehouseIndex];

                if (warehouse.remainingCapacity > customer.demand)
                {
                    warehouse.remainingCapacity = warehouse.remainingCapacity - customer.demand;
                }
                else
                {
                    isFeasible = false;
                }

                i++;
            }

            for (i = 0; i < warehouseCount; i++)
            {
                Warehouses[i].remainingCapacity = Warehouses[i].capacity;
            }
            return isFeasible;
        }

        void Reproduction()
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
                //int firstParentIndex = rnd.Next(populationSize);
                //int secondParentIndex = rnd.Next(populationSize);

                //var firstParent = MatingPool[firstParentIndex];
                //var secondParent = MatingPool[secondParentIndex];

                var firstParent = MatingPool[i * 2];
                var secondParent = MatingPool[(i * 2)+ 1];


                double xoverRandom = rnd.NextDouble();

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

                if (xoverRandom > (1 - XOVER_RATE))
                {
                    for (int j = 0; j < customerCount; j++)
                    {
                        double xoverProb = rnd.NextDouble();
                        if (0.5 < xoverProb)
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

        void Mutation()
        {
            for (int i = 0; i < populationSize; i++)
            {
                var individual = Population[i];

                for (int j = 0; j < customerCount; j++)
                {
                    double mutationRandom = rnd.NextDouble();

                    if (mutationRandom > (1 - MUTATION_RATE))
                    {
                        individual.genotype[j] = rnd.Next(warehouseCount);
                    }

                }
            }
        }
    }
}
