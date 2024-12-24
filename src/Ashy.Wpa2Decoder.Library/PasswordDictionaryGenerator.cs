using System.Text;
using Ashy.Wpa2Decoder.Library.Models;

namespace Ashy.Wpa2Decoder.Library;

public static class PasswordDictionaryGenerator
{
    public record Parameters
    {
        public required string[] Words { get; init; }
        public required string[] Paddings { get; init; }
        public required string[] Years { get; init; }
        public required bool CapitalizeOnlyFirstLetter { get; init; }
        public int MinLength { get; init; } = 8;
        public int MaxLength { get; init; } = 10;
        public required string[] WordConnectors { get; init; }
        public required Dictionary<char, string[]> Transformations { get; init; }
        public required Dictionary<string, string> Modifications { get; init; }
        public required PcapSummary.KeyParameters[] KeyParameters { get; init; }
        public required IProgressBar Progress { get; init; }
    }

    private static string[] GenerateTwoWordCombinations(string word1, string word2, Parameters parameters)
    {
        var modifications1 = GetAllWordModifications(word1, parameters.Modifications);
        var transformed1 = modifications1.SelectMany(modifiedWord => GetAllWordTransformations(modifiedWord, parameters.Transformations, parameters.CapitalizeOnlyFirstLetter)).Distinct().ToArray();
        var modifications2 = word1==word2 ? modifications1 : GetAllWordModifications(word2, parameters.Modifications);
        var transformed2 = word1==word2 ? transformed1 : modifications2.SelectMany(modifiedWord => GetAllWordTransformations(modifiedWord, parameters.Transformations, parameters.CapitalizeOnlyFirstLetter)).Distinct().ToArray();
        transformed1 = transformed1.Concat(parameters.Years).ToArray();

        List<string> combinations = new List<string>();
        int total = parameters.Paddings.Length*transformed1.Length*parameters.WordConnectors.Length*transformed2.Length*parameters.Paddings.Length;
        parameters.Progress.TotalTicks = total;
        int current = 0;
        foreach (var prefix in parameters.Paddings)
        {
            foreach (var w1 in transformed1)
            {
                foreach (var connector in parameters.WordConnectors)
                {
                    foreach (var w2 in transformed2)
                    {
                        foreach (var suffix in parameters.Paddings)
                        {
                            var p1 = string.IsNullOrWhiteSpace(w2) ?  $"{prefix}{w1}{suffix}" : $"{prefix}{w1}{connector}{w2}{suffix}";
                            var p2 = string.IsNullOrWhiteSpace(w1) ? $"{prefix}{w2}{suffix}" : $"{prefix}{w2}{connector}{w1}{suffix}";
                            if (p1.Length >= parameters.MinLength && p1.Length <= parameters.MaxLength)
                            {
                                combinations.Add(p1);
                                if (w1 != w2)
                                {
                                    combinations.Add(p2);
                                }
                            }
                            current++;
                        }
                    }
                }
                parameters.Progress.Report(current, $"{prefix}{w1}...");
            }
        }
        parameters.Progress.Report(total, "Completed");
        return combinations.Distinct().ToArray();
    }

    internal static string[] GetAllWordModifications(string word, Dictionary<string, string> modifications)
    {
        var result = new List<string>();
        var modified = new[]{word}.Concat(modifications.Where(mod => word.Contains(mod.Key, StringComparison.InvariantCultureIgnoreCase))
            .Select(mod => word.Replace(mod.Key, mod.Value))).ToArray();
        result.AddRange(modified.SelectMany(GetBasicWordModifications));
        return result.Distinct().ToArray();
    }

    internal static string[] GetBasicWordModifications(string word)
    {
        return
        [
            word, word.ToUpper(), word.ToLower(), char.ToUpper(word[0]) + word[1..].ToLower(),
            char.ToUpper(word[0]) + word[1..]
        ];
    }

