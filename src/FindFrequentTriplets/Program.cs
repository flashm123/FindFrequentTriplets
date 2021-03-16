using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;

namespace FindFrequentTriplets
{
	class Program
	{
        private const int threadsCount = 3;

		public static void Main(string[] args)
		{
            if (!File.Exists(args[0]))
            {
                Console.Write("The file doesn't exist!");
                return;
            }

            // Read text from the file
            var input = File.ReadAllText(args[0], Encoding.Default);

            // Initialize stopwatch to measure text processing time
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            // Get 10 the most frequent triplets from the input text
            var result = FindTripletsAsync(input);

            stopWatch.Stop();

            Console.WriteLine(stopWatch.Elapsed);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(string.Format("{0} - {1}", result[i].Letters, result[i].Frequency));
            }

            Console.ReadKey();
		}

        public static void FindTripletsSync(string input)
		{
			var triplets = MakeTripletCombinations(PrepareText(input));

            // Initialize stopwatch to measure text processing time
			var stopWatch = new Stopwatch();
		    stopWatch.Start();
			
		    var result = ProcessText(triplets);
			
			stopWatch.Stop();
			Console.WriteLine(stopWatch.Elapsed);

            var groupedOrderedTriplets = result.OrderByDescending(groupedTriplet => groupedTriplet.Frequency)
										       .ToList<Triplet>();
			
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine(string.Format("{0} - {1}", groupedOrderedTriplets[i].Letters, groupedOrderedTriplets[i].Frequency));
			}
			
		}

        public static List<Triplet> FindTripletsAsync(string input)
		{
			var triplets = MakeTripletCombinations(PrepareText(input));

            var tripleParts = SplitTripletsIntoParts(triplets);

            return ProcessTripletsAsync(tripleParts, threadsCount)
                .GroupBy(tr => tr.Letters,
                         triplet => triplet,
                        (key, value) => new Triplet() { Letters = key, Frequency = value.Sum(gr => gr.Frequency) })
                .OrderByDescending(tr => tr.Frequency)
                .ToList<Triplet>();
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
        /// Count how many triplet combinations appaear in the text: create unique triplets table - letters and frequency
        /// </summary>
        /// <param name="triplets">All triplet</param>
        /// <returns>The triplets table - letters and frequency</returns>
        public static List<Triplet> ProcessText(List<Triplet> triplets)
		{
			// Create a unique triplet table to store frequency here
            var uniqueTriplets = triplets.GroupBy(tr => tr.Letters,
                                                  triplet => triplet, 
                                                 (key, value) => new Triplet() { Letters = key, Frequency = value.First().Frequency })
                                         .ToList<Triplet>();

            for (int i = 0; i < uniqueTriplets.Count; i++)
			{
                var uniqueTriplet = uniqueTriplets[i];
                uniqueTriplet.Frequency = GetTripletFrequency(triplets, uniqueTriplets[i].Letters);

                uniqueTriplets[i] = uniqueTriplet;
			}

            return uniqueTriplets;
		}
		
		/// <summary>
		/// Split the tripl list into the several parts (part = thread)
		/// </summary>
		/// <param name="triplets">All triplet combinations</param>
		/// <returns>List of triplet parts</returns>
        private static List<List<Triplet>> SplitTripletsIntoParts(List<Triplet> triplets)
		{
			// parts = threads
			const int partCount = threadsCount;
			var partSize = triplets.Count / partCount;
			var lastPartSize = triplets.Count - partSize * partCount + partSize;
			
			var tripleParts = new List<List<Triplet>>();
			
			for (int i = 0; i < partCount; i++)
			{
				tripleParts.Add(new List<Triplet>());
				
				if (i == partCount - 1)
				{
					for (int j = i * partSize; j < i * partSize + lastPartSize; j++)
					{
						tripleParts[i].Add(triplets[j]);
					}
                    break;
				}
				
				for (int j = i * partSize; j < i * partSize + partSize; j++)
				{
					tripleParts[i].Add(triplets[j]);
				}
			}

            return tripleParts;
		}

        /// <summary>
        /// Perform text processing in multiple threads: each unique triplet part is processed by separate thread. Finally, the result parts unite.
        /// </summary>
        /// <param name="tripleParts">The unique triplet parts</param>
        /// <param name="threadsCount">The count of the threads</param>
        /// <returns>The result </returns>
        private static List<Triplet> ProcessTripletsAsync(List<List<Triplet>> tripleParts, int threadsCount)
        {
            var results = new List<List<Triplet>>();
            var threads = new Thread[threadsCount];
            var output = new List<Triplet>();

            for (int i = 0; i < threadsCount; i++)
            {
                threads[i] = new Thread(new ThreadStart(delegate()
                {
                    results.Add(new List<Triplet>());
                    results[i] = ProcessText(tripleParts[i]);
                }));

                threads[i].Start();
                threads[i].Join();
            }

            foreach(var result in results)
            {
                output.AddRange(result);
            }

            return output;
        }
	}
}