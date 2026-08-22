using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace ConsoleAppES
{
    public static class sanc_to_es
    {
        public static string GetJsonSanc()
        {
            string sanUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            sanUrl = sanUrl.Replace("$$", "&");
            StringBuilder sb = new StringBuilder();
            int count = 0;
            using (var webClient = new WebClient())
            using (var stream = webClient.OpenRead(sanUrl))
            {
                if (stream != null)
                {
                    stream.ReadTimeout = System.Threading.Timeout.Infinite;
                    using (var reader = new StreamReader(stream, Encoding.UTF8, false))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                           // if (line != String.Empty)
                           // {
                                sb.Append(line);
                                count++;
                                // helper.OutMsg("Count {0}", count++);
                            //}
                            //helper.OutMsg(line);
                        }
                    }
                }
            }
            helper.OutMsg("Records in json {0}", count);
            return sb.ToString();
        }

    }
}
