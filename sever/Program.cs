using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Severstal
{
    public class Order //Заказ
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public int Quantity { get; set; }
        public DateTime DataOrder { get; set; }
        public bool GotRezka { get; set; } = false;
        public bool GotSklad { get; set; } = false;
        public bool GotUpak { get; set; } = false;

        public Order(string name, int type, int quantity, DateTime dataOrder)
        {
            Name = name;
            Type = type;
            Quantity = quantity;
            DataOrder = dataOrder;
        }
    }

    public interface IProduction
    {
        public static int lastType;

        public static List<Order> orders;
        public static List<Order> rezka = new List<Order>(2);
        public static List<Order> skladyvanie = new List<Order>(3);
        public static List<Order> upakova = new List<Order>(1);


        static async void WorkRezka(Order r)
        {
            await Task.Run(() =>
            {
                r.GotRezka = true;
                rezka.Add(r);
                Thread.Sleep(1000);
                orders.Add(r);
                Console.WriteLine("заказ " + r.Name + " КОНЕЦ РЕЗКИ");
            });
        }

        static async void WorkSklad(Order r)
        {
            await Task.Run(() =>
            {
                skladyvanie.Add(r);
                Thread.Sleep(2000);
                r.GotSklad = true;
                orders.Add(r);
                Console.WriteLine("заказ " + r.Name + " КОНЕЦ СКЛАДЫВАНИЯ");
            });
        }

        static async void WorkUpak(Order r)
        {
            await Task.Run(() =>
            {
                upakova.Add(r);
                Thread.Sleep(1000);
                r.GotUpak = true;
                orders.Add(r);
                lastType = r.Type;
                upakova.RemoveAt(0);
                Console.WriteLine("заказ " + r.Name + " конец УПАКОВКИ");
            });
        }

        public static void Start()
        {
            for (int j = 0; j <= orders.Count - 1; j++)
            {
                for (int i = 0; i <= orders.Count - 1; i++)
                {
                    Thread.Sleep(100);
                    if (rezka.Count < 2 && orders[i].GotRezka == false)
                    {
                        Order order = orders[i];
                        for (int c = 0; c <= orders.Count - 1; c++) //Поиск по мин дате
                        {
                            if (orders[c].DataOrder < order.DataOrder && orders[c].GotRezka == false)
                            {
                                order = orders[c];
                            }
                        }

                        Console.WriteLine("заказ " + order.Name + " поступил на РЕЗКУ");
                        WorkRezka(order);
                    }
                    else if (skladyvanie.Count < 3 && orders[i].GotRezka == true && orders[i].GotSklad == false)
                    {
                        foreach (Order v in rezka)
                        {
                            if (v.GotRezka == true)
                            {
                                rezka.Remove(v);
                                break;
                            }
                        }
                        Console.WriteLine("заказ " + orders[i].Name + " поступил на СКЛАДЫВАНИЕ");
                        WorkSklad(orders[i]);
                    }
                    else if (upakova.Count < 1 && orders[i].GotSklad == true && orders[i].GotUpak == false)
                    {
                        foreach (Order v in skladyvanie)
                        {
                            if (v.GotSklad == true)
                            {
                                skladyvanie.Remove(v);
                                break;
                            }
                        }
                        Order order = orders[i];
                        for (int c = 0; c <= orders.Count - 1; c++) //Поиск оптимального по типу
                        {
                            if (orders[i].Type == lastType && orders[i].GotSklad == true && orders[i].GotUpak == false)
                            {
                                order = orders[i];
                                break;
                            }
                        }
                        Console.WriteLine("заказ " + orders[i].Name + " поступил на УПАКОВКУ");
                        WorkUpak(order);
                    }
                    else
                    {
                        orders.Add(orders[i]);
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Order> ordersss = new List<Order>{
            new Order("1", 3333, 1000, new DateTime(2021,06,08)),
            new Order("2", 4444, 2000, new DateTime(2021,06,05)),
            new Order("3", 3333, 1111, new DateTime(2021,06,04)),
            new Order("4", 2222, 4444, new DateTime(2021,06,16)),
            new Order("5", 2323, 2000, new DateTime(2021,06,15)),
            new Order("6", 2323, 5000, new DateTime(2021,06,03)),
            new Order("7", 2323, 1600, new DateTime(2021,06,12)),
            new Order("8", 2323, 2200, new DateTime(2021,06,11)),
            new Order("9", 2323, 3100, new DateTime(2021,06,10)),
            new Order("10", 1111, 3300, new DateTime(2021,06,08))
            };

            IProduction.orders = ordersss;
            IProduction.Start();

            Console.ReadKey();
        }
    }
}