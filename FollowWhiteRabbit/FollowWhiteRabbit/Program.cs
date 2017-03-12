using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowWhiteRabbit
{
    class Program
    {
        const string EasyAnswer = "e4820b45d2277f3844eac66c903e84be";
        const string MoreDifficultAnswer = "23170acc097c24edb98fc5488ab033fe";
        const string HardAnswer = "665e5bcb0c20062fe8abaaf4628bb154";

        private enum AnswerType
        {
            None = 0,
            Easy = 1,
            MoreDifficult = 2,
            Hard = 3
        }

        static void Main(string[] args)
        {
            string initPhrase = "poultry outwits ants";
            using (var reader = new StreamReader("wordlist.txt"))
            {
                var line = reader.ReadToEnd();
                var outWords = line.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

                var charArray = initPhrase.ToCharArray();
                var letters = charArray.Distinct().ToDictionary(x => x, y => charArray.Count(x => x == y));
                outWords = outWords.Where(x => CanCreateWord(new Dictionary<char, int>(letters), x)).ToList();

                var result = new ConcurrentDictionary<AnswerType, string>();

                Parallel.ForEach(outWords, (word) => {
                    var dict = new Dictionary<char, int>(letters);
                    if (!CanCreateWord(dict, word))
                        return;

                    foreach (var word2 in outWords)
                    {
                        var dict2 = new Dictionary<char, int>(dict);
                        if (word.Length + word2.Length > initPhrase.Length - 2 || !CanCreateWord(dict2, word2))
                            continue;

                        foreach (var word3 in outWords.Where(x => x.Length == initPhrase.Length - word.Length - word2.Length - 2))
                        {
                            var dict3 = new Dictionary<char, int>(dict2);
                            if (!CanCreateWord(dict3, word3))
                                continue;

                            var phrase = $"{word} {word2} {word3}";

                            var answerType = CheckMd5Hash(phrase);
                            if (answerType != AnswerType.None)
                            {
                                result.TryAdd(answerType, phrase);
                            }
                        }
                    }
                });

                result.ToList().ForEach(x => Console.WriteLine($"{x.Key.ToString()}: {x.Value}"));
                Console.ReadKey();
            }
        }

        public static bool CanCreateWord(Dictionary<char, int> letters, string word)
        {
            var wordLetters = word.ToCharArray();

            foreach (var letter in wordLetters)
            {
                if (!letters.ContainsKey(letter))
                    return false;

                letters[letter]--;

                if (letters[letter] < 0)
                    return false;
            }

            return true;
        }

        private static AnswerType CheckMd5Hash(string phrase)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(phrase);
                var hash = md5.ComputeHash(inputBytes).ToList();

                var builder = new StringBuilder();
                // X2 prints the string as two uppercase hexadecimal characters
                hash.ForEach(x => builder.Append(x.ToString("X2")));

                var result = builder.ToString().ToLower();
                switch (result)
                {
                    case HardAnswer:
                        return AnswerType.Hard;
                    case EasyAnswer:
                        return AnswerType.Easy;
                    case MoreDifficultAnswer:
                        return AnswerType.MoreDifficult;
                }

                return AnswerType.None;
            }
        }
    }
}
