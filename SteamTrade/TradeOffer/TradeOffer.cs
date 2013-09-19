using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamTrade;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SteamKit2;

namespace SteamTrade.TradeOffer
{
    public class TradeOffer
    {
        private TradeUser tradeUser;
        private string partnerName;
        public SteamID partner;
        public string sessionID { get; set; }
        private string inventoryLoadUrl;
        private string partnerInventoryLoadUrl;
        private int tradeId;
        public TradeStatus tradeStatus;
        public sInventoryLoad SInventory = new sInventoryLoad();

        public TradeOffer(TradeUser tradeeUser, int id, SteamID tradePartner)
        {
            tradeUser = tradeeUser;
            tradeId = id;

            string html;
            if(id == 0)
            {
                html = tradeUser.Fetch("http://steamcommunity.com/tradeoffer/new/?partner=" + tradePartner.AccountID, "GET", null);
            }
            else
            {
                html = tradeUser.Fetch("http://steamcommunity.com/tradeoffer/" + id + "/", "GET", null);
            }

            Regex pattern = new Regex("^\\s*var\\s+(g_.+?)\\s+=\\s+(.+?);\\r?$", RegexOptions.Multiline);

            IDictionary<string, string> globals = new Dictionary<string, string>();

            foreach (Match match in pattern.Matches(html))
            {
                globals.Add(match.Groups[1].Value, match.Groups[2].Value);
            }

            tradeStatus = JsonConvert.DeserializeObject<TradeStatus>(globals["g_rgCurrentTradeStatus"]);
            partner = new SteamID(Convert.ToUInt64(JsonConvert.DeserializeObject(globals["g_ulTradePartnerSteamID"])));
            partnerName = JsonConvert.DeserializeObject(globals["g_strTradePartnerPersonaName"]).ToString();
            sessionID = JsonConvert.DeserializeObject(globals["g_sessionID"]).ToString();
            inventoryLoadUrl = JsonConvert.DeserializeObject(globals["g_strInventoryLoadURL"]).ToString();
            partnerInventoryLoadUrl = JsonConvert.DeserializeObject(globals["g_strTradePartnerInventoryLoadURL"]).ToString();
        }

        public void update(string message)
        {
            tradeStatus.version++;
            tradeStatus.newversion = true;

            string json_tradeoffer = JsonConvert.SerializeObject(tradeStatus, Formatting.None, new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.None });

            Console.WriteLine(json_tradeoffer);

            NameValueCollection data = new NameValueCollection();
            data.Add("sessionid", sessionID);
            data.Add("partner", partner.ConvertToUInt64().ToString());
            data.Add("tradeoffermessage", message);
            data.Add("json_tradeoffer", json_tradeoffer.ToString());

            Console.WriteLine("sessionid: " + sessionID);
            Console.WriteLine("partner: " + partner.ConvertToUInt64());
            Console.WriteLine("tradeoffermessage: " + message);
            Console.WriteLine("json_tradeoffer: " + json_tradeoffer.ToString());

            if (tradeId != 0)
            {
                data.Add("tradeofferid_countered", tradeId.ToString());
            }

            string result = tradeUser.Fetch("http://steamcommunity.com/tradeoffer/new/send", "POST", data, true);
            Console.WriteLine(result);
        }

        public class TradeStatus
        {
            public bool newversion { get; set; }
            public int version { get; set; }
            public TradeStatusUser me { get; set; }
            public TradeStatusUser them { get; set; }

            public TradeStatus()
            {
                version = 0;
                me = new TradeStatusUser();
                them = new TradeStatusUser();
            }
        }

        public class TradeStatusUser
        {
            public bool ready { get; set; }
            public List<TradeAsset> assets { get; set; }
            public List<string> currency { get; set; }

            public TradeStatusUser()
            {
                assets = new List<TradeAsset>();
                ready = false;
                currency = new List<string>();
            }

            public sInventory.Description getDescription(TradeAsset tradeAsset)
            {
                return new sInventory().getDescription(tradeAsset);
            }

            public bool addItem(int appId, int contextId, string id, int amount)
            {
                TradeAsset tradeAsset = new TradeAsset();
                tradeAsset.appid = appId;
                tradeAsset.contextid = contextId;
                tradeAsset.assetid = Convert.ToUInt64(id);
                tradeAsset.amount = amount;
                addItem(tradeAsset);
                return true;
            }

            private bool addItem(TradeAsset asset)
            {
                if (!assets.Contains(asset))
                {
                    assets.Add(asset);
                    return true;
                }
                return false;
            }

            public bool removeItem(TradeAsset asset)
            {
                if (assets.Contains(asset))
                {
                    assets.Remove(asset);
                    return true;
                }
                return false;
            }

            public bool containsItem(int appId, int contextId, ulong assetId)
            {
                foreach (TradeAsset tradeAsset in assets)
                {
                    if (tradeAsset.appid == appId && tradeAsset.contextid == contextId && tradeAsset.assetid == assetId)
                        return true;
                }
                return false;
            }
        }

        public class TradeAsset
        {
            public long appid { get; set; }
            public long contextid { get; set; }
            public long amount { get; set; }
            public ulong assetid { get; set; }
        }

        public class sInventory
        {
            public class Item
            {
                public string id { get; set; }
                public string classid { get; set; }
                public string instanceid { get; set; }
                public int amount { get; set; }
                public string pos { get; set; }
                public Description description { get; set; }
                public sInventory inventory { get; set; }

                public Item()
                {
                    description = new Description();
                    inventory = new sInventory();
                }
            }

            public class Description
            {
                public int appid { get; set; }
                public string classid { get; set; }
                public string instanceid { get; set; }
                public string name { get; set; }
                public List<Dictionary<string, string>> tags { get; set; }
                public Dictionary<string, string> app_data { get; set; }

                public Description()
                {
                    tags = new List<Dictionary<string, string>>();
                    app_data = new Dictionary<string, string>();
                }
            }

            public IDictionary<string, Item> rgInventory { get; set; }
            public IDictionary<string, Description> rgDescriptions { get; set; }
            public int contextid { get; set; }

            public sInventory()
            {
                rgInventory = new Dictionary<string, Item>();
                rgDescriptions = new Dictionary<string, Description>();
            }

            public void updateItems()
            {
                foreach (Item item in rgInventory.Values)
                {
                    item.inventory = this;
                    item.description = rgDescriptions[item.classid + "_" + item.instanceid];
                }
            }

            public List<Item> getItems()
            {
                return new List<Item>(rgInventory.Values);
            }

            public Description getDescription(TradeAsset tradeAsset)
            {
                return rgInventory[tradeAsset.assetid.ToString()].description;
            }
        }

        public class sInventoryLoad
        {
            public sInventory Initialize(SteamID id, long appid, long contextid)
            {
                string url = string.Format("http://steamcommunity.com/profiles/{0}/inventory/json/{1}/{2}", id.ConvertToUInt64(), appid, contextid);

                url = SteamWeb.Fetch(url, "GET", null, null, false);
                if (url != null)
                {
                    return JsonConvert.DeserializeObject<sInventory>(url);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
