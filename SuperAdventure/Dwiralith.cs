using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using System.IO;

namespace SuperAdventure
{
    public partial class Dwiralith : Form
    {

        private Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public Dwiralith()
        {
            InitializeComponent();

            if(File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
                rtbMessages.Text += "Character Loaded"+ Environment.NewLine;


            }
            else
            {
                _player = Player.CreateDefaultPlayer();
                rtbMessages.Text += "Character Not Loaded" + Environment.NewLine;
            }

            MoveTo(_player.CurrentLocation);
            UpdatePlayerStats();
        }
        private void SuperAdventure_Load(object sender, EventArgs e)
        {


        }

        private void ScrollToBottom()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void btnNorth_Click_1(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click_1(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click_1(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnEast_Click_1(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void MoveTo(Location newLocation)
        {

            //Does the location have any required Items?
            if(!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                return;
            }

            //Update the players location!
            _player.CurrentLocation = newLocation;

            //Show/hide the available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Display current location name and description!
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Completely heal the player
            _player.currentHitPoints = _player.maximumHitPoints;

            //Update the HP in the UI
            lblHitPoints.Text = _player.currentHitPoints.ToString();

            //Does the location have a quest?
            if(newLocation.QuestAvailableHere != null)
            {
                //See if the the player already has the quest
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);

                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                //See if the player has all the items needed to complete the quest
                bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                // See if the player already has the quest
                if(playerAlreadyHasQuest)
                {
                    // If the player has not completed the quest yet
                    if(!playerAlreadyCompletedQuest) 
                        {
                             // The player has all items required to complete the quest
                             if(playerHasAllItemsToCompleteQuest)
                             {
                                // Display message
                                rtbMessages.Text += Environment.NewLine;
                                rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;

                               //Remove quest Items
                                _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            //Give quest rewards ;)
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            _player.experiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.goldAmount += newLocation.QuestAvailableHere.RewardGold;

                            //Adding the item to the players inventory :)
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            //Mark the Quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);

                        }
                    }
                }
                else
                {
                    //The player doesn't have the quest
                    //Display the messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " Quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach(QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if(qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    //Add the quest to the player's quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }

            }

            //Does the location have a monster?
            if(newLocation.MonsterLivingHere != null)
            {

                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                //Make a monster using values from the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.currentHitPoints, standardMonster.maximumHitPoints);

                foreach(LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
                
            }

            //Refresh player's inventory list
            UpdateInventoryListInUI();

            //Refresh the players quest list
            UpdateQuestListInUI();

            //Refresh the player's Weapons combobox
            UpdateWeaponListInUI();

            //Refresh player's potions combobox
            UpdatePotionListInUI();

            UpdatePlayerStats();

            ScrollToBottom();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach(PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Details is Weapon)
                {
                    if(inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if(weapons.Count == 0)
            {
                //The player doesn't have any weapons, so hide the weapon combobox and "Use" button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
            
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Details is HealingPotion)
                {
                    if(inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if(healingPotions.Count == 0)
            {
                //The player doesn't have any potions so hide the potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click_1(object sender, EventArgs e)
        {
            //Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            //Determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //Apply the damage to the monster's Current Hit Points
            _currentMonster.currentHitPoints -= damageToMonster;

            //Display message
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine;


            //Check if the monster is dead
            if(_currentMonster.currentHitPoints <= 0)
            {
                //Monster is dead
                rtbMessages.Text += Environment.NewLine;
                rtbLocation.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

                //Give player EXP for monster
                _player.experiencePoints += _currentMonster.RewardExperiencePoints;
                rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points." + Environment.NewLine;

                //Give player gold for monster
                _player.goldAmount += _currentMonster.RewardGold;
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold." + Environment.NewLine;

                //Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add items to the lootedItems list, comparing a random number to the drop percentage
                foreach(LootItem lootItem in _currentMonster.LootTable)
                {
                    if(RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                //If no items were randomly selected, then add the default loot item(s).
                if(lootedItems.Count == 0)
                {
                     foreach(LootItem lootItem in _currentMonster.LootTable)
                     {
                         if(lootItem.IsDefaultItem)
                         {
                             lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                         }
                     }
                }

                //Add the looted items to the player's inventory
                foreach(InventoryItem inventoryItem in lootedItems)
                {
                    _player.AddItemToInventory(inventoryItem.Details);

                    if(inventoryItem.Quantity == 1)
                    {
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural + "." + Environment.NewLine; 
                    }
                }


                UpdatePlayerStats();
                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                //Adding a blank space to the messages box just to look nice B)
                rtbMessages.Text += Environment.NewLine;

                //Move player to current location (To make the heal and create a new monster to fight)
                MoveTo(_player.CurrentLocation);

            }
            else
            {

                //Monster is still alive
                
                //Determine the amount of damage the monster does to the player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(1, _currentMonster.MaximumDamage);

                //Display message
                rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

                //Subtract damage from player
                _player.currentHitPoints -= damageToPlayer;

                //Refresh player data in UI
                lblHitPoints.Text = _player.currentHitPoints.ToString();

                if(_player.currentHitPoints <= 0)
                {
                    //Display message
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

                    //Move player to "Home"
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }

            ScrollToBottom();
        }

        private void btnUsePotion_Click_1(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // Add healing amount to the player's current hit points
            _player.currentHitPoints = (_player.currentHitPoints + potion.AmountToHeal);

            //currentHitPoints cannot exceed player's maximumHitPoints
            if(_player.currentHitPoints > _player.maximumHitPoints)
            {
                _player.currentHitPoints = _player.maximumHitPoints;
            }

            //Remove the potion from the player's inventory
            foreach(InventoryItem ii in _player.Inventory)
            {
                if(ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            //Display Message
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            //Monster gets their turn to attack
            
            //Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(1, _currentMonster.MaximumDamage);

            //Display Message
            rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

            //Subtract damage from player
            _player.currentHitPoints -= damageToPlayer;

            if(_player.currentHitPoints <= 0)
            {

                //Display message
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

                //Move Player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            //Refresh player data in UI
            UpdatePlayerStats();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
            ScrollToBottom();

        }

        private void UpdatePlayerStats()
        {
            //Refresh player information and inventory controls
            lblHitPoints.Text = _player.currentHitPoints.ToString();
            lblGold.Text = _player.goldAmount.ToString();
            lblExperience.Text = _player.experiencePoints.ToString();
            lblLevel.Text = _player.currentLevel.ToString();
        }


 
        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }


    }
}
