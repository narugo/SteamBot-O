using SteamKit2;
using System.Collections.Generic;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace SteamBot
{
    public class TradeOfferHandler : UserHandler
    {
        public TradeOfferHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override bool OnFriendAdd()
        {
            return true;
        }

        public override void OnLoginCompleted()
        {

        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            base.OnChatRoomMessage(chatID, sender, message);
        }

        public override void OnFriendRemove() { }

        public override void OnMessage(string message, EChatEntryType type)
        {
            var bot = Bot.tradeUser;
            TradeOffer trade = bot.newTrade(OtherSID);

            var inventory = trade.SInventory.Initialize(trade.partner, 753, 6).getItems();

            foreach (var inv in inventory)
            {
                trade.tradeStatus.them.addItem(753, 6, inv.id, inv.amount);
            }

            trade.update("Test from bot");
        }

        public override void OnTradeAccept()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeClose()
        {
            base.OnTradeClose();
        }

        public override void OnTradeMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeError(string error)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeInit()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeReady(bool ready)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTradeTimeout()
        {
            throw new System.NotImplementedException();
        }

        public override bool OnTradeRequest()
        {
            throw new System.NotImplementedException();
        }

    }

}

