using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CQMParse
{
    public class DataCriteriaTerminal : Terminal
    {
        /// <summary>
        /// The Code or ValueSet that the criteria references
        /// </summary>
        public TerminologyTerminal Using { get; set; }
        /// <summary>
        /// The name of the Code or Valueset that the criteria references
        /// </summary>
        public string UsingName { get; set; }

        public override IEnumerable<ISegment> SubSegments => new[] { this };

        public DataCriteriaTerminal(HtmlNode node, IEnumerable<TerminologyTerminal> allTerminals) : base(node)
        {
            var splitToken = "\" using \"";


            if (!SegmentText.Contains(splitToken))
            {
                throw new ArgumentException($"DataCriteriaTerminal text is expected to contain [{splitToken}], but was [{SegmentText}]");
            }
            var split = SegmentText.Split(splitToken);
            if(split.Length > 2)
            {
                throw new ArgumentException($"DataCriteriaTerminal text is expected to contain a single [{splitToken}], but was [{SegmentText}], containing {split.Length - 1}");
            }
            
            Name = split.First().TrimStart('"');
            UsingName = split.Last().TrimEnd('"').Trim();
            if (UsingName.EndsWith(')'))
            {
                var usingCode = UsingName.GetParentheticalSections().Last();

                UsingName = UsingName.Substring(0, UsingName.Length - 1).Trim();
                if (UsingName.Contains('('))
                {
                    UsingName = UsingName.Substring(0, UsingName.LastIndexOf('(')).Trim();
                }
            }

            Using = allTerminals.FirstOrDefault(x => x.Name.Equals(UsingName));
        }

        public override string ToString()
        {
            return $"Data Criteria: [{Name}] USING [{Using}]";
        }
    }
    
    public class CodeTerminology : TerminologyTerminal
    {
        public const string CodeToken = "code";
        public override string Token => CodeToken;
        public CodeTerminology(HtmlNode node) : base(node) {}

        public override string ToString()
        {
            return $"Code: [{Name}] Value [{Code}]";
        }
    }

    public class ValueSetTerminology : TerminologyTerminal
    {
        public const string ValueSetToken = "valueset";
        public override string Token => ValueSetToken;
        public ValueSetTerminology(HtmlNode node) : base(node) {}

        public override string ToString()
        {
            return $"ValueSet: [{Name}] Set [{Code}]";
        }
    }

    public abstract class TerminologyTerminal : Terminal
    {
        public static TerminologyTerminal Create(HtmlNode node)
        {
            var text = System.Web.HttpUtility.HtmlDecode(node.InnerText).Trim();

            const string codeToken = "code";
            const string valueSetToken = "valueset";

            if (text.StartsWith(CodeTerminology.CodeToken, StringComparison.InvariantCultureIgnoreCase))
            {
                return new CodeTerminology(node);
            }
            else if (text.StartsWith(ValueSetTerminology.ValueSetToken, StringComparison.InvariantCultureIgnoreCase))
            {
                return new ValueSetTerminology(node);
            }
            return null;
        }

        public abstract string Token { get; }
        public string Code { get; protected set; }
        public override IEnumerable<ISegment> SubSegments => new[] { this };


        public TerminologyTerminal(HtmlNode root) : base(root)
        {
            Name = SegmentText.GetQuotedSections().First();
            Code = SegmentText.GetParentheticalSections().Last();
        }
    }

    public abstract class Terminal : ISegment
    {
        public abstract IEnumerable<ISegment> SubSegments { get; }

        public static List<Terminal> GetTerminals(HtmlNode root)
        {
            const string NameNodeClass = "list-header";
            
            var result = new List<Terminal>();
            var allNodes = root.Descendants().ToArray();

            var terms = new List<TerminologyTerminal>();
            var criteria = new List<DataCriteriaTerminal>();

            var h3s = allNodes.Where(x => x.Name.Equals("h3", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            for (int i = 0; i < allNodes.Length; i++)
            {
                var node = allNodes[i];
                if (node.HasName("h3") && node.InnerText.Equals("Terminology", StringComparison.InvariantCultureIgnoreCase))
                {
                    var ul = node.GetNextElementWithName("ul");
                    var lis = ul.Descendants().Where(x => x.HasName("li")).ToArray();                    
                    terms.AddRange(lis.Select(y=> TerminologyTerminal.Create(y)));
                }
            }

            for (int i = 0; i < allNodes.Length; i++)
            {
                var node = allNodes[i];
                if (node.HasName("h3")
                    && (node.InnerText.Contains("Data Criteria", StringComparison.InvariantCulture)
                    || node.InnerText.Contains("QDM Data Elements")))
                {
                    var ul = node.GetNextElementWithName("ul");
                    var lis = ul.Descendants().Where(x => x.HasName("li")).ToArray();                    
                    criteria.AddRange(lis.Select(y => new DataCriteriaTerminal(y, terms)));
                }
            }

            result.AddRange(terms);
            result.AddRange(criteria);

            return result;
        }


        protected Terminal(HtmlNode htmlNode)
        {            
            Node = htmlNode;
            SegmentText = System.Web.HttpUtility.HtmlDecode(Node.InnerText).Trim();
        }


        public string SegmentText { get; }
        public HtmlNode Node { get; }
        public string Name { get; protected set; }
    }
}
