﻿using System;
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

            var dataCriteriaCodeNodes = DataCriteriaTerminal.GetAllLineLineItemsInDataCriteriaSection(root.OwnerDocument);
            // Nodes that start with dollar signs are structured more like regular code nodes, so we'll deal with them elsewhere
            foreach (var node in dataCriteriaCodeNodes.Where(x => x.InnerText.Trim().StartsWith("$")))
            {
                var name = node.InnerText.Split('=', ' ', '\t').First(x=>!string.IsNullOrWhiteSpace(x));
                result.Add(new CodeNode(name, node));
            }

            return result;
        }

        private CodeNode(string codeName, HtmlNode codeNode)
        {
            RawName = codeName;
            CodeHtmlNode = codeNode;
            Name = System.Web.HttpUtility.HtmlDecode(codeName).Trim();
            if (Name.Contains("("))
            {
                Name = Name.Substring(0, Name.IndexOf('('));
            }
            SegmentText = System.Web.HttpUtility.HtmlDecode(codeNode.InnerText).Trim();
        }

        private CodeNode(HtmlNode nameNode, HtmlNode codeNode) : this(nameNode.InnerText, codeNode)
        {

        }

        const string NameNodeClass = "list-header";
        const string CodeNodeClass = "code";

        public bool IsFunction { 
            get
            {
                var txt = RawName;
                if (txt.Contains("(") && txt.Contains("."))
                {
                    return true;
                }
                return false;
            } 
        }

        public string FunctionNameSpace
        {
            get
            {
                if (!IsFunction) return "";
                return RawName.Substring(0, RawName.IndexOf(".")).Trim();
            }
        }

        public string FunctionName
        {
            get
            {
                if (!IsFunction) return "";
                var txt = RawName.Trim();
                txt = txt.Substring($"{FunctionNameSpace}.".Length).Trim();
                txt = txt.Split('(').First();
                return txt;
            }
        }

        public string FunctionCallFormat => $"{FunctionNameSpace}.\"{FunctionName}\"";
        public string NormalizedFunctionCallFormat => $"\"{FunctionNameSpace}.{FunctionName}\"";

        public string Name { get; }
        public string SegmentText { get; }

        //public HtmlNode NameHtmlNode { get; }
        public string RawName { get; }
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
        // Used to help prevent stack overflows
        public bool IsBeingPopulated { get; private set; }

        private static void PopulateSubSegments(CodeNode code, CodeNode[] allCodes, Terminal[] terminals)
        {
            if (code.IsPopulated) return;
            code.IsBeingPopulated = true;

            var segTexts = code.SegmentText.RemoveComments().BreakIntoQuotedAndUnquoted(allCodes);

            var segments = new List<ISegment>();

            var prev = ("", false);
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
                        var prevTxt = prev.Item1;
                        

                        var c = allCodes.FirstOrDefault(x => x.Name.Equals(s.segment, StringComparison.InvariantCultureIgnoreCase));

                        if (c == null && !string.IsNullOrWhiteSpace(prevTxt) && prevTxt.Trim().EndsWith("."))
                        {
                            var prefix = prevTxt.Split(' ', '\r', '\n', '\t').Last();
                            c = allCodes.FirstOrDefault(x => x.Name.Equals($"{prefix}{s.segment}", StringComparison.InvariantCultureIgnoreCase));
                        }
                        if (c == null)
                        {
                            // Sometimes they leave off a prefix.  Grr.
                            c = allCodes.FirstOrDefault(x => x.Name.EndsWith($".{s.segment}", StringComparison.InvariantCultureIgnoreCase));
                        }
                        if (c == null)
                        {

                            // Check to see if there's a plurality issue.  For instance, hospital setting/hospital settings in CMS111v10
                            if (!s.segment.EndsWith("s"))
                            {
                                terminal = terminals.FirstOrDefault(x => x.Name.Equals(s.segment + "s", StringComparison.InvariantCultureIgnoreCase));
                            }
                            if (terminal != null)
                            {
                                segments.Add(terminal);
                            }
                            else
                            {
                                // TODO: Measurement Period is defined up in the top table.  Might want to parse this out.  For now just treating as plain text.
                                var skippableNames = new[] { "Measurement Period", "LengthInDays", "HospitalizationWithObservation",
                                    "Patient Characteristic Birthdate", "LowRiskDatetime", "Laboratory Test, Performed", "InitialADHDMedication", 
                                    "Medication", "Patient Characteristic Expired", "AgeInYearsAt", "Procedure, Order", "Medication, Active",
                                    "Date", "Medication, Administered: Hypoglycemics Treatment Medications",
                                    "Laboratory Test, Performed: Glucose Lab Test Mass Per Volume"};
                                if (skippableNames.Any(y => y.Equals(s.segment, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    segments.Add(new PlainTextSegment(s.segment));
                                }
                                else
                                {
                                    var tempFile = Path.Combine(Path.GetTempFileName() + ".html");
                                    File.WriteAllText(tempFile, code.CodeHtmlNode.OwnerDocument.ParsedText);

                                    using var fileopener = new System.Diagnostics.Process();

                                    fileopener.StartInfo.FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                                    fileopener.StartInfo.Arguments = "\"" + tempFile + "\"";
                                    fileopener.Start();

                                    throw new Exception($"Couldn't find code named [{s.segment}]");
                                }
                            }
                        }
                        else
                        {
                            if (!c.IsPopulated && !c.IsBeingPopulated)
                            {
                                PopulateSubSegments(c, allCodes, terminals);
                            }
                            else if (c.IsBeingPopulated)
                            {
                                // We have some sort of loop rather than a tree...                                
                                segments.Add(new PlainTextSegment(s.segment));
                            }

                            if (!c.IsBeingPopulated && !c.IsFunction)
                            {
                                segments.AddRange(c.SubSegments);
                            }
                            else if (!c.IsBeingPopulated && c.IsFunction)
                            {
                                segments.Add(new PlainTextSegment(s.segment));
                            }
                        }
                    }
                }
                prev = s;
            }
            code._subSegments = segments;
            code.IsBeingPopulated = false;
            code.IsPopulated = true;            
        }

        public string GetFormatted(int indentCount, string indent)
        {
            var sb = new StringBuilder();
            foreach(var s in SubSegments)
            {
                sb.Append(s.GetFormatted(indentCount + 1, indent));
            }
            var txt = sb.ToString();
            return txt;
        }
    }
}
