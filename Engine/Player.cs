using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Engine
{
     public class Player : LivingCreature
    {

         public int goldAmount { get; set; }
         public int experiencePoints { get; set; }
         public int currentLevel 
         {
             get { return ((experiencePoints / 100) + 1); } 
         }

         public Location CurrentLocation { get; set; }

         public List<InventoryItem> Inventory { get; set; }
         public List<PlayerQuest> Quests { get; set; } 

         private Player( int CurrentHitPoints, int MaximumHitPoints, int gold, int exp) : base( CurrentHitPoints, MaximumHitPoints )
         {

             goldAmount = gold;
             experiencePoints = exp;
             

             Inventory = new List<InventoryItem>();
             Quests = new List<PlayerQuest>();

         }

         public static Player CreateDefaultPlayer()
         {

             Player player = new Player(10, 10, 20, 0);
             player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
             player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

             return player;

         }

         public static Player CreatePlayerFromXmlString(string xmlPlayerData)
         {
             try
             {
                 XmlDocument playerData = new XmlDocument();

                 playerData.LoadXml(xmlPlayerData);

                 int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);

                 int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);

                 int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);

                 int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                 Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                 int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);

                 foreach(XmlNode node in playerData.SelectNodes("/Player/Stats/CurrentLocation"))
                 {
                     int id = Convert.ToInt32(node.Attributes["ID"].Value);
                     int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                     for(int i = 0; i < quantity; i++)
                     {
                         player.AddItemToInventory(World.ItemByID(id));
                     }
                 }

                 foreach(XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                 {
                     int id = Convert.ToInt32(node.Attributes["ID"].Value);
                     bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                     PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                     playerQuest.IsCompleted = isCompleted;

                     player.Quests.Add(playerQuest);
                 }

                 return player;
             }
             catch
             {
              
                 
                 //If there was an error with the XML Data, Return a default player object
                 return Player.CreateDefaultPlayer();
             }
         }

         public bool HasRequiredItemToEnterThisLocation(Location location)
         {
             if(location.ItemRequiredToEnter == null)
             {
                 //There is no required item for this location, so return "true"
                 return true;
             }

             //See if the player has the required item in their inventory
             return Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
         }

         public bool HasThisQuest(Quest quest)
         {
             return Quests.Exists(pq => pq.Details.ID == quest.ID);
         }

         public bool CompletedThisQuest (Quest quest)
         {
             foreach(PlayerQuest playerQuest in Quests)
             {
                 if(playerQuest.Details.ID == quest.ID)
                 {
                     return playerQuest.IsCompleted;
                 }
             }

             return false;
         }

         public bool HasAllQuestCompletionItems(Quest quest)
         {
             //See if the player has all the items needed to complete the quest here
             foreach(QuestCompletionItem qci in quest.QuestCompletionItems)
             {  
                 // Check each item in the player's inventory, to see if they have it, and enough of it
                 if(!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                 {
                     return false;
                 }
             }

             //If we got here, then the player must have all the required items and enough of them to complete the quest.
             return true;
         }

         public void RemoveQuestCompletionItems(Quest quest)
         {
             foreach(QuestCompletionItem qci in quest.QuestCompletionItems)
             {
                 InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID
);

                 if(item != null)
                 {
                     //Subtract the quantity from the player's inventory that was needed to complete the quest
                     item.Quantity -= qci.Quantity;
                 }
             }
         }

         public void AddItemToInventory(Item itemToAdd)
         {
             InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

             if(item == null)
             {
                 //They didn't have the item so add it to their inventory, with a quantity of 1
                 Inventory.Add(new InventoryItem(itemToAdd, 1));
             }
             else
             {
                 //They have the item in their inventory, so increase the quantity by one
                 item.Quantity++;
             }
         }

         public void MarkQuestCompleted(Quest quest)
         {
             //Find the quest in the player's quest list
             PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

             if(playerQuest != null)
             {
                 playerQuest.IsCompleted = true;
             }
         }

         public string ToXmlString()
         {
             XmlDocument playerData = new XmlDocument();

             //Create the top-level XML node
             XmlNode player = playerData.CreateElement("Player");
             playerData.AppendChild(player);

             //Create the "Stats" child node to hold the other player statistics nodes
             XmlNode stats = playerData.CreateElement("Stats");
             player.AppendChild(stats);

             //Create child nodes for the "Stats" node
             XmlNode CurrentHitPoints = playerData.CreateElement("CurrentHitPoints");
             CurrentHitPoints.AppendChild(playerData.CreateTextNode(this.currentHitPoints.ToString()));
             stats.AppendChild(CurrentHitPoints);

             XmlNode MaximumHitPoints = playerData.CreateElement("MaximumHitPoints");
             MaximumHitPoints.AppendChild(playerData.CreateTextNode(this.maximumHitPoints.ToString()));
             stats.AppendChild(MaximumHitPoints);
             
             XmlNode Gold = playerData.CreateElement("Gold");
             Gold.AppendChild(playerData.CreateTextNode(this.goldAmount.ToString()));
             stats.AppendChild(Gold);

             XmlNode ExperiencePoints = playerData.CreateElement("ExperiencePoints");
             ExperiencePoints.AppendChild(playerData.CreateTextNode(this.experiencePoints.ToString()));
             stats.AppendChild(ExperiencePoints);

             XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
             currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
             stats.AppendChild(currentLocation);

             //Create the "InventoryItems" child node to hold each InventoryItem node
             XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
             player.AppendChild(inventoryItems);

             //Create the "InventoryItems" Child node for each Item in the player's inventory
             foreach(InventoryItem item in this.Inventory)
             {
                 XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                 XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                 idAttribute.Value = item.Details.ID.ToString();
                 inventoryItem.Attributes.Append(idAttribute);

                 XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                 quantityAttribute.Value = item.Quantity.ToString();
                 inventoryItem.Attributes.Append(quantityAttribute);

                 inventoryItems.AppendChild(inventoryItem);
             }

             //Create the "PlayerQuests" child node to hold each Player Quest node
             XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
             player.AppendChild(playerQuests);

             //Create a "PlayerQuest" node for each Quest the player has acquired
             foreach(PlayerQuest quest in this.Quests)
             {
                 XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                 XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                 idAttribute.Value = quest.Details.ID.ToString();
                 playerQuest.Attributes.Append(idAttribute);

                 XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                 isCompletedAttribute.Value = quest.IsCompleted.ToString();
                 playerQuest.Attributes.Append(isCompletedAttribute);

                 playerQuests.AppendChild(playerQuest);

             }

             return playerData.InnerXml; //The XML document, as a string so we can save the data to disk


         }
    }
}
