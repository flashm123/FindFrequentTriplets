using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FindFrequentTriplets
{   
    /// <summary>
    /// This class measures the top 10 triplets in the text. 
    /// The path to the text file is taken from the command line arguments.
    /// </summary>
    class Program
    {
        // The count of parts to split the text to
        private const int partCount = 8;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("Please input the file path!");
                return;
            }

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

            for (int i = 0; i < 10; i++)
            {
                if (i == 9)
                {
                    Console.WriteLine(string.Format("{0}", result[i].Letters));
                    break;
                }

                Console.Write(string.Format("{0}, ", result[i].Letters));
            }

            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadKey();
        }

        public static List<Triplet> FindTripletsSync(string input)
        {
            var triplets = MakeTripletCombinations(PrepareText(input));

            var result = ProcessText(triplets);

            return result.OrderByDescending(groupedTriplet => groupedTriplet.Frequency)
                         .ToList<Triplet>();
        }

        public static List<Triplet> FindTripletsAsync(string input)
        {
            var triplets = MakeTripletCombinations(PrepareText(input));

            var tripleParts = SplitTripletsIntoParts(triplets);

            return ProcessTripletsAsync(tripleParts)
                .GroupBy(tr => tr.Letters,
                         triplet => triplet,
                        (key, value) => new Triplet() { Letters = key, Frequency = value.Sum(gr => gr.Frequency) })
                .OrderByDescending(tr => tr.Frequency)
                .ToList<Triplet>();
        }

        /// <summary>
        /// Get count of how many times the selected triple is in the triple list
        /// </summary>
        /// <param name="triplets">Triplets list</param>
        /// <param name="letters">Three letters to search</param>
        /// <returns>The number of times the specified triplet is contained in the text</returns>
        private static int GetTripletFrequency(List<Triplet> triplets, string letters)
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
        /// Make triplet combinations from string of letters and keep them to triplet list
        /// </summary>
        /// <param name="source">Initial text with letters only</param>
        private static List<Triplet> MakeTripletCombinations(string source)
        {
            var triplets = new List<Triplet>();

            for (int i = 0; i < source.Length - 2; i++)
            {
                triplets.Add(new Triplet(source[i].ToString()
                                         + source[i + 1].ToString()
                                         + source[i + 2].ToString()));
            }

            return triplets;
        }

        /// <summary>
        /// Count how many triplet combinations appear in the text: create unique triplets table - letters and frequency
        /// </summary>
        /// <param name="triplets">All triplet</param>
        /// <returns>The triplets table - letters and frequency</returns>
        private static List<Triplet> ProcessText(List<Triplet> triplets)
        {
            // Create a unique triplet table to store the frequency here
            var uniqueTriplets = triplets.GroupBy(tr => tr.Letters,
                                                  triplet => triplet,
                                                 (key, value) => new Triplet() { Letters = key, Frequency = value.First().Frequency })
                                         .ToList<Triplet>();

            // Search how many times each unique triplet is contained in the whole triplet list
            for (int i = 0; i < uniqueTriplets.Count; i++)
            {
                var uniqueTriplet = uniqueTriplets[i];
                uniqueTriplet.Frequency = GetTripletFrequency(triplets, uniqueTriplets[i].Letters);

                uniqueTriplets[i] = uniqueTriplet;
            }

            return uniqueTriplets;
        }

        /// <summary>
        /// Split the tripl list into the several parts
        /// </summary>
        /// <param name="triplets">All triplet combinations</param>
        /// <returns>List of triplet parts</returns>
        private static List<List<Triplet>> SplitTripletsIntoParts(List<Triplet> triplets)
        {
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
        /// Perform text processing in parallel foreach: each unique triplet part is processed by separate thread. Finally, the received parts unite.
        /// </summary>
        /// <param name="tripleParts">The unique triplet parts</param>
        /// <returns>The result</returns>
        private static List<Triplet> ProcessTripletsAsync(List<List<Triplet>> tripleParts)
        {
            var results = new List<List<Triplet>>();
            var output = new List<Triplet>();

            Parallel.ForEach(tripleParts, triplePart =>
            {
                results.Add(ProcessText(triplePart));
            });

            foreach (var result in results)
            {
                output.AddRange(result);
            }

            return output;
        }
    }
}