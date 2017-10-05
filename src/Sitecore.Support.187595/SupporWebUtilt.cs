
namespace Sitecore.Support.Web.WebUtil
{
    using HtmlAgilityPack;

    public class SupporWebUtil
    {
        private static readonly System.Text.RegularExpressions.Regex cssExpressionPattern = new System.Text.RegularExpressions.Regex("<[a-zA-Z0-9]+[^>]*?style=['\"]?.*?(?<cssRule>[^;\"]+: expression(?<bracket>\\())", System.Text.RegularExpressions.RegexOptions.Compiled);

        public static string RemoveAllScripts(string content, bool removeScriptTags, bool removeInlineScripts, bool removeCssExpressions)
        {
            string text = string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                return text;
            }
            System.Collections.Generic.List<string> cdatas = new System.Collections.Generic.List<string>();
            content = System.Text.RegularExpressions.Regex.Replace(content, "<!\\[CDATA\\[.*?\\]\\]>", delegate (System.Text.RegularExpressions.Match match)
            {
                cdatas.Add(match.Value);
                return string.Concat(new object[]
                {
            "<cdata_sc",
            cdatas.Count,
            "></cdata_sc",
            cdatas.Count,
            ">"
                });
            });
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);
            HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//*");
            if (htmlNodeCollection != null)
            {
                for (int i = 0; i < htmlNodeCollection.Count; i++)
                {
                    HtmlNode htmlNode = htmlNodeCollection[i];
                    if (htmlNode != null)
                    {
                        if (removeScriptTags && htmlNode.Name == "script")
                        {
                            htmlNode.Remove();
                        }
                        else if (removeInlineScripts)
                        {
                            if (htmlNode.Attributes["allowscriptaccess"] != null)
                            {
                                htmlNode.Attributes["allowscriptaccess"].Remove();
                            }
                            if (htmlNode.Attributes["srcdoc"] != null)
                            {
                                htmlNode.Attributes["srcdoc"].Remove();
                            }
                            for (int j = 0; j < htmlNode.Attributes.Count; j++)
                            {
                                HtmlAttribute htmlAttribute = htmlNode.Attributes[j];
                                if (htmlAttribute.Name.StartsWith("on") || htmlAttribute.Value.Contains("javascript") || htmlAttribute.Value.Contains("base64"))
                                {
                                    HtmlAttribute htmlAttribute2 = htmlNode.Attributes[htmlAttribute.Name];
                                    if (htmlAttribute2 != null)
                                    {
                                        htmlAttribute2.Remove();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            using (System.IO.StringWriter stringWriter = new System.IO.StringWriter())
            {
                htmlDocument.Save(stringWriter);
                text = stringWriter.ToString();
            }
            for (int k = 0; k < cdatas.Count; k++)
            {
                int i1 = k;
                text = System.Text.RegularExpressions.Regex.Replace(text, string.Concat(new object[]
                {
            "<cdata_sc",
            k + 1,
            "></cdata_sc",
            k + 1,
            ">"
                }), (System.Text.RegularExpressions.Match match) => cdatas[i1]);
            }
            if (removeCssExpressions)
            {
                text = SanitizeCssExpression(text);
            }
            return text;
        }

        private static string SanitizeCssExpression(string input)
        {
            System.Text.RegularExpressions.MatchCollection matchCollection = cssExpressionPattern.Matches(input);
            for (int i = matchCollection.Count - 1; i >= 0; i--)
            {
                input = SanitizeExpression(input, matchCollection[i]);
            }
            return input;
        }

        private static string SanitizeExpression(string input, System.Text.RegularExpressions.Match m)
        {
            int index = m.Groups["cssRule"].Index;
            int index2 = m.Groups["bracket"].Index;
            input = StripMatchingBracketsWithContent(input, index2);
            input = input.Remove(index, index2 - index);
            if (input[index] == ';')
            {
                input = input.Remove(index, 1);
            }
            return input;
        }
        private static string StripMatchingBracketsWithContent(string input, int startIndex)
        {
            int num = 0;
            do
            {
                char c = input[startIndex];
                if (c == '"' || c == '\'' || c == '/')
                {
                    input = SanitizeInPairChars(input, startIndex, c);
                    c = input[startIndex];
                }
                input = input.Remove(startIndex, 1);
                if (c == '(')
                {
                    num++;
                }
                else if (c == ')')
                {
                    num--;
                }
            }
            while (num > 0 && input.Length > startIndex);
            return input;
        }

        private static string SanitizeInPairChars(string input, int startIndex, char currentChar)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(currentChar + ".*?(?<!\\\\)" + currentChar);
            string input2 = input.Substring(startIndex);
            input = input.Substring(0, startIndex) + regex.Replace(input2, string.Empty, 1);
            return input;
        }
    }

}