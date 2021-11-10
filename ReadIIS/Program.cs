
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Web.Administration;

namespace ReadIIS
{
    class Program
    {
        static void Main(string[] args)
        {
            var numberOfThreads = ConfigurationManager.AppSettings["NumberOfThreads"];
            var outputPath = ConfigurationManager.AppSettings["OutputPath"];
            int numberOfThreadsParsed;
            if (!int.TryParse(numberOfThreads, out numberOfThreadsParsed))
            {
                numberOfThreadsParsed = 50;
            }

            var iisManager = new ServerManager();
            SiteCollection sites = iisManager.Sites;

            ConcurrentBag<Data> datas = new ConcurrentBag<Data>();
            Parallel.ForEach(sites, new ParallelOptions() { MaxDegreeOfParallelism = numberOfThreadsParsed }, s =>
            {
                var counter = new PerformanceCounter($"web service", "current connections", s.Name);
                datas.Add(new Data() { SiteName = s.Name, CurrentConnections = counter.RawValue });

                Console.WriteLine($"Site:{s.Name} Connections: {counter.RawValue}");
            });

            using (var writer = new StreamWriter(Path.Combine(outputPath, "result.csv")))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(datas);
            }

            Console.WriteLine("Completed");
            Console.ReadLine();
        }
    }

    class Data
    {
        public string SiteName { get; set; }
        public long CurrentConnections { get; set; }
    }
}
