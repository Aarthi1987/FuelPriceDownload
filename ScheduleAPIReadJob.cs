using EFCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using FuelPriceReadAPI.Models;
using Quartz;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FuelPriceReadAPI
{
    public class SCheduleAPIReadJob : IJob

    {
        private static string APIUrl = "http://api.eia.gov/series/?api_key=ec92aacd6947350dcb894062a4ad2d08&series_id=PET.EMD_EPD2D_PTE_NUS_DPG.W";

        public async Task Execute(IJobExecutionContext context)

        {
            ReadFuelPrices();
            await Task.CompletedTask;
            await Task.CompletedTask;
        }

        public void ReadFuelPrices()
        {
            IConfiguration Config = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json")
            .Build();

            //Ndays value is set in appSettings.json
            

            int Ndays = Convert.ToInt32(Config.GetSection("Ndays").Value);
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type: application/json");
                client.Headers.Add("Accept: application/json");
                try
                {

                    var prices = client.DownloadString(APIUrl);
                    List<FuelPrice> fuelPricesList = new List<FuelPrice>();
                    var apiResponse = JsonConvert.DeserializeObject<Root>(prices);

                    foreach (var i in apiResponse.series[0].data)
                    {
                        DateTime convertedDate;
                        if (DateTime.TryParseExact(Convert.ToString(i[0]),
                        "yyyyMMdd", CultureInfo.InvariantCulture,
                          DateTimeStyles.None,
                        out convertedDate))
                        {
                            //valid
                        }

                        // Add data only if the date is not older than Ndays 
                        if (convertedDate > DateTime.Now.AddDays(-Ndays))
                        {
                            FuelPrice fuelprice = new FuelPrice();
                            fuelprice.Date = Convert.ToString(i[0]);
                            fuelprice.price = Convert.ToDouble(i[1]);
                            fuelPricesList.Add(fuelprice);
                        }
                    }

                    using (var db = new EFContext())
                    {
                        //Delete the Data in the Database to enable loading fresh Data
                        var itemsToDelete = db.Set<FuelPrice>();
                        db.FuelPrices.RemoveRange(itemsToDelete);
                        db.SaveChanges();
                    }

                    {
                        using (var db = new EFContext())
                        {
                            foreach (var i in fuelPricesList)
                            {
                                var fuelPriceDb = db.FuelPrices
                                .Where(c => c.Date == i.Date)
                                .SingleOrDefault();
                                FuelPrice fuelprice = new FuelPrice();
                                fuelprice.Date = i.Date;
                                fuelprice.price = i.price;
                                //To avoid Duplicates
                                if (fuelPriceDb == null)
                                    db.Add(fuelprice);

                            }
                            db.SaveChanges();

                        }
                        Console.WriteLine("Downloading Fuel Prices at " + DateTime.Now);
                    }
                }
                catch(JsonReaderException e)
                {
                    Console.WriteLine("The Data from the API is in incorrect format.{0} Exception caught.", e);
                }
            }
        }

    }
}