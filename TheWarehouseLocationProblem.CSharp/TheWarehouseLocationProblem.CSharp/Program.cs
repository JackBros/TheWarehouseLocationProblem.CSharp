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
    }

    public class Individual
    {
        public int[] genotype;
        public double fitness;
        public double selectionProbability;
        public int expectedCopies;
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
        public static double[,] CustomerWarehouseCosts;
        public static int warehouseCount, customerCount, populationSize = 0;
        public static double fitnessCoefficient = 1;
        public static int MAX_POPULATION_SIZE = 35000, MAX_GENERATION_COUNT = 20000;

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
                File.WriteAllText("out.txt", filePath);
            }
            else if (args.Count() == 2)
            {
                filePath = args[0];
                debug = true;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                ReadFile(filePath);

                ACO aco = new ACO()
                {
                    customerCount = customerCount,
                    debug = true,
                    MAX_GENERATION_COUNT = MAX_GENERATION_COUNT,
                    warehouseCount = warehouseCount,
                };
                aco.Start();

                if (debug) Console.ReadKey();
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
                        fitnessCoefficient = Math.Pow(populationSize, 2);

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
                                assignedWarehouseId = null,
                                warehouseCosts = new double[warehouseCount]
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
