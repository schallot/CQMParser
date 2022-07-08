using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CQMParse
{
    public class CodeNode
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
            Code = System.Web.HttpUtility.HtmlDecode(codeNode.InnerText).Trim();
        }

        const string NameNodeClass = "list-header";
        const string CodeNodeClass = "code";

        public string Name { get; }
        public string Code { get; }

        public HtmlNode NameHtmlNode { get; }
        public HtmlNode CodeHtmlNode { get; }

        public override string ToString()
        {
            return $"{Name}: [{Code}]";
        }

    }
}
