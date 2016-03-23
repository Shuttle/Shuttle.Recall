using System;

namespace Shuttle.Recall.Tests
{
	public class Location
	{
		public Location(string address, DateTime dateMoved)
		{
			Address = address;
			DateMoved = dateMoved;
		}

		public string Address { get; private set; }
		public DateTime DateMoved { get; private set; }
	}
}