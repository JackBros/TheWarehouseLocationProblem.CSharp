using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWarehouseLocationProblem.CSharp
{
    public class Warehouse
    {
        public int id;
        public int capacity;
        public int remainingCapacity;
        public double setupCost;
        public bool isSelected;
    }

    public class Customer
    {
        public int id;
        public int demand;
        public int? assignedWarehouseId;
        public double[] warehouseCosts;
        public WarehouseCustomerCost[] assignmentCosts;
        public int[] warehouseCandidates;
    }

    public class Individual
    {
        public int[] genotype;
        public double fitness;
        public bool[] warehouseList;
        public double cost;
    }

    public class Result
    {
        public int generation;
        public Individual individual;
    }

    public class WarehouseCustomerCost
    {
        public int warehouseId;
        public double totalCost;
        public double selectionProbability;
        public int expectedCopies;
    }

    class Program
    {
        static string filePath = "";
        static bool debug = false;
        public static Warehouse[] Warehouses;
        public static Customer[] Customers;
        public static int warehouseCount, customerCount, populationSize = 0;
        public static double fitnessCoefficient = 1;
        public static int MAX_POPULATION_SIZE = 500, MAX_GENERATION_COUNT = 3000, TOURNAMENT_SELECTOR = 2;

        public static double ConvertToDouble(object obj)
        {
            try
            {
                string str = (obj?.ToString() ?? "0");
                double deger = 0;

                str = str.Replace(".", ",");
                if (str.Contains(","))
                {
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

        static void Main(string[] args)
        {
            if (args.Count() == 1)
            {
                filePath = args[0];
            }
            else if (args.Count() == 2)
            {
                filePath = args[0];
                debug = true;
            }
            debug = true;
            filePath = @"data\wl_1000_1";
            GA_I4();
            //filePath = @"data\wl_50_1";
            //GA_Dev();
            //filePath = @"data\wl_200_5";
            //GA_Dev();
            /*if (!string.IsNullOrWhiteSpace(filePath))
            {

                int instanceID = 0;

                switch (instanceID)
                {
                    default:
                    case 0:
                        filePath = @"data\wl_1000_1";
                        GA_Dev();
                        break;
                    case 1:
                        filePath = @"data\wl_25_2";
                        GA_I1();
                        break;
                    case 2:
                        filePath = @"data\wl_100_4";
                        GA_I2();
                        break;
                    case 3:
                        filePath = @"data\wl_500_1";
                        GA_I3();
                        break;
                    case 4:
                        filePath = @"data\wl_1000_1";
                        GA_I4();
                        break;
                    case 5:
                        filePath = @"data\wl_2000_1";
                        GA_I5();
                        break;
                }

                if (debug) Console.ReadKey();
            }*/
        }

        static void GA_Dev()
        {
            ReadFile(filePath);

            MAX_POPULATION_SIZE = 500;
            MAX_GENERATION_COUNT = 3000;
            TOURNAMENT_SELECTOR = 2;

            GA_Dev ga = new GA_Dev();
            ga.customerCount = customerCount;
            ga.warehouseCount = warehouseCount;
            ga.debug = debug;
            ga.fitnessCoefficient = fitnessCoefficient;
            ga.MAX_GENERATION_COUNT = MAX_GENERATION_COUNT;
            ga.MAX_POPULATION_SIZE = MAX_POPULATION_SIZE;
            ga.populationSize = populationSize;
            ga.XOVER_RATE = 1;
            ga.MUTATION_RATE = 0.002685;
            ga.TOURNAMENT_SELECTOR = MAX_POPULATION_SIZE / (20);

            ga.Start();
        }

        static void GA_I1()
        {
            ReadFile(filePath);

            MAX_POPULATION_SIZE = 45000;
            MAX_GENERATION_COUNT = 20;
            TOURNAMENT_SELECTOR = 2;

            GA_I1 ga = new GA_I1();
            ga.customerCount = customerCount;
            ga.warehouseCount = warehouseCount;
            ga.debug = debug;
            ga.fitnessCoefficient = fitnessCoefficient;
            ga.MAX_GENERATION_COUNT = MAX_GENERATION_COUNT;
            ga.MAX_POPULATION_SIZE = MAX_POPULATION_SIZE;
            ga.populationSize = populationSize;
            ga.XOVER_RATE = 1;
            ga.MUTATION_RATE = 0.027;
            ga.TOURNAMENT_SELECTOR = MAX_POPULATION_SIZE / 100;

            ga.Start();
        }

        static void GA_I2()
        {
            ReadFile(filePath);

            MAX_POPULATION_SIZE = 1500;
            MAX_GENERATION_COUNT = 2000;
            TOURNAMENT_SELECTOR = 2;

            GA_I2 ga = new GA_I2();
            ga.customerCount = customerCount;
            ga.warehouseCount = warehouseCount;
            ga.debug = debug;
            ga.fitnessCoefficient = fitnessCoefficient;
            ga.MAX_GENERATION_COUNT = MAX_GENERATION_COUNT;
            ga.MAX_POPULATION_SIZE = MAX_POPULATION_SIZE;
            ga.populationSize = populationSize;
            ga.XOVER_RATE = 1;
            ga.MUTATION_RATE = 0.0017502685;
            ga.TOURNAMENT_SELECTOR = MAX_POPULATION_SIZE / (150);

            ga.Start();
        }

        static void GA_I3()
        {
            ReadFile(filePath);

            MAX_POPULATION_SIZE = 10000;
            MAX_GENERATION_COUNT = 50;
            TOURNAMENT_SELECTOR = 2;

            GA_I3 ga = new GA_I3();
            ga.customerCount = customerCount;
            ga.warehouseCount = warehouseCount;
            ga.debug = debug;
            ga.fitnessCoefficient = fitnessCoefficient;
            ga.MAX_GENERATION_COUNT = MAX_GENERATION_COUNT;
            ga.MAX_POPULATION_SIZE = MAX_POPULATION_SIZE;
            ga.populationSize = populationSize;
            ga.XOVER_RATE = 1;
            ga.MUTATION_RATE = 0.00685;
            ga.TOURNAMENT_SELECTOR = MAX_POPULATION_SIZE / (200);

            ga.Start();
        }

        static void GA_I4()
        {
            ReadFile(filePath);

            MAX_POPULATION_SIZE = 500;
            MAX_GENERATION_COUNT = 3000;
            TOURNAMENT_SELECTOR = 2;

            GA_I4 ga = new GA_I4();
            ga.customerCount = customerCount;
            ga.warehouseCount = warehouseCount;
            ga.debug = debug;
            ga.fitnessCoefficient = fitnessCoefficient;
            ga.MAX_GENERATION_COUNT = MAX_GENERATION_COUNT;
            ga.MAX_POPULATION_SIZE = MAX_POPULATION_SIZE;
            ga.populationSize = populationSize;
            ga.XOVER_RATE = 1;
            ga.MUTATION_RATE = 0.002685;
            ga.TOURNAMENT_SELECTOR = MAX_POPULATION_SIZE / (20);

            ga.Start();
        }

        static void GA_I5()
        {
            ReadFile(filePath);

            MAX_POPULATION_SIZE = 300;
            MAX_GENERATION_COUNT = 14229;
            TOURNAMENT_SELECTOR = 2;

            GA_I5 ga = new GA_I5();
            ga.customerCount = customerCount;
            ga.warehouseCount = warehouseCount;
            ga.debug = debug;
            ga.fitnessCoefficient = fitnessCoefficient;
            ga.MAX_GENERATION_COUNT = MAX_GENERATION_COUNT;
            ga.MAX_POPULATION_SIZE = MAX_POPULATION_SIZE;
            ga.populationSize = populationSize;
            ga.XOVER_RATE = 1;
            ga.MUTATION_RATE = 0.001;
            ga.TOURNAMENT_SELECTOR = MAX_POPULATION_SIZE / (30);

            ga.Start();
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
                    if (items.Count() == 2)
                    {
                        warehouseCount = Convert.ToInt32(items[0]);
                        customerCount = Convert.ToInt32(items[1]);

                        double populationConstant = Math.Pow(customerCount, warehouseCount) / 2;
                        try
                        {
                            populationSize = Convert.ToInt32(Math.Min(populationConstant, MAX_POPULATION_SIZE));
                        }
                        catch
                        {
                            populationSize = MAX_POPULATION_SIZE;
                        }

                        if (populationSize % 2 == 1) populationSize++;
                        fitnessCoefficient = 10 * Math.Pow(populationSize, 2);

                        Warehouses = new Warehouse[warehouseCount];
                        Customers = new Customer[customerCount];
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
                                assignedWarehouseId = null,
                                warehouseCosts = new double[warehouseCount]
                            };
                        }
                    }
                    else
                    {
                        if (items.Count() == warehouseCount)
                        {
                            for (int j = 0; j < warehouseCount; j++)
                            {
                                Customers[customerIndexCounter].warehouseCosts[j] = ConvertToDouble(items[j]);
                            }
                            customerIndexCounter++;
                        }
                    }

                    customerLineChecker = !customerLineChecker;
                }

                i++;
            }
        }
    }
}
