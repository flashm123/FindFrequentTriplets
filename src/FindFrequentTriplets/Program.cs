using System;
using System.Collections.Generic;
using System.Linq;

namespace FindFrequentTriplets
{
	class Program
	{
		public static void Main(string[] args)
		{
			string input = @"Трипле́т (лат. triplus — тройной) в генетике — комбинация из трёх последовательно расположенных нуклеотидов в молекуле нуклеиновой кислоты.
			 	В информационных рибонуклеиновых кислотах (иРНК) триплеты образуют так называемые кодоны, с помощью которых в иРНК закодирована последовательность расположения аминокислот в белках[1].
			 	В молекуле транспортной РНК (тРНК) один триплет служит антикодоном.";
			
			List<Triplet> triplets;
			
			MakeTripletCombinations(PrepareText(input), out triplets);
			
			for (int i = 0; i < triplets.Count; i++)
			{
				var triplet = triplets[i];
				triplet.Frequency = GetTripletFrequency(triplets, triplets[i].Letters.ToLower());
				
				// Remove old triplet and Insert new triplet with defined frequency
				triplets.RemoveAt(i);
				triplets.Insert(i, triplet);
			}
			
			var groupedOrderedTriplets = triplets.GroupBy(triplet => triplet.Letters, 
			                                       triplet => triplet, 
			                                       (key, value) => new Triplet() {Letters = key, Frequency = value.First().Frequency} )
										  .OrderByDescending(groupedTriplet => groupedTriplet.Frequency)
										  .ToList<Triplet>();
			
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine(string.Format("{0} - {1}", groupedOrderedTriplets[i].Letters, groupedOrderedTriplets[i].Frequency));
			}
			
			Console.ReadKey(true);
		}
		
		public static int GetTripletFrequency(List<Triplet> allTriplets, string letters)
		{
			return allTriplets.Count(triplet => triplet.Letters == letters);
		}
		
		/// <summary>
		/// Prepares input text: remove whitespaces and non-letter symbols
		/// </summary>
		/// <param name="text">Text from file</param>
		/// <returns>Modified text with letters only</returns>
		private static string PrepareText(string text)
		{
			return string.Concat(text.Where(letter => (letter != ' ') && (char.IsLetter(letter))).Select(item => item.ToString())).ToLower();
		}
		
		/// <summary>
		/// Make triplet combinations from string of letters and keep them to triplets list
		/// </summary>
		/// <param name="source">Initial text with letters only</param>
		/// <param name="triplets">Triplet list (contains list of triplet structure)</param>
		public static void MakeTripletCombinations(string source, out List<Triplet> triplets)
		{
			triplets = new List<Triplet>();
			
			for (int i = 0; i < source.Length - 2; i++)
			{
				triplets.Add(new Triplet(source[i].ToString() 
				                         + source[i+1].ToString() 
				                         + source[i+2].ToString()));
			}
		}
		
		public struct Triplet
		{
			public string Letters
			{
				get; 
				set;
			}
			
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
}