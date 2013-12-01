using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace DANI_Server.File_Parsing
{
    class cFileParser
    {
        public List<string> Parse(string FileName, int MaxLinesToParse = -1, int MaxSentencesToParse = -1)
        {

            int Linecount = 0;
            List<string> Sentences = new List<string>();
            string PartialSentence = "";
            string Line = null;
            using (StreamReader reader = new FileInfo(FileName).OpenText())
            {

                while (!reader.EndOfStream)
                {
                    //Check MaxLinesToParse not exceeded
                    if (MaxLinesToParse != -1 && Linecount >= MaxLinesToParse)
                        break; // TODO: might not be correct. Was : Exit While
                    //Check MaxSentencesToParse not exceeded
                    if (MaxSentencesToParse != -1 && Sentences.Count >= MaxSentencesToParse)
                        break; // TODO: might not be correct. Was : Exit While

                    Linecount += 1;
                    Line = reader.ReadLine();
                    //If Line.Contains("No sign of my attorney.") Then Stop
                    Line = ProcessLine(Line);
                    if (string.IsNullOrEmpty(Line))
                        continue;

                    //Convert lines to sentences. First look for SentenceEnds in the line.
                    string[] SentenceEnds = Regex.Split(Line, "[!\\.\\?]");
                    if (SentenceEnds.Count() == 0)
                        System.Diagnostics.Debugger.Break();
                    //Split should be return at least 1 element
                    if (SentenceEnds.Count() == 1)
                    {
                        //No sentence end, append to PartialSentence until we get one
                        PartialSentence += " " + Line;
                    }
                    else
                    {
                        for (int i = 0; i <= SentenceEnds.Length - 1; i++)
                        {
                            if (i == 0)
                            {
                                //This is the 1st SentenceEnd.
                                //Save it with any outstanding PartialSentence
                                Sentences.Add((PartialSentence + " " + SentenceEnds[i]).Trim());
                                PartialSentence = "";
                            }
                            else if (i == SentenceEnds.Length - 1 & !string.IsNullOrEmpty(SentenceEnds[i]))
                            {
                                //If last split part is not "", the sentence is spilling onto the next line
                                //so append it to PartialSentence to be completed by a subsequent line
                                PartialSentence += SentenceEnds[i];
                            }
                            else if (!string.IsNullOrEmpty(SentenceEnds[i]))
                            {
                                Sentences.Add(SentenceEnds[i].Trim());
                            }
                        }
                    }
                }
            }

            return Sentences;

        }

        private string ProcessLine(string Line)
        {

            //If it doesn't start with 10 spaces, it's a scene description
            if (!Regex.IsMatch(Line, "\\s{20}"))
            {
                return "";
            }

            Line = Line.Trim();
            if (string.IsNullOrEmpty(Line))
                return "";

            //If it starts or ends with 2 CAPs, it's a character name
            if (Regex.IsMatch(Line, "^[A-Z]{2}") | Regex.IsMatch(Line, "[A-Z]{2}$"))
            {
                return "";
            }

            if (Line.StartsWith("-- 3RD"))
                return "";
            //just a line that we don't want in Fear & Loathing

            //If it starts with a bracket, it's a direction
            if (Regex.IsMatch(Line, "^\\("))
            {
                return "";
            }

            //Replace ... with .
            Line = Regex.Replace(Line, "\\.\\.\\.", ".");
            Line = Regex.Replace(Line, "^\\.+", "");
            //remove leading fullstops
            Line = Regex.Replace(Line, "  ", " ");
            Line = Regex.Replace(Line, "--", " ");
            Line = Regex.Replace(Line, "\"", "");
            //remove quotes.
            Line = Regex.Replace(Line, ",", "");

            return Line;

        }
    }
}
