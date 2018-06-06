using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWarehouseLocationProblem.CSharp
{
    public class GA_I1
    {
        Individual[] Population;
        Individual[] NewGeneration;

        const int RANDOM_SEED = 3758580;
        public double XOVER_RATE = 0;
        public double MUTATION_RATE = 0;
        public int TOURNAMENT_SELECTOR = 0;
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
            Console.Write((bestRun.individual.cost).ToString().Replace(',', '.'));
            Console.WriteLine();
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
            while (generationCount < MAX_GENERATION_COUNT)
            {
                CalculateFitness();

                var orderedPop = Population.OrderByDescending(o => o.fitness);
                var bestIndividual = orderedPop.FirstOrDefault();
                var worstIndividual = orderedPop.LastOrDefault();

                results.Add(new Result
                {
                    generation = generationCount,
                    individual = bestIndividual
                });

                if (debug)
                {
                    Console.WriteLine(string.Format("MAXG : {6} - POPS : {7} // Gen {0} : MinC({1}) - MaxC ({2}) // BF ({3}) - WF ({4}) // TF ({5})", generationCount, bestIndividual.cost, worstIndividual.cost, bestIndividual.fitness, worstIndividual.fitness, totalFitness, MAX_GENERATION_COUNT, MAX_POPULATION_SIZE));
                }

                Reproduction();
                if (MUTATION_RATE > 0) Mutation();
                generationCount++;
            }


            CalculateFitness();

            results.Add(new Result
            {
                generation = generationCount,
                individual = Population.OrderByDescending(o => o.fitness).FirstOrDefault()
            });

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
            NewGeneration = new Individual[populationSize];

            for (int i = 0; i < customerCount; i++)
            {
                Program.Customers[i].assignmentCosts = new WarehouseCustomerCost[warehouseCount];
                double totalCost = 0;
                int totalCopies = 0;

                for (int k = 0; k < warehouseCount; k++)
                {
                    var cost = (Program.Customers[i].warehouseCosts[k]);
                    cost = (cost == 0) ? 1 : cost;
                    Program.Customers[i].assignmentCosts[k] = new WarehouseCustomerCost
                    {
                        warehouseId = k,
                        totalCost = fitnessCoefficient / cost
                    };

                    totalCost += Program.Customers[i].assignmentCosts[k].totalCost;
                }

                for (int k = 0; k < warehouseCount; k++)
                {
                    Program.Customers[i].assignmentCosts[k].selectionProbability = Program.Customers[i].assignmentCosts[k].totalCost / totalCost;
                    Program.Customers[i].assignmentCosts[k].expectedCopies = (int)Math.Ceiling(Program.Customers[i].assignmentCosts[k].selectionProbability * warehouseCount);
                    totalCopies += Program.Customers[i].assignmentCosts[k].expectedCopies;
                }

                Program.Customers[i].warehouseCandidates = new int[totalCopies];
                int candidateCounter = 0;

                for (int k = 0; k < warehouseCount; k++)
                {
                    for (int j = 0; j < Program.Customers[i].assignmentCosts[k].expectedCopies; j++)
                    {
                        Program.Customers[i].warehouseCandidates[candidateCounter] = k;
                        candidateCounter++;
                    }
                }
            }

            for (int i = 0; i < populationSize; i++)
            {
                Population[i] = new Individual
                {
                    genotype = new int[customerCount],
                    fitness = 0,
                    warehouseList = new bool[warehouseCount],
                    cost = 0
                };

                for (int j = 0; j < customerCount; j++)
                {
                    while (true)
                    {
                        int randomWarehouseIndex = rnd.Next(Program.Customers[j].warehouseCandidates.Length);
                        int selectedWarehouseIndex = Program.Customers[j].warehouseCandidates[randomWarehouseIndex];
                        Warehouse selectedWarehouse = Program.Warehouses[selectedWarehouseIndex];

                        if (selectedWarehouse.remainingCapacity > Program.Customers[j].demand)
                        {
                            Population[i].genotype[j] = selectedWarehouseIndex;
                            Population[i].warehouseList[selectedWarehouseIndex] = true;
                            selectedWarehouse.remainingCapacity -= Program.Customers[j].demand;
                            break;
                        }

                    }
                }

                for (int j = 0; j < warehouseCount; j++)
                {
                    Program.Warehouses[j].remainingCapacity = Program.Warehouses[j].capacity;
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
                        ind.cost += Program.Customers[j].warehouseCosts[warehouseIndex];
                    }

                    for (int k = 0; k < warehouseCount; k++)
                    {
                        if (ind.warehouseList[k])
                        {
                            ind.cost += Program.Warehouses[k].setupCost;
                        }
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
            bool isFeasible = true;
            int i = 0;

            foreach (var warehouseIndex in ind.genotype)
            {
                var customer = Program.Customers[i];
                var warehouse = Program.Warehouses[warehouseIndex];

                if (warehouse.remainingCapacity > customer.demand)
                {
                    warehouse.remainingCapacity -= customer.demand;
                }
                else
                {
                    isFeasible = false;
                }

                i++;
            }

            for (i = 0; i < warehouseCount; i++)
            {
                Program.Warehouses[i].remainingCapacity = Program.Warehouses[i].capacity;
            }

            return isFeasible;
        }

        void Reproduction()
        {
            var orderedList = Population.OrderByDescending(o => o.fitness).ToList();
            for (int i = 0; i < populationSize / 2; i++)
            {
                XOver(orderedList[rnd.Next(populationSize / TOURNAMENT_SELECTOR)], orderedList[rnd.Next(populationSize / TOURNAMENT_SELECTOR)], i);
            }
            Population = NewGeneration;
        }

        void XOver(Individual firstParent, Individual secondParent, int index)
        {
            var firstChild = new Individual
            {
                genotype = new int[customerCount],
                fitness = 0,
                warehouseList = new bool[warehouseCount],
                cost = 0
            };
            var secondChild = new Individual
            {
                genotype = new int[customerCount],
                fitness = 0,
                warehouseList = new bool[warehouseCount],
                cost = 0
            };

            double xoverRandom = rnd.NextDouble();
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

                for (int j = 0; j < customerCount; j++)
                {
                    firstChild.warehouseList[firstChild.genotype[j]] = true;
                    secondChild.warehouseList[secondChild.genotype[j]] = true;
                }

                NewGeneration[index * 2] = firstChild;
                NewGeneration[(index * 2) + 1] = secondChild;
            }
            else
            {
                NewGeneration[index * 2] = firstParent;
                NewGeneration[(index * 2) + 1] = secondParent;
            }
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

                individual.warehouseList = new bool[warehouseCount];

                for (int j = 0; j < customerCount; j++)
                {
                    individual.warehouseList[individual.genotype[j]] = true;
                }
            }
        }
    }
}
