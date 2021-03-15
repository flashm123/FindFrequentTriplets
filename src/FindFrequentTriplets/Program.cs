using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FindFrequentTriplets
{
	class Program
	{
		
		public static void Main(string[] args)
		{
			OneThread();
			ManyThreads();
			Console.ReadKey();
		}
		
		public static void OneThread()
		{
			string input = @"Трипле́т (лат. triplus — тройной) в генетике — комбинация из трёх последовательно расположенных нуклеотидов в молекуле нуклеиновой кислоты.
			 	В информационных рибонуклеиновых кислотах (иРНК) триплеты образуют так называемые кодоны, с помощью которых в иРНК закодирована последовательность расположения аминокислот в белках[1].
			 	В молекуле транспортной РНК (тРНК) один триплет служит антикодоном.";
			
			var triplets = MakeTripletCombinations(PrepareText(input));
			
			var stopWatch = new Stopwatch();
		    stopWatch.Start();
			
		    ProcessText(triplets);
			
			stopWatch.Stop();
			Console.WriteLine(stopWatch.Elapsed);
			
			var groupedOrderedTriplets = triplets.GroupBy(triplet => triplet.Letters, 
			                                       triplet => triplet, 
			                                       (key, value) => new Triplet() {Letters = key, Frequency = value.First().Frequency} )
										  .OrderByDescending(groupedTriplet => groupedTriplet.Frequency)
										  .ToList<Triplet>();
			
			
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine(string.Format("{0} - {1}", groupedOrderedTriplets[i].Letters, groupedOrderedTriplets[i].Frequency));
			}
			
		}
		
		
		public static void ManyThreads()
		{
			string input = @"Трипле́т (лат. triplus — тройной) в генетике — комбинация из трёх последовательно расположенных нуклеотидов в молекуле нуклеиновой кислоты.
			 	В информационных рибонуклеиновых кислотах (иРНК) триплеты образуют так называемые кодоны, с помощью которых в иРНК закодирована последовательность расположения аминокислот в белках[1].
			 	В молекуле транспортной РНК (тРНК) один триплет служит антикодоном.";
			
			var triplets = MakeTripletCombinations(PrepareText(input));
			
			// split the tripl list into the several parts (part = thread)
			const int parts = 2;
			var partSize = triplets.Count / parts;
			var lastPartSize = triplets.Count - partSize * parts + partSize;
			
			var trPart1 = new List<Triplet>();
			var trPart2 = new List<Triplet>();
			
			for (int i = 0; i < triplets.Count; i++)
			{
				if (i < partSize)
				{
					trPart1.Add(triplets[i]);
				}
				
				if (i >= partSize)
				{
					trPart2.Add(triplets[i]);
				}
			}
			
			var stopWatch = new Stopwatch();
		    stopWatch.Start();
			
		    var thr1 = new Thread(new ThreadStart(() => ProcessText(trPart1)));
       	    thr1.Start();
       	    
       	    var thr2 = new Thread(new ThreadStart(() => ProcessText(trPart2)));
       	    thr2.Start();
       	    
       	    
       	    thr1.Join();
       	    thr2.Join();
			
			stopWatch.Stop();
			Console.WriteLine(stopWatch.Elapsed);
			

			trPart1 = trPart1.GroupBy(triplet => triplet.Letters,
			                          triplet => triplet, 
			                          (key, value) => new Triplet() {Letters = key, Frequency = value.First().Frequency} )
									.ToList<Triplet>();
			
			trPart2 = trPart2.GroupBy(triplet => triplet.Letters, 
			                                       triplet => triplet, 
			                                       (key, value) => new Triplet() {Letters = key, Frequency = value.First().Frequency} )
									.ToList<Triplet>();
										  
			
			triplets = trPart1.Concat(trPart2).GroupBy(triplet => triplet.Letters, 
			                                       triplet => triplet, 
			                                       (key, value) => new Triplet() {Letters = key, Frequency = value.Sum(tr => tr.Frequency)} )
										  .OrderByDescending(groupedTriplet => groupedTriplet.Frequency)
										  .ToList<Triplet>();
			
			
			
			var groupedOrderedTriplets = triplets.OrderByDescending(groupedTriplet => groupedTriplet.Frequency).ToList<Triplet>();
			
			
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine(string.Format("{0} - {1}", groupedOrderedTriplets[i].Letters, groupedOrderedTriplets[i].Frequency));
			}
			
		}
		
		
		
		/// <summary>
		/// Get count of how many times the selected triple is in triple list
		/// </summary>
		/// <param name="triplets">Triplets list</param>
		/// <param name="letters">Three letters to search</param>
		/// <returns></returns>
		public static int GetTripletFrequency(List<Triplet> triplets, string letters)
		{
			return triplets.Count(triplet => triplet.Letters == letters);
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
		public static List<Triplet> MakeTripletCombinations(string source)
		{
			var triplets = new List<Triplet>();
			
			for (int i = 0; i < source.Length - 2; i++)
			{
				triplets.Add(new Triplet(source[i].ToString() 
				                         + source[i+1].ToString() 
				                         + source[i+2].ToString()));
			}
			
			return triplets;
		}
		
		/// <summary>
		/// This struct defines Triplet entity
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
		
		
		public static void ProcessText(List<Triplet> triplets)
		{
			for (int i = 0; i < triplets.Count; i++)
			{
				var triplet = triplets[i];
				triplet.Frequency = GetTripletFrequency(triplets, triplets[i].Letters.ToLower());
				
				// Remove old triplet and Insert new triplet with defined frequency
				triplets.RemoveAt(i);
				triplets.Insert(i, triplet);
			}
		}
	}
}