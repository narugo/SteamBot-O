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
        private String partnerName;
        public SteamID partner;
        public string sessionId;
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
                html = tradeUser.Fetch("http://steamcommunity.com/tradeoffer/new/?partner=" + tradePartner.ConvertToUInt64(), "GET", null);
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
            sessionId = JsonConvert.DeserializeObject(globals["g_sessionID"]).ToString();
            inventoryLoadUrl = JsonConvert.DeserializeObject(globals["g_strInventoryLoadURL"]).ToString();
            partnerInventoryLoadUrl = JsonConvert.DeserializeObject(globals["g_strTradePartnerInventoryLoadURL"]).ToString();

            String tradeOfferMessage = "";
            String json_tradeoffer = JsonConvert.SerializeObject(tradeStatus, Formatting.None, new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            NameValueCollection basic = new NameValueCollection();
            basic.Add("sessionid", sessionId);
            basic.Add("partner", partner.ToString());
            basic.Add("tradeoffermessage", tradeOfferMessage);
            basic.Add("json_tradeoffer", json_tradeoffer);
            basic.Add("tradeofferid_countered", id.ToString());
        }

        public void update(string message)
        {
            tradeStatus.version++;
            tradeStatus.newversion = true;

            String json_tradeoffer = JsonConvert.SerializeObject(tradeStatus, Formatting.None, new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            NameValueCollection data = new NameValueCollection();
            data.Add("sessionid", sessionId);
            data.Add("partner", partner.AccountID.ToString());
            data.Add("tradeoffermessage", message);
            data.Add("json_tradeoffer", json_tradeoffer.ToString());

            if (tradeId != 0)
            {
                data.Add("'tradeofferid_countered'", tradeId.ToString());
            }

            string result = tradeUser.Fetch("http://steamcommunity.com/tradeoffer/new/send", "GET", null, true);
            Console.WriteLine(result);
        }

        public class TradeStatus
        {
            public bool newversion { get; set; }
            public int version { get; set; }
            public TradeStatusUser me { get; set; }
            public TradeStatusUser them { get; set; }
        }

        public class TradeStatusUser
        {
            Boolean ready { get; set; }
            public List<TradeAsset> assets = new List<TradeAsset>();

            public sInventory.Description getDescription(TradeAsset tradeAsset)
            {
                return new sInventory().getDescription(tradeAsset);
            }

            public Boolean addItem(sInventory.Item item)
            {
                return addItem(item.inventory.appId, item.inventory.contextId, item.id);
            }

            public bool addItem(long appId, long contextId, String id)
            {
                TradeAsset tradeAsset = new TradeAsset();
                tradeAsset.appid = appId;
                tradeAsset.contextid = contextId;
                tradeAsset.assetid = Convert.ToUInt64(id);
                addItem(tradeAsset);
                return true;
            }

            public Boolean addItem(TradeAsset asset)
            {
                if (!assets.Contains(asset))
                {
                    assets.Add(asset);
                    return true;
                }
                return false;
            }

            public Boolean removeItem(TradeAsset asset)
            {
                if (assets.Contains(asset))
                {
                    assets.Remove(asset);
                    return true;
                }
                return false;
            }

            public Boolean containsItem(int appId, int contextId, ulong assetId)
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
                public String id { get; set; }
                public String classid { get; set; }
                public String instanceid { get; set; }
                public String amount { get; set; }
                public String pos { get; set; }
                public Description description { get; set; }
                public sInventory inventory { get; set; }
            }

            public class Description
            {
                public String appid { get; set; }
                public String classid { get; set; }
                public String instanceid { get; set; }
                public String name { get; set; }
                public List<Dictionary<String, String>> tags = new List<Dictionary<String, String>>();
                public Dictionary<String, String> app_data = new Dictionary<String, String>();
            }

            public Boolean success { get; set; }
            public long appId { get; set; }
            public long contextId { get; set; }
            public Dictionary<String, Item> rgInventory = new Dictionary<string,Item>();
            public Dictionary<String, Description> rgDescriptions = new Dictionary<string,Description>();

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
                string url = String.Format("http://steamcommunity.com/profiles/{0}/inventory/json/{1}/{2}", id.ConvertToUInt64(), appid, contextid);

                url = SteamWeb.Fetch(url, "GET", null, null, false);
                if (url != null)
                {
                    var json = JsonConvert.DeserializeObject<sInventory>(url);
                    return json;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
