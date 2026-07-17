using System;
using System.Net.Http;

public class CPHInline
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public bool Execute()
    {
        // 1. Get the guest's username from your command (e.g., !giveword guestname)
        CPH.TryGetArg("rawInput", out string rawInput);
        string guestUser = string.IsNullOrEmpty(rawInput) ? "" : rawInput.Trim().Replace("@", "").ToLower();
        
        if (string.IsNullOrEmpty(guestUser)) {
            CPH.SendMessage("Please specify a guest: !giveword username");
            return false;
        }

        try 
        {
            // 2. Fetch curated Pictionary words from a raw text list
            string response = _httpClient.GetStringAsync("https://raw.githubusercontent.com/engichang1467/word-pictionary-list/master/myWords.txt").GetAwaiter().GetResult();

            // Split the response by newlines into an array and remove any empty entries
            string[] allWords = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if(allWords.Length >= 3)
            {
                // Select 3 random distinct words
                Random rnd = new Random();
                string[] words = new string[3];
                for (int i = 0; i < 3; i++)
                {
                    int index = rnd.Next(allWords.Length);
                    words[i] = allWords[index].Trim().ToUpper();
                    // Basic safeguard to avoid duplicates (could be optimized, but fine for n=3)
                    while (i > 0 && Array.IndexOf(words, words[i], 0, i) != -1)
                    {
                        index = rnd.Next(allWords.Length);
                        words[i] = allWords[index].Trim().ToUpper();
                    }
                }

                if(words.Length >= 3) 
                {
                    // 3. Save options globally. Capitalizing for visual clarity and comparison later.
                    CPH.SetGlobalVar("guestOptions_A", words[0], true);
                    CPH.SetGlobalVar("guestOptions_B", words[1], true);
                    CPH.SetGlobalVar("guestOptions_C", words[2], true);
                    
                    CPH.SetGlobalVar("currentGuest", guestUser, true);
                    CPH.SetGlobalVar("secretWord", "", true); // Clear any old word out

                    // 4. Send the whisper
                    string message = $"Guessing Game! Reply to this whisper with A, B, or C.  A) {words[0]}   B) {words[1]}   C) {words[2]}";
                    CPH.SendWhisper(guestUser, message, true);
                    
                    // 5. Let chat know we are waiting
                    CPH.SendMessage($"Sent 3 secret options to @{guestUser}. Waiting for them to lock in their choice...");
                }
            }
        }
        catch (Exception ex) 
        {
            CPH.LogInfo("Word Fetch Error: " + ex.Message);
            CPH.SendMessage("Error fetching words. Check Streamer.bot logs.");
        }
        
        return true;
    }
}