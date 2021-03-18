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
        private const int partCount = 32;

        // The top 10 triplets to display
        private static int top = 10;

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

            // if the triplets count in the test is less than the defined top count
            if (result.Count < top)
                top = result.Count;

            for (int i = 0; i < top; i++)
            {
              
            }

            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadKey();
        }

        public static List<string> FindTripletsAsync(string input)
        {
            return new List<string>();
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
    }
}