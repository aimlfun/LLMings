namespace LLMing.LLM;

/// <summary>
/// Implement the inference matcher. Nothing fancy.
/// </summary>
internal static class InferenceMatcher
{
    /* REQUIREMENTS
     * I want to have a string that contains placeholders like *.
     * I won't know the values of the "*" when I compare.
     * I want to compare an AI question from the user, and best match the known questions with their associated "help".
     * Example. 
     * Stored Question: How old is *?
     * User Question: How old is Jane Doe?
     * The inference matcher should work out if the user question is a match to the stored question. In this case, it is.
     * The matcher would match "How old is", and "?" to "How old is", and "?".
     * It should give +1 point for each word that matches.
     * It should calculate the max points that could match. That total is the number of words in the stored question, excluding the "*".
     * e.g. in "How old is *?", the max points would be 4. It should not include "*" in the count.
     * For that to work, we need to split the text into words, by " ", ",","-","?".
     * The match cannot be positional. e.g. "who is Percy Henry Sweetnam's oldest child?" should match "who is * oldest child?".
     * In that example:
     * "who is * oldest child?"
     *   1   2 3    4     5  6
     * "who is Percy Henry Sweetnam's oldest child?"
     *   1   2   3     4      5         6     7   8
     * 
     * A "*" can be anywhere in the sentence, and indicates one or more missing words.
     * 
     */
    public static float Match(string inferredQuestion, string userQuestion)
    {
        int points = 0;

        inferredQuestion = inferredQuestion.Replace("?", "").Replace("\n", "");
        userQuestion = userQuestion.Replace("?", "").Replace("\n", "");

        // Split the stored question into words.
        ReadOnlySpan<char> separators = [' ', ',', '-', '?'];

        string[] inferredQuestionWords = inferredQuestion.ToLower().Split(separators);

        // Split the user question into words.
        string[] userQuestionWords = userQuestion.ToLower().Split(separators);

        // Calculate the max points that could match.
        int maxPoints = 0;

        foreach (string word in inferredQuestionWords)
        {
            if (word != "*")
            {
                maxPoints++;
            }
        }

        // Loop through the stored words.
        int inferredQuestionIndex = 0;

        // pointer to the user words.
        int userQuestionIndex = 0;
        int skippedWords = 0;
        bool skipUntilNextWordLocated = false;

        while (userQuestionIndex < userQuestionWords.Length)
        {
            // If the stored word is a "*", skip it.
            if (inferredQuestionWords[inferredQuestionIndex] == "*")
            {
                inferredQuestionIndex++;
                ++skippedWords;
                skipUntilNextWordLocated = true;
            }

            if (inferredQuestionIndex >= inferredQuestionWords.Length || userQuestionIndex >= userQuestionWords.Length) break;

            // If the stored word is not a "*", and we are skipping words, skip the user word.
            if (skipUntilNextWordLocated)
            {
                // we need to skip until the next matching word is located.
                while (inferredQuestionWords[inferredQuestionIndex] != userQuestionWords[userQuestionIndex])
                {
                    ++userQuestionIndex;

                    // if we reach the end of the user words, return the points, we didn't manage to continue matching
                    if (userQuestionIndex >= userQuestionWords.Length)
                    {
                        return (float)points / (float)maxPoints;
                    }
                }

                skipUntilNextWordLocated = false;
            }


            // If the stored word matches the user word, add a point.
            if (inferredQuestionWords[inferredQuestionIndex] == userQuestionWords[userQuestionIndex])
            {
                points++;
                inferredQuestionIndex++;
            }
            else
            {
                if (userQuestionIndex > inferredQuestionIndex + skippedWords) ++maxPoints;
            }

            ++userQuestionIndex;

            if (inferredQuestionIndex >= inferredQuestionWords.Length)
            {
                break;
            }
        }

        return (float)points / (float)maxPoints;
    }
}