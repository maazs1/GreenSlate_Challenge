using System;

public class Items
{
	String name;
	double cost;
	int quantity;

	public Items(String name, double cost, int quantity)
	{
		this.name = name;
		this.cost = cost;
		this.quantity = quantity;
	}

	public String getName()
	{
		return name;
	}

	public double getCost()
	{
		return cost;
	}

	public int getQuantity()
	{
		return quantity;
	}

	public void setQuantity(int newQuantity)
	{
		if (newQuantity >= 0)
		{
			quantity = newQuantity;
		}
	}


}
