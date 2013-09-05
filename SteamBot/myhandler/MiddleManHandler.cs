using SteamKit2;
using System.Collections.Generic;
using SteamTrade;
using System;
using System.Timers;
using Newtonsoft.Json;
using System.IO;

namespace SteamBot
{
    public class MiddleManHandler : UserHandler
    {
        int UserRareAdded, BotRareAdded, BotKeyAdded, UserKeyAdded = 0;

        bool middleitemadd = false;
        bool credititemadd = false;
        MiddleManItem.Record record = new MiddleManItem.Record();
        List<ulong> CreditItemAdded = new List<ulong>();
        MiddleManItem.Record recordtomodify = new MiddleManItem.Record();
        int UserItemAdded = 0;
        int RareWardNum, UncommonWardNum = 0;
        int fakeitem = 0;
        //UserItem.Useritem item = null;
        UserItem.Useritem item = new UserItem.Useritem();
        List<UserItem.Useritem> UserItemToAdded = new List<UserItem.Useritem>();
        List<int> IndexOfItems = new List<int>();
        List<ulong> ItemIdAdded = new List<ulong>();
        int index = 0;
        int PriceKey, PriceRr = 0;
        bool SetingPrice = false;
        int TradeType = 0; // 1为卖家放置物品,2为买家预定购买;
        bool TradeError = false;
        static int RateOfKeyAndRare = 5;
        public static MiddleManItem currentmiddlerecords = null;
        public static UserItem currentuseritem = null;
        static long filetime;
        static bool Warding = false;
        public MiddleManHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {
        }
        public override void OnTradeSuccess()
        {
            if (TradeType == 1)
            {
                currentmiddlerecords.Records.Add(record);
                Writejson();
            }
            else if (TradeType == 2)
            {
                recordtomodify.Status = 1;
                recordtomodify.Buyercredititems = CreditItemAdded;
                recordtomodify.Buyersteam64id = OtherSID.ConvertToUInt64();
            }
            else
            {
            }
        }
        public override bool OnFriendAdd()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");
            // Using a timer here because the message will fail to send if you do it too quickly

            return true;
        }
        public void ReInit()
        {


            TradeError = false;
            TradeType = 0;
            middleitemadd = false;
            credititemadd = false;
        }
        public void ReInititem()
        {

        }


        public override void OnLoginCompleted()
        {
            if (currentmiddlerecords == null)
            {
                currentmiddlerecords = MiddleManItem.FetchSchema();
                Bot.log.Success("middlerecords 读取完成");
            }

        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            base.OnChatRoomMessage(chatID, sender, message);
        }

