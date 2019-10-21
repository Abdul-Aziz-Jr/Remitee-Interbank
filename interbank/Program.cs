using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Geocoding;
using Geocoding.Google;


namespace interbank
{

    public class Record
    {
        public string Name {get; set;}
        public string State {get; set;}

        public string City {get; set;}

        public string AddressLine1 {get; set;}

        public string Time {get; set;}

        public string Status {get; set;}

        public double? Latitude {get; set;}

        public double? Longitude {get; set;}

    }

    public class MapInterbakAgents : ClassMap<Record>
    {
        public MapInterbakAgents()
        {
            Map(m => m.Name).Name("Nombre_comercio");
            Map(m => m.State).Name("Provincia");
            Map(m => m.City).Name("Distrito");
            Map(m => m.AddressLine1).Name("Dirección");
             Map(m => m.Status).Name("Status_comercio");
              Map(m => m.Time).Name("Horario_atención");
        }
    }

    public class MapInterbakBranches : ClassMap<Record>
    {
        public MapInterbakBranches()
        {
            Map(m => m.Name).Name("Nombre tienda");
             Map(m => m.State).Name("Estado");
              Map(m => m.City).Name("DirecciónSBS");
               Map(m => m.AddressLine1).Name("Distrito");
                Map(m => m.Status).Name("Provincia");
                Map(m => m.Time).Name("Horario");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var recordsAgentes = Process("/home/dabaj/Documents/maxi/Interbank_Agentes_activos.csv", new MapInterbakAgents());

            var recordsTiendas = Process("/home/dabaj/Documents/maxi/Tiendas_actual.csv", new MapInterbakBranches());

            var records =recordsAgentes.Union(recordsTiendas);

            using (var writer = new StreamWriter("/home/dabaj/Documents/maxi/salida.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }
            
        }

        static List<Record> Process(string sourceFile, ClassMap<Record> fileMap)
        {    
            var apiKey = Environment.GetEnvironmentVariable("APIKEY");

            var geocoder = new GoogleGeocoder() { ApiKey = apiKey };

            using (var reader = new StreamReader(sourceFile))
            using (var csv = new CsvReader(reader))
            { 
                csv.Configuration.RegisterClassMap(fileMap);
                
                var records = csv.GetRecords<Record>().ToList();

                foreach(var record in records)
                {
                   FillCorrdinates(geocoder, record);
                }

                return records;
             }
        }

        static void FillCorrdinates(GoogleGeocoder geocoder, Record record)
        {
            var formattedAddress = $"{record.AddressLine1}, {record.City}, {record.State}";
            IEnumerable<Address> addresses = geocoder.GeocodeAsync(formattedAddress).Result;

            if(addresses.Any())
            {
                record.Longitude = addresses.First().Coordinates.Longitude;
                record.Latitude = addresses.First().Coordinates.Latitude;
            }
        }
    }
}