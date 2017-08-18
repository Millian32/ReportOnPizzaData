using Newtonsoft.Json;
using ReportOnPizzaData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace ReportOnPizzaData
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string pizzaToppingUrl = "http://files.olo.com/pizzas.json";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(pizzaToppingUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.GetAsync(pizzaToppingUrl).Result;
                var pizzaOrders = JsonConvert.DeserializeObject<List<PizzaOrders>>(response.Content.ReadAsStringAsync().Result);

                List<Topping> toppings = new List<Topping>();
                foreach (var rec in pizzaOrders)
                {
                    var orderedToppings = rec.Toppings.OrderBy(t => t);  //need to order the toppings to make sure we dont count the same toppings as a different topping selection!!!
                    StringBuilder toppingsString = new StringBuilder();

                    foreach (var order in orderedToppings)
                    {
                        toppingsString.Append(order + ", ");
                    }

                    toppings.Add(new Topping() { ToppingName = toppingsString.ToString().Trim().TrimEnd(',') });
                }

                var distinctToppingSelections = (from order in toppings
                                                 select order.ToppingName).Distinct().OrderBy(a => a);

                var toppingCounts = new List<Topping>();
                foreach (var selections in distinctToppingSelections)
                {
                    toppingCounts.Add(new Topping()
                    {
                        ToppingName = selections,
                        ToppingCount = toppings.Where(z => z.ToppingName == selections).Count()
                    });
                };

                var top20 = (from toppingCount in toppingCounts
                             orderby toppingCount.ToppingCount descending
                             select toppingCount).Take(20);

                var rank = 1;
                foreach (var record in top20)
                {
                    Console.WriteLine("Rank: " + rank + "    " + "Count: " + record.ToppingCount + "    " + "Toppings: " + record.ToppingName);
                    rank++;
                }

                Thread.Sleep(10000);
            }
        }
    }
}