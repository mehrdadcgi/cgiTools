using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Nest;

namespace ConsoleAppES
{
    class Program
    {


        static void Main(string[] args)
        {


            var countRequest = es_helper.esClient().Indices.Get(es_helper.getDefaultIndex());
            long count = countRequest.Indices.Count();

            Console.WriteLine("count indexes : " + count.ToString());

            //  es_helper.createIndex(es_helper.getDefaultIndex());
            Console.WriteLine("Started on: " + DateTime.Now.ToString());

            bool DeleteAndCreate = helper.CBool(helper.GetConfigByKey("DeleteAndCreate"));

            if (DeleteAndCreate)
            {
                es_helper.dropIndex(es_helper.getDefaultIndex()); //orig correct
                es_helper.createIndex(es_helper.getDefaultIndex());
                Console.WriteLine("droped and re-create index: "+es_helper.getDefaultIndex()+" " + DateTime.Now.ToString());
            }
            bool doSDN  = helper.CBool(helper.GetConfigByKey("doSDN_UN"));
            if (doSDN)
            { 
                sancDb_to_es.AddSantionBySource(doSDN);
                //this is from finalSanNames
                Console.WriteLine("Completed finalSanctionNames table : " + DateTime.Now.ToString());

            }
            bool doPepWiki = helper.CBool(helper.GetConfigByKey("doPepWiki"));

            if (doPepWiki)
            {
                wikiPep_to_es.addPepToESbyCountry(doPepWiki);
                Console.WriteLine("Completed wiki Pep : " + DateTime.Now.ToString());

            }

            bool doEu = helper.CBool(helper.GetConfigByKey("doEU"));
            if (doEu)
            {
                new eu_union().addEU();
                Console.WriteLine("Completed EU : " + DateTime.Now.ToString());

            }
            bool doUk = helper.CBool(helper.GetConfigByKey("douk"));
            if(doUk)
            {
                new uk_hm().addUK();
                Console.WriteLine("Completed UK HM : " + DateTime.Now.ToString());

            }
            bool dott = helper.CBool(helper.GetConfigByKey("dott"));
            if (dott)
            {
                new trinidad().addUK();
                Console.WriteLine("Completed tt fiu : " + DateTime.Now.ToString());

            }
            helper.updatLastdate();
             

            Console.WriteLine("Ended all: " + DateTime.Now.ToString());
           // Console.ReadLine();
          
        }

    }
}
