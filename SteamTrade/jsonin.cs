using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamKit2;
using System.Text;

namespace SteamTrade
{
    /// <summary>
    /// This class represents the TF2 Item schema as deserialized from its
    /// JSON representation.
    /// </summary>
    public class JsonTest
    {


        public static JsonTest FetchJsonTest(int appid,int contextId, SteamID steamid)
        {
            string result = SteamWeb.Fetch(string.Format("http://steamcommunity.com/profiles/{0}/inventory/json/{1}/{2}/", steamid.ConvertToUInt64(), appid, contextId), "GET", null, null, true);
            JsonTest jsonResult = JsonConvert.DeserializeObject<JsonTest>(result);
            return jsonResult ?? null; 
        }

        public ItemDescription GetItemDescription(ulong id)
        {
            
            
            if ( !rgInventory.ContainsKey ( id.ToString ()))
            
            {
                return null;
            }
            else
            {
                string temp = id.ToString();
                Item x = rgInventory[temp];
                string tempid = x.classid  + "_"+x.instanceid;
                return rgDescriptions[tempid];
            }

        }

        [JsonProperty("success")]
        public bool success { get; set; }

        [JsonProperty("rgInventory")]
        public Dictionary<string, Item> rgInventory { get; set; }

        [JsonProperty("rgDescriptions")]
        public Dictionary<string, ItemDescription> rgDescriptions { get; set; }

        public class Item
        {

            [JsonProperty("id")]
            public string id { get; set; }

            [JsonProperty("classid")]
            public string classid { get; set; }

            [JsonProperty("instanceid")]
            public string instanceid { get; set; }

            [JsonProperty("amount")]
            public string amount { get; set; }

            [JsonProperty("pos")]
            public string pos { get; set; }
            
        }

        public class ItemDescription
        {

            [JsonProperty("appid")]
            public string appid { get; set; }

            [JsonProperty("classid")]
            public string classid { get; set; }

            [JsonProperty("instanceid")]
            public string instanceid { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }

            [JsonProperty("market_hash_name")]
            public string market_hash_name { get; set; }

            [JsonProperty("type")]
            public string type { get; set; }

            [JsonProperty("tradable")]
            public int tradable { get; set; }

        }


    }
}

