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
        private const string PizzaToppingUrl = "http://files.olo.com/pizzas.json";  //current url tograb json data, this should be the only place you need to change this value
        private const int TopToppingCounts = 20; //currently we are looking for the top 20, this should be the only place you need to change this value

        private static void Main(string[] args)
        {
            var httpResponseMessage = MakeHttpCall(PizzaToppingUrl);
            if (httpResponseMessage == null) { return; }

            var pizzaOrders = JsonConvert.DeserializeObject<List<PizzaOrders>>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            var toppings = OrderToppings(pizzaOrders);
            var toppingCounts = CreateDistinctToppingList(toppings);
            var topToppingCounts = FindTopToppingCounts(toppingCounts);

            PrintToppingCounts(topToppingCounts);
        }

        private static void PrintToppingCounts(IEnumerable<Topping> topToppings)
        {
            var rank = 1;
            foreach (var record in topToppings)
            {
                Console.WriteLine("Rank: " + rank + "    " + "Count: " + record.ToppingCount + "    " + "Toppings: " + record.ToppingName);
                rank++;
            }

            Thread.Sleep(10000);
        }

        private static IEnumerable<Topping> FindTopToppingCounts(IEnumerable<Topping> toppingCounts)
        {
            var topToppingCounts = (from toppingCount in toppingCounts
                                    orderby toppingCount.ToppingCount descending
                                    select toppingCount).Take(TopToppingCounts);

            if (topToppingCounts == null) { Console.WriteLine("ERROR FINDING TOPPING COUNTS."); }

            return topToppingCounts;
        }

        private static IEnumerable<Topping> CreateDistinctToppingList(IReadOnlyCollection<Topping> toppings)
        {
            var distinctToppingSelections = (from order in toppings select order.ToppingName).Distinct().OrderBy(a => a);

            var toppingCounts = distinctToppingSelections.Select(selections => new Topping
            {
                ToppingName = selections,
                ToppingCount = toppings.Count(z => z.ToppingName == selections)
            }).ToList();

            return toppingCounts;
        }

        private static HttpResponseMessage MakeHttpCall(string apiUrl)
        {
            var httpResponseMessage = new HttpResponseMessage();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        httpResponseMessage = client.GetAsync(apiUrl).Result;
                    }
                    httpResponseMessage = client.GetAsync(apiUrl).Result;
                }
                catch (Exception)
                {
                    Console.WriteLine("AN ERROR OCCURRED TRYING TO CONTACT THE SERVER.");
                    Thread.Sleep(10000);

                    return null;
                }

                return httpResponseMessage;
            }
        }

        private static List<Topping> OrderToppings(IEnumerable<PizzaOrders> pizzaOrders)
        {
            var toppings = new List<Topping>();
            foreach (var rec in pizzaOrders)
            {
                var orderedToppings = rec.Toppings.OrderBy(t => t);  //need to order the toppings to make sure we dont count the same toppings as a different topping selection!!!
                var toppingsString = new StringBuilder();

                foreach (var order in orderedToppings)
                {
                    toppingsString.Append(order + ", ");
                }

                toppings.Add(new Topping { ToppingName = toppingsString.ToString().Trim().TrimEnd(',') });
            }

            return toppings;
        }
    }
}