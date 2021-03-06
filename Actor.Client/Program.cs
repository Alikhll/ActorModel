﻿using Actor.Contract;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace Actor.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await DoCall();
        }

        static async Task DoCall()
        {
            int count = -1;

            for (int i = 0; i <= 100; i++)
            {
                using (var client = await ConnectClient())
                {
                    try
                    {
                        //await ATM(client);
                        // count = await GetClientWork(client);
                        await SendSms(client);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            Console.WriteLine($"Count number: {count}");
            Console.ReadKey();
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                //localDev
                //.UseLocalhostClustering()

                //Clustering
                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = "Integrated Security=true;Initial Catalog=Orleans1;Server=.";
                    options.Invariant = "System.Data.SqlClient";
                })

                //Streaming
                .AddSimpleMessageStreamProvider("SMSProvider")

                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                })
                .Build();

            await client.Connect();
            return client;
        }

        private static async Task<int> GetClientWork(IClusterClient client)
        {
            var gen = client.GetGrain<IOrderNumberGenerator>("key");
            Console.WriteLine(await gen.GenerateOrderNumber());

            var friend = client.GetGrain<IHello>(1);
            return await friend.GetHello();
        }

        private static async Task ATM(IClusterClient client)
        {
            try
            {
                IATMGrain atm = client.GetGrain<IATMGrain>(0);
                var from = "A";
                var to = "B";

                await atm.Transfer(from, to, 100);

                var fromBalance = await client.GetGrain<IAccountGrain>(from).GetBalance();
                var toBalance = await client.GetGrain<IAccountGrain>(to).GetBalance();

                Console.WriteLine(fromBalance);
                Console.WriteLine(toBalance);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static async Task SendSms(IClusterClient client)
        {
            var guid = Guid.Parse("ef0874b9-4696-4493-bb83-4b184865b957");

            var streamProvider = client.GetStreamProvider("SMSProvider");
            
            var stream = streamProvider.GetStream<int>(guid, "RANDOMDATA");

            await stream.OnNextAsync(new Random().Next(100));
        }

    }
}
