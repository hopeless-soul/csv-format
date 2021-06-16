using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Globalization;

/* 
 * REQUIREMENTS: nuget: CsvHelper
 * Install: Install-Package CsvHelper -Version 27.1.0
 *          or
 *          dotnet add package CsvHelper --version 27.1.0            
 * 
 * Была проблема с выгрузкой на Git так что выкладываю .cs файл спрограммой.
 *
 * CSV файл для парсинга должен лежать в папке с исполняемым файлом, туда же будет сгенерирован новый файл "new.csv"
 *
 */

namespace csv_parser_01
{
    public class Worker_record
    {
        [CsvHelper.Configuration.Attributes.Index(0)]
        public string Name { get; set; }

        private DateTime _date;

        [CsvHelper.Configuration.Attributes.Index(1)]
        public DateTime Date
        {
            get { return this._date; }
            set { _date = Convert.ToDateTime(value); }
        }
        
        [CsvHelper.Configuration.Attributes.Index(2)]
        public float Hours { get; set; }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            IEnumerable<Worker_record> records;
            using (var reader = new StreamReader("acme_worksheet.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<Worker_record>().ToList();
            }

            //# Extract Dates && Select Uniqe
            IEnumerable<DateTime> dates_unique = Enumerable.Empty<DateTime>();
            IEnumerable<string> names_unique = Enumerable.Empty<string>();
            foreach (var worker in records)
            {
                dates_unique = dates_unique.Concat(new[] { worker.Date });
                names_unique = names_unique.Concat(new[] { worker.Name });
            }
            dates_unique = dates_unique.Distinct();
            names_unique = names_unique.Distinct();

            //# Get work hours
            Dictionary<string, List<float>> dWorkerWH = new Dictionary<string, List<float>>();
            List<List<float>> workers_wh= new List<List<float>>();

            foreach (var name in names_unique)
            {
                dWorkerWH.Add(name, foo(name, dates_unique, records));
            }

            //# Forming CSV text
            string new_csv_text = "Name \\ Date";
            
            //> Write Headers
            foreach (var day in dates_unique)
                new_csv_text += "," + day.ToString("d");
            new_csv_text += "\n";

            //> Write (Name and work hours) rows
            foreach (var name in names_unique.OrderBy(i => i))
            {
                new_csv_text += name;
                foreach (var wh in dWorkerWH[name])
                    new_csv_text += "," + Convert.ToString(wh);
                new_csv_text += "\n";
            }

            //# Write a new CSV
            File.WriteAllText(".\\new.csv", new_csv_text);








        }

        private static List<float> foo(string name, IEnumerable<DateTime> dates, IEnumerable<Worker_record> workers)
        {
            List<float> temp = new List<float>();
            foreach (var day in dates)
            {
                var worker_byDay = from worker in workers
                                   where worker.Name == name && worker.Date == day
                                   select worker;
                try
                { temp.Add(worker_byDay.First().Hours); }
                catch (Exception)
                { temp.Add(0); }
            }
            return temp;
        }


    }
}
