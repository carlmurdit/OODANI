using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DANI_Server.Word_Processing
{
    class cLinkUsageStats
    {
        public int UseCount = 0;
        public int StartEndCount = 0;
        public cLinkUsageStats(bool IsBoundary)
        {
	        this.Update(IsBoundary);
        }
        public void Update(bool IsBoundary)
        {
	        this.UseCount += 1;
	        if (IsBoundary)
		        StartEndCount += 1;
        }
        public override string ToString()
        {
	        return string.Format("Link UseCount: {0}\tLink StartEndCount: {1}", UseCount, StartEndCount);
        }
    }
}
