using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace DANI_Server.Word_Processing
{
    public class cWordProcessor
    {
        public event AlertEventHandler Alert;
        public delegate void AlertEventHandler(string msg, eProcessType ProcessType);
	    //Dictionary with words as keys. Value is info on how the word is used.
        private Dictionary<string, cWordUsageStats> WordUsages;
	    //used to chose words when there are several candidates
        private static Random Rand;

        public enum eProcessType
        {
	        Learning,
	        Responding,
            Responded,
        }

        public int KnownWords()
        { if (WordUsages == null) return 0; else return WordUsages.Count; }

        private void WriteLine(string msg, eProcessType ProcessType)
        {
	        //Console.WriteLine(msg);
	        if (Alert != null) {
		        Alert(msg, ProcessType);
	        }
        }

        public cWordProcessor()
        {
	        WordUsages = new Dictionary<string, cWordUsageStats>(System.StringComparer.OrdinalIgnoreCase);
	        Rand = new Random();
        }

        public string Process(string Sentence, bool GetReply)
        {

            WriteLine(string.Format("Processing sentence '{0}'", Sentence),  eProcessType.Learning);

            //Parse the sentence into words
            List<string> Words = ParseSentence(Sentence);

            //Add blanks to start and end, each word must have a neighbour, even if it's nothing
            Words.Insert(0, null);
            Words.Add(null);

            //Add or update the known usage of every (non-blank) word
            bool IsStart = false;
            bool IsEnd = false;
            bool PreviousWordStarts = false;
            bool NextWordEnds = false;
            cWordUsageStats WordUsage = null;
            for (int i = 1; i <= Words.Count - 2; i++)
            {
                IsStart = Words[i - 1] == null;
                IsEnd = Words[i + 1] == null;
                PreviousWordStarts = !IsStart && Words[i - 2] == null;
                NextWordEnds = !IsEnd && Words[i + 2] == null;
                //Check is it a word we already know 
                if (WordUsages.TryGetValue(Words[i], out WordUsage))
                {
                    WordUsage.Update(IsStart, IsEnd, Words[i - 1], PreviousWordStarts, Words[i + 1], NextWordEnds);
                }
                else
                {
                    WordUsage = new cWordUsageStats(IsStart, IsEnd, Words[i - 1], PreviousWordStarts, Words[i + 1], NextWordEnds);
                    WordUsages.Add(Words[i], WordUsage);
                }
                WriteLine(string.Format("\tProcessed word '{0}'\t{1}", Words[i], WordUsage), eProcessType.Learning);
            }

            //All words saved. Return if no reply required
            if (GetReply == false)
                return null;

            WriteLine(string.Format("Responding to {0}...", Sentence), eProcessType.Responding);

            //remove dupes from words list
            List<string> NoDupes = new List<string>();
            foreach (string s in Words)
            {
                if ((s != null))
                {
                    if (!NoDupes.Contains(s, StringComparer.OrdinalIgnoreCase))
                    {
                        NoDupes.Add(s);
                    }
                }
            }
            Words = NoDupes;

            //Pick a seed word as a starting point for our reply
            string SeedWord = GetSeedWord(Words);

            //Return a sentence, ie the seedword with DANI's choice of preceding & following words 
            string PreviousWords = GetNeighbouringWords(SeedWord: SeedWord, GoBackwards: true);
            string FollowingWords = GetNeighbouringWords(SeedWord: SeedWord, GoBackwards: false);

            string resp = string.Format("{0} {1} {2}", PreviousWords, SeedWord, FollowingWords).Trim();
            resp = resp[0].ToString().ToUpper() + resp.Substring(1).ToLower() + ".";
            WriteLine(string.Format("Response: {0}", resp), eProcessType.Responding);
            WriteLine("", eProcessType.Responded);
            return resp;

        }

        private List<string> ParseSentence(string Sentence)
        {
            //Remove punctuation
            Sentence = Regex.Replace(Sentence, "[^A-Za-z0-9\\s]+", string.Empty).ToLower();
            string[] aWords = Regex.Split(Sentence, "[\\s\\.]+");
            List<string> lWords = new List<string>();
            //remove blanks, trim others
            foreach (string s in aWords)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    lWords.Add(s.Trim());
                }
            }
            return lWords;
        }

        public string GetSeedWord(List<string> Words)
        {
            List<string> Insignificants = new List<string>
                (new string[] {"a","at","but","by","of","how","in","on","so","to","the","where","who","when","why"});

            WriteLine("\tChoosing seedword...", eProcessType.Responding);

            //Score each word based on count of times it has been used, unless it's an insignificant word
            List<Tuple<int, string>> wordscores = new List<Tuple<int, string>>();
            int score = 0;
            int topScore = 0;
            foreach (string w in Words)
            {
                //when user enters "My" DANI should treat it as "Your" etc
                string word = Microsoft.VisualBasic.Interaction.Switch(w == "i", "you", w == "my", "your", w == "you", "i", w == "your", "my", true, w).ToString();
                if (Insignificants.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    score = 0;
                }
                else
                {
                    score = WordUsages[word].UseCount;
                }
                wordscores.Add(new Tuple<int, string>(score, word));
                if (score > topScore)
                    topScore = score;
            }

            //Sort, then choose a random word from the half that was used most
            wordscores.Sort((b, a) => a.Item1.CompareTo(b.Item1));
            //todo, within any score, words are sorted by order they were added, so same words make top half
            int MaxIndex = Convert.ToInt32(Math.Ceiling((decimal) wordscores.Count / 2) - 1);
            //last index to make the top half
            for (int i = 0; i <= wordscores.Count - 1; i++)
            {
                WriteLine(string.Format("\t\t'{0}' scored {1} based on UseCount, Candidate={2}", wordscores[i].Item2, wordscores[i].Item1, i <= MaxIndex), eProcessType.Responding);
            }
            string ChosenWord = wordscores[Rand.Next(0, MaxIndex + 1)].Item2;
            WriteLine(string.Format("\tChose '{0}' as seedword.", ChosenWord), eProcessType.Responding);

            return ChosenWord;

        }

        public string GetNeighbouringWords(string SeedWord, bool GoBackwards, int MaxWords = 8)
        {
            //Build a chain of linked words working back/forward from SeedWord
            //Apply scoring based on how often the link has started/ended a sentence.
            //Stop at the outermost of the highest scoring links. 

            List<Tuple<int, string>> NeighbouringWords = new List<Tuple<int, string>>();
            string BaseWord = SeedWord; //the word which will be the base to find each previous/next word
            //Score and index of link found to have the highest start/end score
            int HighScore = -1;
            int HighScoreIndex = -1;
            for (int i = 0; i <= MaxWords - 1; i++)
            {
                // (Start or End Score, Word)
                Tuple<int, string> NeighbourWord = GetNeighbouringWord(BaseWord, GoBackwards);
                if (NeighbourWord == null)
                {
                    break; //no word has been used before/after this
                }
                else
                {
                    NeighbouringWords.Add(NeighbourWord);
                    if (NeighbourWord.Item1 >= HighScore)
                    {
                        HighScore = NeighbourWord.Item1;
                        HighScoreIndex = i;
                    }
                }
                BaseWord = NeighbourWord.Item2; //next loop will pass this word to get the next link
            }

            if (HighScore > -1)
            {
                //One or more links has started/ended a sentence. We have the index of the last link with the highest start/end score.
                WriteLine(string.Format("One of the best links to {0} a sentence is '{1}', it scored {2} (based on Link StartEndCount).",
                    GoBackwards ? "start" : "end",
                    NeighbouringWords[HighScoreIndex].Item2,
                    NeighbouringWords[HighScoreIndex].Item1), eProcessType.Responding);
            }
            else
            //If no starting links found, examine the total amount of times each word started/ended a sentence
            {
                WriteLine(string.Format("None of these words {0} sentences when in these links, so counting when linked to any word...", 
                    (GoBackwards ? "started" : "ended")), eProcessType.Responding);
                for (int i = 0; i <= NeighbouringWords.Count - 1; i++)
                {
                    string Word = NeighbouringWords[i].Item2;
                    cWordUsageStats WordUsage = WordUsages[NeighbouringWords[i].Item2];
                    int Score = Convert.ToInt32((GoBackwards ? WordUsage.StartCount : WordUsage.EndCount));
                    if (Score >= HighScore)
                    {
                        HighScore = Score;
                        HighScoreIndex = i;
                    }
                    WriteLine(string.Format("{0}{0}'{1}' scored {2} (based on Word Start/EndCount)", Constants.vbTab, Word, Score), eProcessType.Responding);
                }

                if (HighScore > -1)
                {
                    //One or more links has started/ended a sentence. We have the index of the last link with the highest start/end score.
                    WriteLine(string.Format("One of the best links to {0} a sentence is '{1}', it scored {2} (based on Word StartEndCount).",
                        GoBackwards ? "start" : "end",
                        NeighbouringWords[HighScoreIndex].Item2,
                        NeighbouringWords[HighScoreIndex].Item1), eProcessType.Responding);
                }
                else
                {
                    WriteLine(string.Format("None of these words {0} sentences when in any links, so including them all...",
                       (GoBackwards ? "started" : "ended")), eProcessType.Responding);
                    HighScoreIndex = NeighbouringWords.Count - 1;
                }
            }

            //Truncate NeighbouringWords list to use only words up to the highest Start/End score of the link or word
            List<Tuple<int, string>> NeighbouringWordsCopy = new List<Tuple<int, string>>();
            NeighbouringWordsCopy.AddRange(NeighbouringWords.GetRange(0, HighScoreIndex + 1));
            NeighbouringWords = NeighbouringWordsCopy;

            string retVal = "";
            foreach (Tuple<int, string> kvp in NeighbouringWords)
            {
                if (GoBackwards)
                {
                    retVal = string.Concat(kvp.Item2, " ", retVal);
                }
                else
                {
                    retVal = string.Concat(retVal, " ", kvp.Item2);
                }
            }

            return retVal.Trim();
        }

        /// <summary>
        /// Return one of the preceding/following words, with a score based on its BoundaryCount that
        /// may be used by the caller to decide if it should start a sentence
        /// </summary>
        private Tuple<int, string> GetNeighbouringWord(string BaseWord, bool Backwards)
        {

            WriteLine(string.Format(Constants.vbTab + "Choosing {0} word for '{1}'...", (Backwards ? "preceding" : "following"), BaseWord), eProcessType.Responding);

            //Create list of every word that has been known to precede BaseWord. 
            //Apply score (based on how many times the two words have been used together).
            List<Tuple<int, KeyValuePair<string, cLinkUsageStats>>> wordscores = new List<Tuple<int, KeyValuePair<string, cLinkUsageStats>>>();
            Dictionary<string, cLinkUsageStats> LinkedWords = null;
            //Get the words that have been known to link to this word
            if (Backwards)
            {
                LinkedWords = WordUsages[BaseWord].PreviousWords;
            }
            else
            {
                LinkedWords = WordUsages[BaseWord].NextWords;
            }
            //Assign each link a score based on how many times the words have been used as a pair
            foreach (KeyValuePair<string, cLinkUsageStats> LinkedWord in LinkedWords)
            {
                wordscores.Add(new Tuple<int, KeyValuePair<string, cLinkUsageStats>>(LinkedWord.Value.UseCount, LinkedWord));
            }
            if (wordscores.Count == 0)
            {
                WriteLine(Constants.vbTab + Constants.vbTab + "No links known.", eProcessType.Responding);
                return null;
                //nothing linked to this word in the direction we want
            }

            //Sort, then choose a random word from the half that was used most
            wordscores.Sort((b, a) => a.Item1.CompareTo(b.Item1));
            //sort by score desc 
            int MaxIndex = Convert.ToInt32(Math.Ceiling((decimal) wordscores.Count / 2) - 1);
            //last index to make the top half
            for (int i = 0; i <= wordscores.Count - 1; i++)
            {
                WriteLine(string.Format("{0}{0}'{1}', Score (based on Link UseCount)={2}, Candidate={3}", Constants.vbTab, wordscores[i].Item2, wordscores[i].Item1, i <= MaxIndex), eProcessType.Responding);
            }

            //Choose a random word from the half that was linked most
            int ChosenIndex = Rand.Next(0, MaxIndex + 1);
            Tuple<int, KeyValuePair<string, cLinkUsageStats>> ChosenWord = wordscores[ChosenIndex];

            //Return the linked word with the count of times it started/ended a sentence linked to the BaseWord
            WriteLine(string.Format(Constants.vbTab + "Chose '{0}' as {1} word for '{2}', StartEndCount={3}", ChosenWord.Item2.Key, (Backwards ? "preceding" : "following"), BaseWord, ChosenWord.Item2.Value.StartEndCount), eProcessType.Responding);
            return new Tuple<int, string>(ChosenWord.Item2.Value.StartEndCount, ChosenWord.Item2.Key);

        }

    }
}
