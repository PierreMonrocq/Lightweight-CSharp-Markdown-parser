/*
MIT License

Copyright (c) 2020 Pierre Monrocq

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace markdownParser
{
    class Parser
	{

        private static string[] patterns = new string[] { @"!\[([^\[]+)\]\(([^\)]+)\)", @"\[([^\[]+)\]\(([^\)]+)\)", @"(\*\*|__)(.*?)\1", @"(\*|_)(.*?)\1", @"\~\~(.*?)\~\~", @"\:\'(.*?)\'\:", @"`(.*?)`" };
        private static string[] remplacements = new string[] { @"<img src=\'$2\' alt=\'$1\'>", @"<a href=\'$2\'>$1</a>", @"<strong>$2</strong>", @"<em>$2</em>", @"<del>$2</del>", @"<q>$1</q>", @"<code>$1</code>" };
        //images, link, bold, emphasis, del, quote, code

        private static string Title(string input)
        {
            string pattern = @"(#+)(.*)";
            while (Regex.Match(input, pattern).Success)
            {
                string tmp = input;
                string[] res = Regex.Split(tmp, pattern);
                int size = res[1].Length;
                if (size > 0)
                {
                    string header = string.Format(@"<h{0}>{1}</h{0}>", size, res[2].Trim());
                    Regex rgx = new Regex(pattern);
                    input = rgx.Replace(input, header, 1);
                }
            }
            return input;
        }

        private static string Paragraph(string input)
        {
            StringBuilder sb = new StringBuilder();
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for(int i=0; i<lines.Length;i++)
            {
                string l = lines[i];
                
                if (Regex.IsMatch(l, @"^</?(li|h|p|bl|ul|ol)"))
                {
                    sb.Append(l);
                }
                else
                {
                    string p = Regex.Replace(l, @"\n([^\n]+)\n", "");
                    sb.Append(string.Format("\n<p>{0}</p>\n", p));
                } 
            }
            return sb.ToString();
        }

        public static string Render(string text)
        {
            text = Title(text);
            for (var i = 0; i < patterns.Length; i++)
            {
                text = Regex.Replace(text, patterns[i], remplacements[i]);
            }
            text = ParsePattern(text, "\n<ul>\n<li>{0}</li>\n</ul>", @"\n\*(.*)", 1, @"\n<\/ul>\s?<ul>");
            text = ParsePattern(text, "\n<ol>\n<li>{0}</li>\n</ol>", @"\n[0-9]+\.(.*)", 1, @"\n<\/ol>\s?<ol>");
            text = ParsePattern(text, "\n<ul>\n<li>{0}</li>\n</ul>", @"\n\*(.*)", 1, @"\n<\/ul>\s?<ul>");
            text = ParsePattern(text, "\n<blockquote>{0}</blockquote>", @"\n(&gt;|\>)(.*)", 2, @"\n<\/blockquote><blockquote");
            text = Regex.Replace(text, @"\n-{5,}","\n <hr/>");
            text = Paragraph(text);
            return text;
        }

        private static string ParsePattern(String input, String format, String pattern, int selection, String fix)
        {
            while (Regex.Match(input, pattern).Success)
            {
                string tmp = input;
                string header = string.Format(format, Regex.Split(tmp, pattern)[selection].Trim());
                Regex rgx = new Regex(pattern);
                input = rgx.Replace(input, header, 1);
            }
            input = Regex.Replace(input, fix, "");
            return input;
        }
    }
}
