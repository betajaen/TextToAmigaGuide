using System;
using System.IO;
using System.Text;
using TextToAmigaGuide;

namespace TextToAmigaGuide
{
    partial class Program
    {
        /// <summary>
        /// Converts all text files (.txt) in a directory to an AmigaGuide.
        /// 
        /// Text files may contain some very minor markdown-like formatting:
        /// 
        /// Titles:
        /// 
        /// The first line of any text file is considered the title of the Node.
        /// 
        /// Headers:
        /// 
        /// # Header 1
        /// ## Header 2
        /// ### Header 3
        /// 
        /// Code Indention:
        ///   
        ///   Code blocks is indented by at least two spaces, and must 
        ///   start with an empty line.
        ///   
        /// Bold and Underline:
        ///  
        ///  This is some *bold* text and this is some _underline_ text.
        ///  
        /// Links:
        /// 
        ///  Please see the [Table of Contents](TOC) these can be also \[Escaped\].
        /// 
        /// 
        /// </summary>
        /// 
        /// <param name="input">The path to the directory of markdown files that is to be converted.</param>
        /// <param name="output">The name of the output from the conversion.</param>
        /// <param name="main">The name of the main page</param>
        static void Main(DirectoryInfo input, FileInfo output, string main = "MAIN")
        {
            if (input == null)
            {
                input = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            }

            if (output == null)
            {
                output = new FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "output.guide"));
            }

            GuideWriter writer = new GuideWriter();

            foreach (var fileInfo in input.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
            {

                string name = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name).ToUpper();

                if (main.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    name = "MAIN";
                }

                Node node = writer.GetNode(name);

                string source;

                using (StreamReader reader = fileInfo.OpenText())
                {
                    source = reader.ReadToEnd();
                }

                Render(source, node);

                Console.WriteLine($"--> {fileInfo.Name} is {node.Name} \"{node.Title}\"");

            }

            writer.Save(output);

            Console.WriteLine("Saved.");
        }

        static StringBuilder sb = new StringBuilder();

        static string EscapeAndHardwrap(string line)
        {
            sb.Length = 0;

            foreach (var ch in line)
            {

                if (ch == '@')
                    sb.Append("\\@");
                else if (ch == '\\')
                    sb.Append("\\\\");
                else if (char.IsControl(ch))
                { }
                else if (ch > 127)
                { }
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }

        static string Enclose(string line, char pattern, string start, string end)
        {
            sb.Length = 0;

            int isOpen = 0;
            bool lastSpace = true;
            bool escapeNext = false;
            foreach (var ch in line)
            {
                if (ch == '^')
                {
                    escapeNext = true;
                    continue;
                }

                if (escapeNext)
                {
                    if (ch == pattern)
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        sb.Append('^');
                        sb.Append(ch);
                    }

                    escapeNext = false;
                    continue;
                }

                if (ch == pattern)
                {
                    if (isOpen == 0 && lastSpace)
                    {
                        isOpen = 1;
                        sb.Append(start);
                    }
                    else if (isOpen == 1)
                    {
                        isOpen = 0;
                        sb.Append(end);
                    }
                    else
                    {
                        sb.Append(ch);
                    }

                    lastSpace = false;
                    continue;
                }

                sb.Append(ch);
                lastSpace = (ch == ' ');
            }

            if (isOpen == 1)
            {
                sb.Append(end);
            }

            return sb.ToString();
        }

        static string Link(string line)
        {
            sb.Length = 0;

            int isOpen = 0;
            bool escapeNext = false;
            bool lastSpace = true;
            foreach (var ch in line)
            {
                if (ch == '^')
                {
                    escapeNext = true;
                    continue;
                }

                if (escapeNext)
                {
                    if (ch == '[')
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        sb.Append('^');
                        sb.Append(ch);
                    }

                    escapeNext = false;
                    continue;
                }

                if (lastSpace && ch == '[' && isOpen == 0)
                {
                    sb.Append("@{\"");
                    isOpen = 1;
                    lastSpace = true;
                    continue;
                }
                else if (ch == ']' && isOpen == 1)
                {
                    sb.Append("\" LINK ");
                    isOpen = 2;
                    continue;
                }
                else if (ch == '(' && isOpen == 2)
                {
                    isOpen = 2;
                    continue;
                }
                else if (ch == ')')
                {
                    sb.Append("}");
                    isOpen = 0;
                    continue;
                }
                

                sb.Append(ch);
                lastSpace = (ch == ' ');
            }

            return sb.ToString();
        }

        internal static void Render(string source, Node node)
        {
            int firstNl = source.IndexOf('\n');
            node.Title = source.Substring(0, firstNl).Trim();

            string text = source.Substring(firstNl + 1).Trim();

            StringReader strReader = new StringReader(text);
            string line;

            Para p = node.Paragraph();
            bool hasData = false;
            bool codeBlock = false;
            bool lastEmpty = true;

            while (null != (line = strReader.ReadLine()))
            {
                line = EscapeAndHardwrap(line);

                if (string.IsNullOrWhiteSpace(line))
                {
                    p.Emit("\n");
                    hasData = true;
                    lastEmpty = true;
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    if (hasData)
                        p = node.Paragraph();

                    p.Span(line.Substring(1).Trim(), Colour.None, Colour.None, true, true, true);

                    p = node.Paragraph();
                    hasData = false;
                    lastEmpty = false;
                    continue;
                }

                if (line.StartsWith("## "))
                {
                    if (hasData)
                        p = node.Paragraph();

                    p.Span(line.Substring(2).Trim(), Colour.None, Colour.None, true, false, true);

                    p = node.Paragraph();
                    hasData = false;
                    lastEmpty = false;
                    continue;
                }

                if (line.StartsWith("### "))
                {
                    if (hasData)
                        p = node.Paragraph();

                    p.Span(line.Substring(3).Trim(), Colour.None, Colour.None, false, false, true);

                    p = node.Paragraph();
                    hasData = false;
                    lastEmpty = false;
                    continue;
                }

                if (line.StartsWith("#### "))
                {
                    if (hasData)
                        p = node.Paragraph();

                    p.Span(line.Substring(4).Trim(), Colour.None, Colour.None, false, false, false);

                    p = node.Paragraph();
                    hasData = false;
                    lastEmpty = false;
                    continue;
                }

                if (line.StartsWith("  ") && lastEmpty)
                {
                    if (hasData)
                        p = node.Paragraph();

                    p.Span(line, Colour.Text, Colour.None, false, false, false);

                    hasData = true;
                    codeBlock = true;
                    continue;
                }

                if (codeBlock)
                {
                    p = node.Paragraph();
                    codeBlock = false;
                }

                line = Link(line);
                line = Enclose(line, '_', "@{u}", "@{uu}");
                line = Enclose(line, '*', "@{b}", "@{ub}");

                p.Emit(line);
                p.Emit("\n");
                hasData = true;
                lastEmpty = false;
            }

        }


    }
}
