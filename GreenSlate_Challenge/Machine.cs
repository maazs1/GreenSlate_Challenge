using System;
using System.Collections.Generic;

public class Machine
{
	List<Items> listofItems;
	Dictionary<String, int> coins;
	List<String> acceptCoins;

	public Machine(Dictionary<String, int> coins, List<String> acceptCoins, params Items[] items)
	{
		this.coins = coins;
		this.acceptCoins = acceptCoins;
		listofItems = new List<Items>(items);
	}

	public List<Items> getItems()
	{
		return listofItems;
	}
	public Dictionary<String, int> getCoin()
	{
		return coins;
	}

	public List<String> getAcceptCoins()
	{
		return acceptCoins;
	}

	public void setCoins(Dictionary<String, int> updatedCoins)
	{
		foreach (KeyValuePair<String, int> entry in updatedCoins)
		{
			coins[entry.Key] = entry.Value;
		}
	}

	public void setCoins(String key, int value)
	{
		coins[key] = value;

	}

}
