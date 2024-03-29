﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQMParse
{
    public static class StringExtensions
    {
        private static string FormatFunctionCalls(string s, IEnumerable<CodeNode> allCodes)
        {
            var functions = allCodes.Where(x => x.IsFunction).ToArray();
            foreach(var f in functions)
            {
                s = s.Replace(f.FunctionCallFormat, f.NormalizedFunctionCallFormat);
            }
            return s;
        }

        public static string RemoveComments(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var commentStart = str.IndexOf("/*");
            if (commentStart < 0) return str;
            var commentEnd = str.Substring(commentStart).IndexOf("*/");

            while(commentStart >= 0 && commentEnd > commentStart)
            {
                str = str.Substring(0, commentStart) + str.Substring(commentEnd + 2);
                commentStart = str.IndexOf("/*");
                if (commentStart >= 0)
                {
                    commentEnd = str.Substring(commentStart).IndexOf("*/");
                }
                else
                {
                    commentEnd = -1;
                }
            }

            return str;            
        }

        public static IEnumerable<(string segment, bool isQuoted)> BreakIntoQuotedAndUnquoted(this string s, IEnumerable<CodeNode> allCodes)
        {
            s = FormatFunctionCalls(s, allCodes);
            s = s.Replace("\": \"", ": ");
            var results = new List<(string segment, bool isQuoted)>();

            int openCount = 0;
            var sb = new StringBuilder();

            char opening = '"';
            char closing = '"';


            foreach (var c in s)
            {
                if (openCount > 0)
                {
                    if (c == closing)
                    {
                        openCount--;

                        if (openCount == 0)
                        {
                            var t = $"{sb}";
                            results.Add((t,true));
                            sb.Clear();
                            if (t == null) t = "";
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    else if (c == opening)
                    {
                        openCount++;
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append(c);
                    }

                }
                else // closed
                {
                    if (c == opening)
                    {
                        if (!string.IsNullOrEmpty($"{sb}"))
                        {
                            results.Add(($"{sb}", false));
                        }
                        sb.Clear();
                        openCount++;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            if (!string.IsNullOrEmpty($"{sb}"))
            {
                results.Add(($"{sb}", false));
            }
            return results.ToArray();
        }

        private static string[] GetDelimitedSections(this string s, char opening, char closing)
        {
            var results = new List<string>();

            int openCount = 0;
            var sb = new StringBuilder();


            foreach(var c in s)
            {
                if (openCount > 0)
                {
                    if (c == closing)
                    {
                        openCount--;

                        if (openCount == 0)
                        {
                            var t = $"{sb}";
                            results.Add(t);
                            sb.Clear();
                            if (t == null) t = "";
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    else if (c == opening)
                    {
                        openCount++;
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append(c);
                    }

                }
                else // closed
                {
                    if(c == opening)
                    {
                        sb.Clear();
                        openCount++;
                    }
                }
            }

            return results.ToArray();
        }

        public static string[] GetQuotedSections(this string s)
        {
            return GetDelimitedSections(s, '"', '"');
        }

        public static string[] GetParentheticalSections(this string s)
        {
            return GetDelimitedSections(s, '(', ')');
        }
    }
}
