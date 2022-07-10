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

        string GetFormatted(int indentCount, string indent);
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

        public override string ToString()
        {
            return SegmentText;
        }

        public string GetFormatted(int indentCount, string indent)
        {
            var ind = string.Concat(Enumerable.Repeat(indent, indentCount));
            var txt = SegmentText.Replace("\r\n", $"\r\n{ind}");
            if (txt.EndsWith(ind))
            {
                txt = txt.Substring(0, txt.Length - ind.Length);
            }
            return txt;
        }
    }
}