        public override void OnFriendRemove()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") removed me!");
        }

        public override void OnMessage(string message, EChatEntryType type)
        {
            // if (message == ".removeall")
            //{
            // Commenting this out because RemoveAllFriends is a custom function I wrote.
            // Bot.SteamFriends.RemoveAllFriends();
            // Bot.log.Warn("Removed all friends from my friends list.");
            // }
            if (message.Contains("confirm"))
            {
                string msg = message;
                msg = msg.Remove(0, 7);
                msg = msg.Trim();
                bool find = false;
                foreach (var xxx in currentmiddlerecords.Records)
                {
                    if (xxx.Recordid == msg && xxx.Sellersteam64id == OtherSID.ConvertToUInt64())
                    {
                        find = true;
                        xxx.Status = 2;// 2为买家已经确认
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "单号 " + msg + " 的物品已经由卖家确认收到款");
                        SteamID xid = new SteamID();
                        xid.SetFromUInt64(xxx.Buyersteam64id);
                        Bot.SteamFriends.SendChatMessage(xid, type, "单号 " + msg + " 的物品已经由卖家确认收到款");
                        break;
                    }
                }
                if (find == false)
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, type, "单号 " + msg + " 的物品没有找到");
                }

            }
            else if (message.Contains("info"))
            {
                string msg = message;
                msg = msg.Remove(0, 4);
                msg = msg.Trim();
                bool find = false;
                foreach (var xxx in currentmiddlerecords.Records)
                {
                    if (xxx.Recordid == msg)
                    //    && xxx.Sellersteam64id == OtherSID.ConvertToUInt64())
                    {
                        find = true;

                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "单号 " + msg + "  " + xxx.Sellersteam64id);

                        break;
                    }
                }
                if (find == false)
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, type, "单号 " + msg + " 的物品没有找到");
                }

            }
            else if (message.Contains("myitems"))
            {
                string msg = message;

                foreach (var xxx in currentuseritem.Items)
                {
                    if (xxx.Steam64id == OtherSID.ConvertToUInt64())
                    {
                        string xstatus, xerror;
                        if (xxx.Status == 0)
                        {
                            xstatus = "正在出售中";
                        }
                        else if (xxx.Status == 1)
                        {
                            xstatus = "已出售";
                        }
                        else if (xxx.Status == 2)
                        {
                            xstatus = "钱已取";
                        }
                        else
                        {
                            xstatus = "物品已被所有者移走";
                        }

                        if (xxx.Error == false)
                        {
                            xerror = "";
                        }

                        else
                        {
                            xerror = "此物品状态错误,请与管理员联系";
                        }

                        string xmsg = "物品名称:" + xxx.Item_name + " Id:" + xxx.Id + " 价格:" + xxx.Pricekey + "key" + xxx.Pricerr + "RR  " + xstatus + "  " + xerror;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, xmsg);
                    }
                }
            }
            else
            {

            }

            //Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.ChatResponse);
        }

        public override bool OnTradeRequest()
        { /*
            long timecheck = DateTime.Now.ToFileTime();
            if ((timecheck - filetime )>350000000)
            {
                Warding=false;
            } */
            if (Warding == true)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "其他人正在操作,请稍后再试");
                return false;
            }
            else
            {
                Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
                UInt64 xxid = OtherSID.ConvertToUInt64();
                Bot.log.Warn(xxid.ToString());
                return true;
            }
        }

        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Oh, there was an error: " + error + "."
                                              );
            Bot.log.Warn(error);
        }

        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "Sorry, but you were AFK and the trade was canceled.");
            Bot.log.Info("User was kicked because he was AFK.");
        }

        public override void OnTradeInit()
        {
            ReInit();
            //TradeCountInventory(true);
            Trade.SendMessage("初始化成功.");
        }

        public int AddRare(int num)
        {

            // var items = new List<Inventory.Item>();
            int i = 0;
            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {

                bool incurrentitem = false;
                if (i >= num)
                {

                    break;
                }

                else
                {
                    foreach (var xxx in currentuseritem.Items)
                    {
                        if (xxx.Id == item.OriginalId)
                        {
                            incurrentitem = true;
                            break;
                        }
                    }
                    if (incurrentitem == false)
                    {
                        var dota2item = Trade.Dota2Schema.GetItem(item.Defindex);

                        if (dota2item != null)
                        {

                            if (dota2item != null && dota2item.Item_rarity == "rare" && dota2item.Prefab == "wearable")
                            {

                                i++;
                                Trade.AddItem(item.Id);

                            }
                        }
                    }
                }

            }
            return i;

        }
        public int AddKey(int num)
        {

            int x = 0;
            for (int i = 0; i < num; i++)
            {
                if (Trade.AddItemByDefindex(15003))
                {
                    x++;
                }
                else
                {
                    break;
                }
            }
            return x;

        }

        public void AddUncommon(int num)
        {

            var items = new List<Inventory.Item>();
            var dota2item = Trade.Dota2Schema.GetItem(0);
            int i = 0;

            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {

                if (i >= num)
                {

                    break;
                }

                else
                {

                    dota2item = Trade.Dota2Schema.GetItem(item.Defindex);
                    if (dota2item != null)
                    {

                        if (dota2item != null && dota2item.Item_rarity == "uncommon" && dota2item.Prefab == "wearable")
                        {

                            i++;
                            Trade.AddItem(item.Id);


                        }
                    }
                }


            }
            Trade.SendMessage("添加罕见" + i + "件");

        }



        public void AddCommon(int num)
        {

            var items = new List<Inventory.Item>();
            var dota2item = Trade.Dota2Schema.GetItem(0);
            int i = 0;

            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {

                if (i >= num)
                {

                    break;
                }

                else
                {

                    dota2item = Trade.Dota2Schema.GetItem(item.Defindex);

                    if (dota2item != null)
                    {

                        if (dota2item != null && (dota2item.Item_rarity == "common" || dota2item.Item_rarity == null) && (dota2item.Prefab == "default_item" || dota2item.Prefab == "wearable"))
                        {
                            i++;
                            Trade.AddItem(item.Id);
                        }
                    }
                }
            }
            Trade.SendMessage("添加普通" + i + "件");
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            if (TradeType == 0)
            {
                TradeType = 1;
            }
            if (TradeType == 1)
            {
                if (inventoryItem == null)
                {

                    Trade.SendMessage("无法识别物品，请打开仓库显示,交易即将取消");
                    Trade.CancelTrade();
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "无法识别物品，请打开仓库显示,交易取消，请打开库存显示后重试");

                }
                else
                {
                    if (!middleitemadd)
                    {
                        middleitemadd = true;
                        record = new MiddleManItem.Record();
                        var zhitem = Trade.CurrentSchemazh.GetItem(inventoryItem.Defindex);
                        record.Sellersteam64id = OtherSID.ConvertToUInt64();
                        //ReInititem();
                        //SetingPrice = true;
                        DateTime dt = DateTime.Now;
                        string a = dt.Month.ToString();
                        if (a.Length < 2)
                        {
                            a = "0" + a;
                        }
                        record.Recordid = dt.Year.ToString() + a;
                        a = dt.Day.ToString();
                        if (a.Length < 2)
                        {
                            a = "0" + a;
                        }
                        record.Recordid = record.Recordid + a;
                        a = dt.Hour.ToString();
                        if (a.Length < 2)
                        {
                            a = "0" + a;
                        }
                        record.Recordid = record.Recordid + a;
                        a = dt.Minute.ToString();
                        if (a.Length < 2)
                        {
                            a = "0" + a;
                        }
                        record.Recordid = record.Recordid + a;
                        a = dt.Second.ToString();
                        if (a.Length < 2)
                        {
                            a = "0" + a;
                        }
                        record.Recordid = record.Recordid + a;

                        Trade.SendMessage(record.Recordid);
                        record.Id = inventoryItem.OriginalId;
                        record.Defindex = inventoryItem.Defindex;
                        record.Item_name = zhitem.ItemName;
                        Trade.SendMessage("需中间人交易的物品已记录,请放置一定数量的稀有作为押金,买家需要缴纳相同数量的押金,押金在买家拿到物品后将被退回");
                    }
                    else
                    {
                        if (!credititemadd)
                        {
                            CreditItemAdded = new List<ulong>();
                        }
                        credititemadd = true;
                        var dota2item = Trade.Dota2Schema.GetItem(schemaItem.Defindex);
                        if (dota2item.Item_rarity == "rare" && (dota2item.Prefab == "wearable" || dota2item.Prefab == "ward" || dota2item.Prefab == "hud_skin"))
                        {
                            //UserRareAdded++;
                            // Trade.SendMessage("机器人添加:" + "稀有 " + BotRareAdded + " 用户添加:" + "稀有 " + UserRareAdded);
                            CreditItemAdded.Add(inventoryItem.OriginalId);
                            record.Sellercredititems = CreditItemAdded;
                        }
                        else
                        {
                            Trade.SendMessage("不是菠菜,交易将取消");
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                                      "不是菠菜,交易取消，请打开库存显示后重试");
                        }

                    }


                }
            }
            else if (TradeType == 2)
            {
                Trade.RemoveAllItems();
                if (!credititemadd)
                {
                    CreditItemAdded = new List<ulong>();
                }
                credititemadd = true;
                var dota2item = Trade.Dota2Schema.GetItem(schemaItem.Defindex);
                if (dota2item.Item_rarity == "rare" && (dota2item.Prefab == "wearable" || dota2item.Prefab == "ward" || dota2item.Prefab == "hud_skin"))
                {
                    UserRareAdded++;
                    // Trade.SendMessage("机器人添加:" + "稀有 " + BotRareAdded + " 用户添加:" + "稀有 " + UserRareAdded);
                    CreditItemAdded.Add(inventoryItem.OriginalId);
                    //record.Buyercredititems = CreditItemAdded;
                }
                else
                {
                    Trade.SendMessage("不是菠菜,交易将取消");
                    Trade.CancelTrade();
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "不是菠菜,交易取消，请重试");
                }
            }


            else
            {
                Trade.SendMessage("当前模式为返还物品，无需添加物品");
            }


        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {

            Trade.CancelTrade();

        }

        public override void OnTradeMessage(string message)
        {
            Bot.log.Info("[TRADE MESSAGE] " + message);
            string msg = message;
            if (msg.Contains("buy"))
            {
                if (TradeType == 0)
                {
                    TradeType = 2;
                    Trade.SendMessage("已设定为预定购买模式");
                }
                if (TradeType == 2)
                {
                    msg = msg.Remove(0, 3);
                    msg = msg.Trim();
                    bool find = false;
                    foreach (var xxx in currentmiddlerecords.Records)
                    {
                        if (xxx.Recordid == msg)
                        {
                            recordtomodify = xxx;
                            Trade.AddItemByOriginal_id(xxx.Id);
                            foreach (var yyy in xxx.Sellercredititems)
                            {
                                Trade.AddItemByOriginal_id(yyy);
                            }
                            find = true;
                            Trade.SendMessage("机器人添加的第一个是卖家的交易物品,其余为押金,请仔细确认");
                        }

                    }
                    if (find == false)
                    {
                        Trade.SendMessage("没有找到物品");
                    }

                }
                else
                {
                    Trade.SendMessage("当前处于其他模式，不能预定购买，请重新交易");
                }
            }
            else if (msg.Contains("buyerget"))
            {
                if (TradeType == 0)
                {
                    TradeType = 3; //买家拿到物品及拿回押金
                    Trade.SendMessage("交易模式已经设定为买家拿取物品模式");
                }
                if (TradeType == 3)
                {
                   msg = msg.Remove(0, 8);
                    msg = msg.Trim();
                    bool find = false;
                    
                    foreach (var xxx in currentmiddlerecords.Records)
                    {
                        if (xxx.Recordid == msg && xxx.Buyersteam64id ==OtherSID.ConvertToUInt64() && xxx.Status ==2)
                        {
                            recordtomodify = xxx;
                            find = true;
                            Trade.AddItemByOriginal_id(xxx.Id );
                            foreach (var yyy in xxx.Buyercredititems)
                            {
                                Trade.AddItemByOriginal_id (yyy);
                            }
                            break;
                        }
                    }
                    if (find)
                    {
                        Trade.SendMessage("物品已放置");
                    }
                    else
                    {
                        Trade.SendMessage("没有找到符合要求的物品");
                    }

                }
                else
                {
                    Trade.SendMessage("错误的命令");
                }


            }
            else if (msg.Contains("sellerget"))
            {
                if (TradeType == 0)
                {
                    TradeType = 4; //卖家拿回押金
                    Trade.SendMessage("交易模式已经设定为卖家拿回押金模式");
                }
                if (TradeType == 4)
                {
                    msg = msg.Remove(0, 9);
                    msg = msg.Trim();
                    bool find = false;

                    foreach (var xxx in currentmiddlerecords.Records)
                    {
                        if (xxx.Recordid == msg && xxx.Sellersteam64id  == OtherSID.ConvertToUInt64() && xxx.Status == 3)//3为买家已拿回物品
                        {
                            recordtomodify = xxx;
                            find = true;
                            //Trade.AddItemByOriginal_id(xxx.Id);
                            foreach (var yyy in xxx.Sellercredititems )
                            {
                                Trade.AddItemByOriginal_id(yyy);
                            }
                            break;
                        }
                    }
                    if (find)
                    {
                        Trade.SendMessage("物品已放置");
                    }
                    else
                    {
                        Trade.SendMessage("没有找到符合要求的物品");
                    }

                }
                else
                {
                    Trade.SendMessage("错误的命令");
                }

            }

            else
            {
                Trade.SendMessage("错误的指令或者未设定交易模式");
            }

        }

        public override void OnTradeReady(bool ready)
        {
            //Because SetReady must use its own version, it's important
            //we poll the trade to make sure everything is up-to-date.
            if (!ready)
            {
                Trade.SetReady(false);
            }
            else
            {

                if (Validate())
                {

                    Bot.log.Success("User is ready to trade!");

                    Trade.SetReady(true);

                }
                else
                {
                   // Trade.SendMessage("你提供的有我不支持的物品");
                    Trade.SetReady(false);
                }

            }




        }

        public override void OnTradeAccept()
        {

            //Even if it is successful, AcceptTrade can fail on
            //trades with a lot of items so we use a try-catch
            if (Validate())
            {

                try
                {
                    Trade.AcceptTrade();
                }
                catch
                {
                    Log.Warn("The trade might have failed, but we can't be sure.");

                }
                Log.Success("Trade Complete!");




            }
            else
            {
                Trade.SetReady(false);
            }


        }

        public void Writejson()
        {

            string json = JsonConvert.SerializeObject(currentmiddlerecords);
            string path = @"middlemanitem.json";
            //string jsonadd = JsonConvert.SerializeObject(UserItemToAdded );
            // Bot.log.Warn(jsonadd);
            StreamWriter sw = new StreamWriter(path, false);
            sw.WriteLine(json);
            sw.Close();
            // 写入文件；

        }

        public void Additemstofile()
        {
            if (TradeError == true)
            {
                for (int i = 0; i < UserItemToAdded.Count; i++)
                {
                    UserItemToAdded[i].Error = true;
                }
            }



            foreach (var xxx in UserItemToAdded)
            {
                currentuseritem.Items.Add(xxx);

            }
            Writejson();

            // 写入文件；

        }
        public void Sellitemsfromfile()
        {
            /*foreach (var xxx in UserItemToAdded)
            {
                int x = 0;
                    foreach (var yyy in currentuseritem.Items )
                    {
                        if (yyy.Id ==xxx.Id && yyy.Steam64id ==xxx.Steam64id && yyy.Defindex ==xxx.Defindex  &&yyy.Status== 0)
                        {
                            x = currentuseritem.Items.IndexOf(yyy);
                            currentuseritem.Items[x].Status = 1;
                        }
                    }
            } */
            /*
            foreach (var xxx in IndexOfItems )
            {
                currentuseritem.Items[xxx].Status = 1;
                if (TradeError == true)
                {
                    currentuseritem.Items[xxx].Error = true;
                }
            }  */
            foreach (var xxx in UserItemToAdded)
            {
                xxx.Status = 1;
                if (TradeError == true)
                {
                    xxx.Error = true;
                }
            }
            Writejson();// 写入文件；

        }

        public void ToRemoveitemsfromfile()
        {   /*
            foreach (var xxx in UserItemToAdded)
            {
                int x = 0;
                foreach (var yyy in currentuseritem.Items)
                {
                    if (yyy.Id == xxx.Id && yyy.Steam64id == xxx.Steam64id && yyy.Defindex == xxx.Defindex &&   yyy.Status==0 )
                    {
                        x = currentuseritem.Items.IndexOf(yyy);
                        currentuseritem.Items[x].Status = 3;
                    }
                }
            } */
            /*
            foreach (var xxx in IndexOfItems)
            {
                currentuseritem.Items[xxx].Status = 3;
                if (TradeError == true)
                {
                    currentuseritem.Items[xxx].Error = true;
                }
            } */
            foreach (var xxx in UserItemToAdded)
            {
                xxx.Status = 3;
                if (TradeError == true)
                {
                    xxx.Error = true;
                }
            }
            Writejson(); // 写入文件；

        }

        public void Moneygiveditemsfromfile()
        { /*
            foreach (var xxx in UserItemToAdded)
            {
                int x = 0;
                foreach (var yyy in currentuseritem.Items)
                {
                    if (yyy.Id == xxx.Id && yyy.Steam64id == xxx.Steam64id && yyy.Defindex == xxx.Defindex && yyy.Status == 1)
                    {
                        x = currentuseritem.Items.IndexOf(yyy);
                        currentuseritem.Items[x].Status = 2;
                    }
                }
            } */
            /*
       foreach (var xxx in IndexOfItems)
       {
           currentuseritem.Items[xxx].Status = 2;
           if (TradeError == true)
           {
               currentuseritem.Items[xxx].Error = true;
           }
       } */

            foreach (var xxx in UserItemToAdded)
            {
                xxx.Status = 2;
                if (TradeError == true)
                {
                    xxx.Error = true;
                }
            }
            Writejson(); // 写入文件；

        }


        public override void OnTradeClose()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
            //Bot.log.Warn("[USERHANDLER] TRADE CLOSED");
            base.OnTradeClose();
        }

        public bool Validate()
        {
            if (TradeType == 1)
            {
                return true;
            }
            else if (TradeType == 2)
            {
                Trade.RemoveAllItems();
                if (BotRareAdded == UserRareAdded)
                {
                    return true;
                }
                else
                {
                    Trade.SendMessage("你添加的稀有数量与卖家添加的不符");
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

    }    
    
 
}