    internal static string[] GetAllWordTransformations(string word, Dictionary<char, string[]> transformations,
        bool capitalizeOnlyFirstLetter)
    {
        // Use HashSet for uniqueness directly, avoiding Distinct()
        var resultSet = new HashSet<string>();

        // Pre-allocate terms list for efficiency
        var terms = new List<List<string>>(word.Length);

        // Generate term lists with replacements
        foreach (var ch in word)
        {
            var replacements = new List<string> { ch.ToString() };
            if (transformations.TryGetValue(ch, out var transformation))
            {
                replacements.AddRange(transformation);
            }

            terms.Add(replacements);
        }

        // Extend terms with case variations efficiently
        ExtendTermsWithCases(terms, capitalizeOnlyFirstLetter);
        // Generate combinations and add them to resultSet
        GenerateCombinations(terms, resultSet);
        return resultSet.ToArray();
    }

    private static void ExtendTermsWithCases(List<List<string>> terms, bool capitalizeOnlyFirstLetter)
    {
        int index = 0;
        // Iterate over each list in terms to add capitalized variations
        foreach (var list in terms)
        {
            if (capitalizeOnlyFirstLetter && index > 0)
            {
                return;
            }

            // Use HashSet to ensure no duplicates are added
            var wordsToAdd = new HashSet<string>();

            foreach (var transformedChar in list)
            {
                var capitalizedTransformations = GenerateCapitalizedTransformations(transformedChar);
                foreach (var capitalizedWord in capitalizedTransformations)
                {
                    wordsToAdd.Add(capitalizedWord);
                }
            }

            list.AddRange(wordsToAdd);
            index++;
        }
    }

    private static List<string> GenerateCapitalizedTransformations(string transformedChar)
    {
        var result = new List<string>();
        var chars = transformedChar.ToCharArray();

        // Generate all capitalized versions of the char
        for (int i = 0; i < chars.Length; i++)
        {
            var newWord = new char[chars.Length];
            Array.Copy(chars, newWord, chars.Length);
            newWord[i] = char.ToUpper(newWord[i]);
            result.Add(new string(newWord));
        }

        return result;
    }

    private static void GenerateCombinations(List<List<string>> wordLists, HashSet<string> resultSet)
    {
        // Use a StringBuilder to efficiently concatenate words
        var totalCombinations = wordLists.Count;
        var indices = new int[totalCombinations];

        while (true)
        {
            // Build the word combination by accessing each list via indices
            var combination = new StringBuilder(totalCombinations);
            for (int i = 0; i < totalCombinations; i++)
            {
                combination.Append(wordLists[i][indices[i]]);
            }

            // Add the combination to the result set (automatically handles duplicates)
            resultSet.Add(combination.ToString());

            // Increment the indices array to generate the next combination
            int index = totalCombinations - 1;
            while (index >= 0 && indices[index] == wordLists[index].Count - 1)
            {
                indices[index] = 0;
                index--;
            }

            if (index < 0)
                break;

            indices[index]++;
        }
    }

    public static string DictionaryAttack(Parameters parameters)
    {
        var keyParameters = parameters.KeyParameters.FirstOrDefault() ?? throw new Exception("There is no key parameters");
        int totalWords = parameters.Words.Length;
        int totalRounds = (totalWords * totalWords - totalWords) / 2 + totalWords;
        int round = 1;
        for (int i = 0; i <= totalWords-1; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                parameters.Progress.SetRoundAndStep(totalRounds, round, 1);
                var dictionary = GenerateTwoWordCombinations(parameters.Words[i], parameters.Words[j], parameters);
                parameters.Progress.SetRoundAndStep(totalRounds, round, 2);
                int passwordCount = 1;
                foreach (var password in dictionary)
                {
                    parameters.Progress.TotalTicks = dictionary.Length;
                    if (passwordCount % 100 == 0)
                    {
                        parameters.Progress.Report(passwordCount, $"{password} ({passwordCount} of {dictionary.Length} words)");
                    }
                    if (Wpa2Crypto.Test(password, keyParameters))
                    {
                        return password;
                    }

                    passwordCount++;
                }

                round++;
            }
        }
        return string.Empty;
    }
}