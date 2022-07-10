using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQMParse
{
    public interface ISegment
    {
        string Name { get; }
        string SegmentText { get; }
        IEnumerable<ISegment> SubSegments { get; }
    }

    public class PlainTextSegment : ISegment
    {
        public PlainTextSegment(string text)
        {
            SegmentText = text;
        }

        public string SegmentText { get; set; }
        public string Name => "<unnamed>";
        public IEnumerable<ISegment> SubSegments => new[] { this };        
    }
}
