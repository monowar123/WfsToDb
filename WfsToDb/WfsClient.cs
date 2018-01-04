using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WfsToDb
{
    public class WfsClient
    {
        string uri = string.Empty;
        long totalFeatures = 0;
        long startIndex = 0;
        int maxFeatures = 100;

        public WfsClient(string endPoint)
        {
            uri = endPoint;
        }

        public WfsClient(string endPoint, string layerName, string sortBy)
        {
            uri = endPoint + string.Format("service=WFS&version=1.0.0&request=GetFeature&typeName={0}&startindex={1}&maxFeatures={2}&sortBy={3}&outputFormat=application/json", layerName, startIndex, maxFeatures, sortBy);
        }

        private string GetGeoJsonData()
        {
            string response = string.Empty;        
            using (var client = new WebClient())
            {
                response = client.DownloadString(uri);          
            }
            
            return response;
        }

        private async Task<string> GetGeoJsonDataAsync()
        {
            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(uri);
            }
        }

        public void ProcessGeojsonAndInsertIntoDb(string connectionString, string tableName)
        {    
            do
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                uri = Regex.Replace(uri, @"startindex=\d+", "startindex=" + startIndex);

                Console.WriteLine("Getting Features...{0}--{1}", startIndex + 1, startIndex + maxFeatures);

                string geoJsonData = GetGeoJsonData();
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(geoJsonData);

                if (totalFeatures == 0)
                {
                    totalFeatures = Convert.ToInt64(jsonObject.totalFeatures.ToString());
                    Console.WriteLine("Total Features : {0}", totalFeatures);
                }

                List<List<NpgsqlParameter>> parameterDictionary = new List<List<NpgsqlParameter>>();
                List<NpgsqlParameter> parameter;
                string insertString = string.Empty;
                string collString = string.Empty;
                string valueString = string.Empty;
                string geometry_name = string.Empty;
                bool stringFlug = true;

                foreach (var features in jsonObject.features)
                {
                    parameter = new List<NpgsqlParameter>();
      
                    //create wkt from geometry coordinates
                    string pattern = @"\[([-+]?\d+(\.\d+)?)\,([-+]?\d+(\.\d+)?)\]";
                    string replacement = "$1 $3";
                    string wkt = features.geometry.type + Convert.ToString(features.geometry.coordinates);
                    wkt = wkt.Replace("\r\n", "").Replace(" ", "");
                    wkt = Regex.Replace(wkt, pattern, replacement);
                    wkt = wkt.Replace("[", "(").Replace("]", ")");

                    //loop into properties                  
                    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(features.properties))
                    {
                        string name = descriptor.Name;
                        object value = descriptor.GetValue(features.properties);
                        //Console.WriteLine("{0}={1}", name, value);

                        parameter.Add(new NpgsqlParameter("@" + name, value.ToString() == string.Empty ? null : value.ToString()));

                        if (stringFlug)
                        {
                            collString += name + ", ";
                            valueString += "@" + name + ", ";                         
                        }
                    }

                    if (stringFlug)
                    {
                        geometry_name = features.geometry_name;
                        stringFlug = false;
                    }

                    parameter.Add(new NpgsqlParameter("@" + geometry_name, wkt));         
                    parameterDictionary.Add(parameter);

                    //DbHandler dbHandler = new DbHandler(connectionString);
                    //dbHandler.ExecuteNonQueryWithParameter(insertString, parameter);                    
                }

                insertString = string.Format(@"INSERT INTO {0} ({1}{3}) VALUES ({2}st_geomfromtext(@{3}, 28992));", tableName, collString, valueString, geometry_name);
                
                DbHandler dbHandler = new DbHandler(connectionString);
                dbHandler.ExecuteNonQueryWithParameter(insertString, parameterDictionary);  

                Console.WriteLine("Rows inserted : {0}--{1}", startIndex + 1, startIndex + maxFeatures);

                watch.Stop();
                var time = watch.ElapsedMilliseconds / 1000;
                Console.WriteLine(Convert.ToString(time));

            } while ((startIndex += maxFeatures) < totalFeatures);
 
        }
    }
}
