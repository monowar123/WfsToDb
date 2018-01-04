using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WfsToDb
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //string endpoint = "http://bgtviewer.chalois.com:8080/geoserver/ACC1/ows?";
            //string layerName = "ACC1:hitbgt";
            //string sortBy = string.Empty; ;
            //string connString = "SERVER=localhost; Port=5432; Database=AO01; User id=postgres; Password=postgres; encoding=unicode;";
            //string tableName = "hitbgt_test";

            try
            {
                //WfsClient client = new WfsClient(endpoint, layerName, sortBy);
                //client.ProcessGeojsonAndInsertIntoDb(connString, tableName);

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            watch.Stop();
            var time = watch.ElapsedMilliseconds / 1000;
            Console.WriteLine(Convert.ToString(time));

            Console.ReadKey();
        }
    }
}
