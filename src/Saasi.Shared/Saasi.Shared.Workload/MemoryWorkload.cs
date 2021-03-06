﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Saasi.Shared.Workload
{
    public class MemoryWorkload : IWorkload
    {
        public async Task<ExecutionResult> Run(int round)
        {
            var startTime = DateTime.Now;
            var exceptions = false;
            string result = "";
            try {
                result = await XMLManipulation(round);
            } catch (Exception e) {
                exceptions = true;
                Console.WriteLine(e.ToString());
            }

            return new ExecutionResult {
                HasExceptions = exceptions,
                TaskFinishedAt = System.DateTime.Now,
                TaskStartedAt = startTime,
                ThreadOfExecution = Thread.CurrentThread.GetHashCode().ToString(),
                ExecutedLoops = round,
                Payload = result
            };
        }

        public async Task<string> XMLManipulation(int round)
        {
            
            // simulate memory use 
            var url = "https://news.ycombinator.com/";
            var web = new HtmlWeb();
            HtmlDocument doc;
            try {
                doc = await web.LoadFromWebAsync(url);
            } catch {
                var assembly = Assembly.GetExecutingAssembly();
                string[] names = assembly.GetManifestResourceNames(); 
                var resourceStream = assembly.GetManifestResourceStream("Saasi.Shared.Workload.Data.hn.html");
                doc = new HtmlDocument();
                doc.Load(resourceStream);
            }
            var result = new List<object>();
            for (var i = 0; i < round; ++i) {
                var mainTable = doc.DocumentNode.Descendants("table")
                    .Where(x => x.Attributes["id"].Value == "hnmain")
                    .First();

                var itemList = mainTable.Descendants("table")
                    .Where(x => x.Attributes["class"]?.Value  == "itemlist")
                    .First()
                    .Descendants("tr")
                    .ToList();
                var resultList = itemList
                    .Select(x => {
                            var link = x.Descendants("a")
                                .Where(a => a.Attributes["class"]?.Value == "storylink")
                                .FirstOrDefault();
                            if (link!= null) {
                                return new {
                                    Url = link.Attributes["href"].Value,
                                    Title = link.InnerText
                                };
                            }  else
                            {
                                return null;
                            } 
                        })
                    .ToList()
                    .Where(x => x != null)
                    .ToList();
                result.Add(resultList);
            }
            return JsonConvert.SerializeObject(result);

        }
    }
}
