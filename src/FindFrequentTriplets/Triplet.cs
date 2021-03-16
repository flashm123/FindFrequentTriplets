using System;

namespace FindFrequentTriplets
{
	/// <summary>
	/// Description of Triplet.
	/// </summary>
	public struct Triplet
	{
		/// <summary>
		/// Three letters
		/// </summary>
		public string Letters
		{
			get; 
			set;
		}
		
		/// <summary>
		/// How many times the letters present in the whole text
		/// </summary>
		public int Frequency
		{
			get; 
			set;
		}
		
		public Triplet(string letters) : this()
		{
			Letters = letters;
		}
	}
}
