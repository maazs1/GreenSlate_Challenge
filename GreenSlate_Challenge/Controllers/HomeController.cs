using GreenSlate_Challenge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GreenSlate_Challenge.Controllers
{
    public class HomeController : Controller
    {
        //Dictionary to store the coin name and their value. Can be updated to store more 
        public Dictionary<String, int> coinConversion = new Dictionary<String, int>
        {
            {"penny", 1},
            {"nickel", 5},
            {"dime",10 },
            {"quarter", 25}
        };

        //Coins initialized in the machine
        private static Dictionary<String, int> machineCoins = new Dictionary<String, int>
        {
            {"penny", 10},
            {"dime", 10},
            {"nickel", 5},
            {"quarter", 25}
        };


        //Initialzing machine object
        private static Machine drinksMachine = new Machine(

            // Coins stored in the machine
            machineCoins,

            // Coins the machine accepts
            new List<String>() { "penny", "nickel", "dime", "quarter" },

            // Items are instantiaed (supported by name, price, and quantity) 
            new Items("VM_Coke", 1.25, 5),
            new Items("VM_Pepsi", 0.50, 15),
            new Items("VM_Soda", 0.75, 3)
        );

        //Item objects
        Items coke = drinksMachine.getItems().Find(x => x.getName() == "VM_Coke");
        Items pepsi = drinksMachine.getItems().Find(x => x.getName() == "VM_Pepsi");
        Items soda = drinksMachine.getItems().Find(x => x.getName() == "VM_Soda");

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
            ViewData["Coke"] = coke;
            ViewData["Soda"] = soda;
            ViewData["Pepsi"] = pepsi;
            return View();
        }

        [HttpPost]
        public IActionResult Receipt()
        {
            //Retreiving and validating users input from the form
            int penny = Request.Form["penny"] == "" ? 0 : Convert.ToInt32(Request.Form["penny"]);
            int nickel = Request.Form["nickel"] == "" ? 0 : Convert.ToInt32(Request.Form["nickel"]);
            int dime = Request.Form["dime"] == "" ? 0 : Convert.ToInt32(Request.Form["dime"]);
            int quarter = Request.Form["quarter"] == "" ? 0 : Convert.ToInt32(Request.Form["quarter"]);
            int cokeQuantity = Request.Form["coke"] == "" ? 0 : Convert.ToInt32(Request.Form["coke"]);
            int pepsiQuantity = Request.Form["pepsi"] == "" ? 0 : Convert.ToInt32(Request.Form["pepsi"]);
            int sodaQuantity = Request.Form["soda"] == "" ? 0 : Convert.ToInt32(Request.Form["soda"]);
            double total = (cokeQuantity * coke.getCost()) + (pepsiQuantity * pepsi.getCost()) + (sodaQuantity * soda.getCost());

            //Storing the coin name and amount the user inputs
            Dictionary<String, int> userCoins = new Dictionary<String, int>
            {
                {"penny", penny},
                {"nickel", nickel},
                {"dime", dime},
                {"quarter", quarter}
            };

            //Check to see if the drinksMachine has the quantity for the items selected
            if (cokeQuantity > coke.getQuantity() || sodaQuantity > soda.getQuantity() || pepsiQuantity > pepsi.getQuantity())
            {
                ViewBag.Message = "Drink is sold out, your purchase cannot be processed";
                return View("Error");
            }

            //If total is less than or equal to 0, drink was not selected
            if (total <= 0)
            {
                ViewBag.Message = "No drink was selected, your purchase cannot be processed";
                return View("Error");
            }

            //Calculate total amount of coins inputted from the user
            List<String> vmAcceptedCoins = drinksMachine.getAcceptCoins();
            int paid = 0;
            foreach (KeyValuePair<String, int> entry in userCoins)
            {
                //Check to see if the machine supports the coins entered
                if (!vmAcceptedCoins.Contains(entry.Key))
                {
                    ViewBag.Message = "The machine does not support one or more of coins entered";
                    return View("Error");
                }
                if (coinConversion.ContainsKey(entry.Key))
                {
                    paid += (entry.Value * coinConversion[entry.Key]);
                }

            }

            //Calculating the change
            double change = paid - (total * 100);

            //Checking to see if the user didn't input enough coins
            if (change < 0)
            {
                ViewBag.Message = "Not enough coins were entered, your purchase cannot be processed";
                return View("Error");
            }

            //Getting dictionary of current coins in the machine
            Dictionary<String, int> vmCoins = drinksMachine.getCoin();
            int vmTotal = 0;

            List<int> vmCoinsValue = new List<int>();
            List<String> vmCoinsKey = new List<String>();

            foreach (KeyValuePair<String, int> entry in vmCoins)
            {
                //Iterating through the machines current coins to calculate the total amount money
                if (coinConversion.ContainsKey(entry.Key))
                {
                    vmTotal = vmTotal + (coinConversion[entry.Key] * entry.Value);
                    vmCoinsValue.Add(coinConversion[entry.Key]);
                    vmCoinsKey.Add(entry.Key);
                }
            }

            // Check to see if machine has enough change
            if (change > vmTotal)
            {
                ViewBag.Message = "Not sufficient change in the inventory";
                return View("Error");
            }

            //Sort the list of coin values the machine supports and arrange them in descending order
            //Ex. If the machine has quarter (25) and dime (10), 25 will be first.
            vmCoinsValue.Sort();
            vmCoinsValue.Reverse();

            Dictionary<String, int> receipt = new Dictionary<String, int>();

            //Starting from the greatest coin value to get the highest denomination of change possible
            for (int i = 0; i < vmCoinsValue.Count; i++)
            {
                int output = (int)Math.Floor(change / vmCoinsValue[i]);
                if (output != 0)
                {
                    for (int j = 0; j < vmCoinsKey.Count; j++)
                    {
                        //Find coin name of the current coin value by through the coinConversion dictionary 
                        if (coinConversion[vmCoinsKey[j]] == vmCoinsValue[i])
                        {
                            System.Diagnostics.Debug.WriteLine("1");
                            System.Diagnostics.Debug.WriteLine(vmCoinsKey[j]);

                            //Check to see if the number of coins being output is greater than the coin the machine has 
                            if (output > vmCoins[vmCoinsKey[j]])
                            {
                                //Get the difference between how many coins are being returned from how many coins the machine has
                                int diff = output - vmCoins[vmCoinsKey[j]];

                                //Take as much of the coins as the machine has and update the machines coin amount to 0. Update change and 
                                //add the number of coins being returned on the receipt
                                drinksMachine.setCoins(vmCoinsKey[j], 0);
                                change = change + (diff * vmCoinsValue[i]) - (vmCoins[vmCoinsKey[j]] * vmCoinsValue[i]);
                                receipt.Add(vmCoinsKey[j], vmCoins[vmCoinsKey[j]]);
                                break;
                            }
                            else
                            {
                                //Else just add the amount of coins being outputed to the receipt, update change, and the machines coins 
                                receipt.Add(vmCoinsKey[j], output);
                                drinksMachine.setCoins(vmCoinsKey[j], vmCoins[vmCoinsKey[j]] - output);
                                change = change - (output * vmCoinsValue[i]);
                                break;
                            }
                        }
                    }
                }
            }

            //Update items quantity
            coke.setQuantity(coke.getQuantity() - cokeQuantity);
            pepsi.setQuantity(pepsi.getQuantity() - pepsiQuantity);
            soda.setQuantity(soda.getQuantity() - sodaQuantity);

            ViewData["Receipt"] = receipt;
            ViewData["cokeQuantity"] = cokeQuantity;
            ViewData["pepsiQuantity"] = pepsiQuantity;
            ViewData["sodaQuantity"] = sodaQuantity;
            ViewData["Coke"] = coke;
            ViewData["Soda"] = soda;
            ViewData["Pepsi"] = pepsi;
            ViewData["total"] = total;
            ViewData["paid"] = (double)paid / 100;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
