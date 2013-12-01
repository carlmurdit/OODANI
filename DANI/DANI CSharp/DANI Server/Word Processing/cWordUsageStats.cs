using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DANI_Server.Word_Processing
{
    class cWordUsageStats
    {
        public Dictionary<string, cLinkUsageStats> PreviousWords;
        public Dictionary<string, cLinkUsageStats> NextWords;
        public int StartCount = 0;
        public int EndCount = 0;

        public int UseCount = 0;
        public cWordUsageStats(bool IsStart, bool IsEnd, string PreviousWord, bool PreviousWordStarts, string NextWord, bool NextWordEnds)
        {
	        PreviousWords = new Dictionary<string, cLinkUsageStats>(System.StringComparer.OrdinalIgnoreCase);
	        NextWords = new Dictionary<string, cLinkUsageStats>(System.StringComparer.OrdinalIgnoreCase);
	        this.Update(IsStart, IsEnd, PreviousWord, PreviousWordStarts, NextWord, NextWordEnds);
        }


        public void Update(bool IsStart, bool IsEnd, string PreviousWord, bool PreviousWordStarts, string NextWord, bool NextWordEnds)
        {
	        UseCount += 1;
	        if (IsStart)
		        StartCount += 1;
	        if (IsEnd)
		        EndCount += 1;
	        //Create or update link to previous word
	        if (!string.IsNullOrEmpty(PreviousWord)) {
		        cLinkUsageStats PreviousLink = null;
		        if (this.PreviousWords.TryGetValue(PreviousWord, out PreviousLink)) {
			        PreviousLink.Update(PreviousWordStarts);
		        } else {
			        this.PreviousWords.Add(PreviousWord, new cLinkUsageStats(PreviousWordStarts));
		        }
	        }
	        //Create or update link to following word
	        if (!string.IsNullOrEmpty(NextWord)) {
		        cLinkUsageStats NextLink = null;
		        if (this.NextWords.TryGetValue(NextWord, out NextLink)) {
			        NextLink.Update(NextWordEnds);
		        } else {
			        this.NextWords.Add(NextWord, new cLinkUsageStats(NextWordEnds));
		        }
	        }
        }

        public override string ToString()
        {
	        string s = null;
	        s = string.Format("Word UseCount: {0},\tWord StartCount: {1},\tWord EndCount: {2}\n", UseCount, StartCount, EndCount);
	        foreach (KeyValuePair<string, cLinkUsageStats> kvp in this.PreviousWords) {
		        s += string.Format("\t\tPreceded by:'{0}'\t{1}\n", kvp.Key,  kvp.Value);
	        }
	        foreach (KeyValuePair<string, cLinkUsageStats> kvp in this.NextWords) {
                s += string.Format("\t\tFollowed by:'{0}'\t{1}\n", kvp.Key, kvp.Value);
	        }
	        return s;
        }
    }
}
