using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CQMParse
{
    public static class HtmlExtensions
    {
        public static HtmlNode GetMostRecentCommonAncestor(this HtmlNode node1, HtmlNode node2)
        {
            if (node1 == node2) return node1;

            var ancestors1 = node1.AncestorsAndSelf();
            var ancestors2 = node2.AncestorsAndSelf();

            var common = ancestors1.Intersect(ancestors2).ToArray();

            var mostRecent = common.OrderByDescending(y => y.StreamPosition).First();

            return mostRecent;            
        }

        public static HtmlNode GetNextElementWithClass(this HtmlNode node,string className)
        {
            var all = node.OwnerDocument.DocumentNode.DescendantsAndSelf().ToList();
            var index = all.IndexOf(node);
            var result = GetNextElementWithClass(className, all.ToArray(), index);
            return result;
        }

        public static HtmlNode GetNextElementWithClass(string className, HtmlNode[] allDocumentNodes, long nodeIndex)
        {
            for(long i=nodeIndex + 1; i<allDocumentNodes.Length; i++)
            {
                var n = allDocumentNodes[i];
                if (n != null && n.HasClass(className)) return n;
            }
            return null;
        }

        public static HtmlNode GetNextElementWithName(this HtmlNode node, string name)
        {
            var all = node.OwnerDocument.DocumentNode.DescendantsAndSelf().ToList();
            var index = all.IndexOf(node);
            var result = GetNextElementWithName(name, all.ToArray(), index);
            return result;
        }

        public static HtmlNode GetNextElementWithName(string name, HtmlNode[] allDocumentNodes, long nodeIndex)
        {
            for (long i = nodeIndex + 1; i < allDocumentNodes.Length; i++)
            {
                var n = allDocumentNodes[i];
                if (n != null && n.HasName(name)) return n;
            }
            return null;
        }

        public static bool HasName(this HtmlNode node, string name)
        {
            if (node == null) return false;
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (string.IsNullOrWhiteSpace(node.Name)) return false;
            return node.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
