using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CQMParse
{
    public class CodeNode : ISegment
    {
        public static List<CodeNode> GetCodeNodes(HtmlNode root)
        {
            var result = new List<CodeNode>();
            var allNodes = root.Descendants().ToArray();
            for(int i=0; i<allNodes.Length; i++)
            {
                var node = allNodes[i];
                if (node.HasClass(NameNodeClass))
                {
                    var code = node.GetNextElementWithClass(CodeNodeClass);
                    if(code != null)
                    {
                        result.Add(new CodeNode(node, code));
                    }
                }
            }
            return result;
        }

        private CodeNode(HtmlNode htmlNode, HtmlNode codeNode)
        {
            NameHtmlNode = htmlNode;
            CodeHtmlNode = codeNode;
            Name = System.Web.HttpUtility.HtmlDecode(htmlNode.InnerText).Trim();
            SegmentText = System.Web.HttpUtility.HtmlDecode(codeNode.InnerText).Trim();
        }

        const string NameNodeClass = "list-header";
        const string CodeNodeClass = "code";

        public string Name { get; }
        public string SegmentText { get; }

        public HtmlNode NameHtmlNode { get; }
        public HtmlNode CodeHtmlNode { get; }

        public override string ToString()
        {
            return $"{Name}: [{SegmentText}]";
        }

        private IEnumerable<ISegment> _subSegments = null;

        public IEnumerable<ISegment> SubSegments => _subSegments;

        public static void PopulateSubSegments(IEnumerable<CodeNode> codes, IEnumerable<Terminal> terminals)
        {
            var codesArray = codes.ToArray();
            var termArray = terminals.ToArray();
            foreach(var c in codes)
            {
                PopulateSubSegments(c, codesArray, termArray);
            }
        }

        public bool IsPopulated { get; private set; }

        private static void PopulateSubSegments(CodeNode code, CodeNode[] allCodes, Terminal[] terminals)
        {
            if (code.IsPopulated) return;

            var segTexts = code.SegmentText.BreakIntoQuotedAndUnquoted();

            var segments = new List<ISegment>();

            foreach(var s in segTexts)
            {
                if (!s.isQuoted)
                {
                    segments.Add(new PlainTextSegment(s.segment));
                }
                else
                {
                    var terminal = terminals.FirstOrDefault(x => x.Name.Equals(s.segment, StringComparison.InvariantCultureIgnoreCase));
                    if(terminal != null)
                    {
                        segments.Add(terminal);
                    }
                    else
                    {
                        var c = allCodes.FirstOrDefault(x => x.Name.Equals(s.segment, StringComparison.InvariantCultureIgnoreCase));
                        if(c == null)
                        {
                            c = allCodes.FirstOrDefault(x => x.Name.Equals($"TJC.{s.segment}", StringComparison.InvariantCultureIgnoreCase));
                        }

                        if(c == null)
                        {
                            throw new Exception($"Couldn't find code named [{s.segment}]");
                        }
                        else
                        {
                            if (!c.IsPopulated)
                            {
                                PopulateSubSegments(c, allCodes, terminals);
                            }
                            segments.AddRange(c.SubSegments);
                        }
                    }
                }
            }

            code.IsPopulated = true;
        }

    }
}
