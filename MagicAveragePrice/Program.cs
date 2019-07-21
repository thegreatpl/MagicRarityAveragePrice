using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace MagicAveragePrice
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter 3 character set code");
                var setCode = Console.ReadLine().Trim();
                if (setCode.Length > 3)
                {
                    Console.WriteLine("ERROR: set code is 3 characters long");
                }
                var obj = GetAllCards(setCode);

                var Rarities = GetRarities(obj);

                Console.WriteLine("Average prices for set:");
                foreach (var rar in Rarities.Values)
                {
                    Console.WriteLine($"Rarity:{rar.RarityName}     No:{rar.TotalCards}");
                    Console.WriteLine($"Total USD:{rar.TotalPriceUSD}   Average:{Math.Round(rar.TotalPriceUSD / rar.TotalCards, 2)}");
                    Console.WriteLine($"Total EUR:{rar.TotalPriceEUR}   Average:{Math.Round(rar.TotalPriceEUR / rar.TotalCards, 2)}");
                    Console.WriteLine($"Total TIX:{rar.TotalPriceTIX}   Average:{Math.Round(rar.TotalPriceTIX / rar.TotalCards, 2)}");

                }

            }
        }


        public static Dictionary<string, Rarity> GetRarities(RootObject obj)
        {
            Dictionary<string, Rarity> Rarities = new Dictionary<string, Rarity>();

            foreach (var datum in obj.data)
            {
                if (!Rarities.ContainsKey(datum.rarity))
                {
                    Rarities.Add(datum.rarity, new Rarity()
                    {
                        RarityName = datum.rarity,
                        TotalCards = 0,
                        TotalPriceEUR = 0,
                        TotalPriceTIX = 0,
                        TotalPriceUSD = 0
                    });
                }

                try
                {
                    var rarity = Rarities[datum.rarity];

                    rarity.TotalPriceEUR += Convert.ToDecimal(datum.prices.eur);
                    rarity.TotalPriceUSD += Convert.ToDecimal(datum.prices.usd);
                    rarity.TotalPriceTIX += Convert.ToDecimal(datum.prices.tix);
                    rarity.TotalCards++;

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                    throw;
                }
            }
            return Rarities; 
        }



        public static RootObject GetAllCards(string setName)
        {
            return GetRootObject($"https://api.scryfall.com/cards/search?q=s:{setName}"); 
        }

        public static RootObject GetRootObject(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var stream = new StreamReader(response.GetResponseStream());
                var obj = JsonConvert.DeserializeObject<RootObject>(stream.ReadToEnd()); 
                if (obj.has_more)
                {
                    var nextPage = GetRootObject(obj.next_page);
                    obj.data.AddRange(nextPage.data); 
                }
                return obj; 
            }
            else
                throw new Exception($"Error with conntecting:{response.StatusCode.ToString()}"); 
        }
    }
    class Rarity
    {
        public string RarityName { get; set; }
        public decimal TotalPriceUSD { get; set; }
        public decimal TotalPriceEUR { get; set; }
        /// <summary>
        /// MTGO currency? 
        /// </summary>
        public decimal TotalPriceTIX { get; set; }
        public int TotalCards { get; set; }
    }
}
