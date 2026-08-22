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

            helper.OutMsg("count indexes : " + count.ToString());

            //  es_helper.createIndex(es_helper.getDefaultIndex());
            helper.OutMsg("Started on: " + DateTime.Now.ToString());

            helper.OutMsg("Running clean_STG_SDN ... " + DateTime.Now);
            new DbClass().RunStoredProcNoQuery("clean_STG_SDN", new Dictionary<string, object>());
            helper.OutMsg("Completed clean_STG_SDN : " + DateTime.Now);

            bool DeleteAndCreate = helper.CBool(helper.GetConfigByKey("DeleteAndCreate"));

            if (DeleteAndCreate)
            {
              //  es_helper.dropIndex(es_helper.getDefaultIndex()); //orig correct
              //  es_helper.createIndex(es_helper.getDefaultIndex());
                helper.OutMsg("droped and re-create index: "+es_helper.getDefaultIndex()+" " + DateTime.Now.ToString());
            }
            bool doSDN = helper.CBool(helper.GetConfigByKey("doSDN"));
            if (doSDN)
            {
                es_helper.DeletByListISourceDES(7);
                helper.OutMsg("Delete SDN data : 7" + DateTime.Now.ToString());

                sancDb_to_es.AddSantionBySource(doSDN);
                helper.OutMsg("Completed SDN : " + DateTime.Now.ToString());
            }

            bool doUN = helper.CBool(helper.GetConfigByKey("doUN"));
            if (doUN)
            {
                es_helper.DeletByListISourceDES(4);
                helper.OutMsg("Delete UN data : 4" + DateTime.Now.ToString());

                un_list_to_es.AddUnListToEs(doUN);
                helper.OutMsg("Completed UN list 4: " + DateTime.Now.ToString());
            }


            bool doEu = helper.CBool(helper.GetConfigByKey("doEU"));
            if (doEu)
            {
                es_helper.DeletByListISourceDES(10);
                helper.OutMsg("Delete UK data 10 : " + DateTime.Now.ToString());

                new eu_union().addEU();
                helper.OutMsg("Completed EU 10 :  " + DateTime.Now.ToString());

            }
            bool doUk = helper.CBool(helper.GetConfigByKey("douk"));
            if(doUk)
            {
                es_helper.DeletByListISourceDES(5);
                helper.OutMsg("Delete UK data 5 : " + DateTime.Now.ToString());

                new uk_hm().addUK();
                helper.OutMsg("Completed UK HM 5: " + DateTime.Now.ToString());

            }
            bool doPepWiki = helper.CBool(helper.GetConfigByKey("doPepWiki"));
            if (doPepWiki)
            {
                es_helper.DeletByListISourceDES(14);
                helper.OutMsg("Delete UK data 14 : " + DateTime.Now.ToString());

                wikiPep_to_es.addPepToESbyCountry(doPepWiki);
                helper.OutMsg("Completed wiki Pep : " + DateTime.Now.ToString());

            }
            helper.updatLastdate();

            
            helper.OutMsg("Ended all: " + DateTime.Now.ToString());
            bool isDebug = helper.CBool(helper.GetConfigByKey("isDebug"));
            if(isDebug)
            Console.ReadLine();
          
        }

    }
}
