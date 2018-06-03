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
//        static string filePath = "";
//        static bool debug = false;
//        public static GA.Warehouse[] Warehouses;
//        public static GA.Customer[] Customers;
//        public static double[,] CustomerWarehouseCosts;
//        public static int warehouseCount, customerCount, populationSize = 0;
//        public static double fitnessCoefficient = 1;
//        public static int MAX_POPULATION_SIZE = 35000, MAX_GENERATION_COUNT = 350;

//        class osman
//        {
//            public int i, j = 0;
//            public double k = 0;
//            public GA.Result result;
//        }

//        static List<osman> sonuc;

//        static void Main(string[] args)
//        {
//            try
//            {
//                sonuc = new List<osman>();
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
//                    //public static int MAX_POPULATION_SIZE = 27312, MAX_GENERATION_COUNT = 350

//                    GA ga = new GA
//                    {
//                        customerCount = customerCount,
//                        Customers = Customers,
//                        CustomerWarehouseCosts = CustomerWarehouseCosts,
//                        debug = false,
//                        fitnessCoefficient = fitnessCoefficient,
//                        MAX_GENERATION_COUNT = 300,
//                        MAX_POPULATION_SIZE = 22000,
//                        populationSize = populationSize,
//                        warehouseCount = warehouseCount,
//                        Warehouses = Warehouses,
//                        XOVER_RATE = 1
//                    };
//                    var osm = new osman
//                    {
//                        i = 22000,
//                        j = 350,
//                        k = 0.5,
//                        result = ga.Start()
//                    };
//                    //sonuc.Add(osm);
//                    //Console.WriteLine(osm.result.individual.cost.ToString().Replace(',', '.'));// + " i : " + i + " j : " + J + " k : " + k);



//                    //for (int i = 10000; i <= 20000; i += 2000)
//                    //{
//                    //    for (int J = 100; J <= 300; J += 50)
//                    //    {
//                    //        for (double k = 0.5; k <= 1; k += 0.1)
//                    //        {
//                    //            Task.Run(() =>
//                    //            {
//                    //                GA ga = new GA
//                    //                {
//                    //                    customerCount = customerCount,
//                    //                    Customers = Customers,
//                    //                    CustomerWarehouseCosts = CustomerWarehouseCosts,
//                    //                    debug = true,
//                    //                    fitnessCoefficient = fitnessCoefficient,
//                    //                    MAX_GENERATION_COUNT = J,
//                    //                    MAX_POPULATION_SIZE = i,
//                    //                    populationSize = populationSize,
//                    //                    warehouseCount = warehouseCount,
//                    //                    Warehouses = Warehouses,
//                    //                    XOVER_RATE = k
//                    //                };
//                    //                var osm = new osman
//                    //                {
//                    //                    i = i,
//                    //                    j = J,
//                    //                    k = k,
//                    //                    result = ga.Start()
//                    //                };
//                    //                sonuc.Add(osm);
//                    //                Console.WriteLine(osm.result.individual.cost.ToString().Replace(',', '.') + " i : " + i + " j : " + J + " k : " + k);
//                    //            });
//                    //        }
//                    //    }
//                    //}


//                    if (debug) Console.ReadKey();
//                }
//            }
//            catch (Exception exc)
//            {
//                File.WriteAllText("exc.txt", exc.Message);
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

//                        Warehouses = new GA.Warehouse[warehouseCount];
//                        Customers = new GA.Customer[customerCount];
//                        CustomerWarehouseCosts = new double[customerCount, warehouseCount];
//                    }
//                }
//                else if (i > 0 && i <= warehouseCount)
//                {
//                    if (items.Count() == 2)
//                    {
//                        Warehouses[i - 1] = new GA.Warehouse
//                        {
//                            id = i - 1,
//                            capacity = Convert.ToInt32(items[0]),
//                            remainingCapacity = Convert.ToInt32(items[0]),
//                            setupCost = GA.ConvertToDouble(items[1]),
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
//                            Customers[customerIndexCounter] = new GA.Customer
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
//                                CustomerWarehouseCosts[customerIndexCounter, j] = GA.ConvertToDouble(items[j]);
//                                Customers[customerIndexCounter].warehouseCosts[j] = GA.ConvertToDouble(items[j]);
//                            }
//                            customerIndexCounter++;
//                        }
//                    }

//                    customerLineChecker = !customerLineChecker;
//                }

//                i++;
//            }
//        }
//    }
//}
