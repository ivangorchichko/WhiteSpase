using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhiteSpaceWarrior
{
    public class CSharpCompressors
    {
        Options Options { get; }

        private readonly Regex _emptyParamRe;
        private readonly Regex _singleLineEmptySummaryRe;
        private static Regex[] _removeTagRegexs;

        static readonly RegexOptions options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant;
        static readonly RegexOptions OptionsIgnoreCase = options | RegexOptions.IgnoreCase;

        private static readonly Regex EmptyTypeParam = new Regex(@"[ \t]+/// <typeparam name\s*=\s*""[^""]*"">\s*</typeparam>[^\n]*\n", options);
        private static readonly Regex EmptyReturns = new Regex(@"[ \t]+/// <returns>\s*</returns>[^\n]*\n", options);
        private static readonly Regex EmptyNewlineAfterCurlyStart = new Regex(@"(?<curly>{[ \t]*\r?\n)([ \t]*\r?\n)+", options);
        private static readonly Regex EmptyLinesBeforeCurlyEnd = new Regex(@"\n([ \t]*\r?\n)+(?<indentedCurly>[ \t]*})", options);
        private static readonly Regex VersionHistory = new Regex(@"( |\t)*#region Version History( |\t)*((?>\s*//[^\n]*))*\s*#endregion( |\t)*(\r\n?|\n)+", OptionsIgnoreCase);
        private static readonly Regex SingleLineSummary = new Regex(@"(?<indent>[ \t]+)/// <summary>[ \t]*\r?\n[ \t]+///[ \t]*(?<comment>[^\r\n]*)\r?\n[ \t]+/// </summary>[^\n]*\n", options);
        private static readonly Regex MultilineEmptySummary = new Regex(@"[ \t]+/// <summary>[ \t]*\r?\n([ \t]+///[ \t]*\r?\n)*[ \t]+/// </summary>[^\n]*\n", options);
        private static readonly Regex MultilinePropertiesGetSet = new Regex(@"\s*\{\s+get;\s+(?<modifier>protected |private )?set;\s+\}", options);
        private static readonly Regex MultilinePropertiesSetGet = new Regex(@"\s*\{\s+(?<modifier>protected |private )?set;\s+get;\s+\}", options);
        private static readonly Regex RegionStartEnd = new Regex("[ \t]*(#region([ \t]*\\w*)+|#endregion[ \t]*)(\r?\n|\\Z)", options);
        private static readonly Regex OldStyleMethodSeparator = new Regex(@"(\r?\n){2,}[ \t]*///////+[ \t]*\r?\n(\r?\n)+", options);
        private static readonly Regex OldStyleMethodSeparatorPreprocessorDirectives = new Regex(@"(?<=(#if|#region) \w*[ \t]*(\r?\n))[ \t]*///////+[ \t]*\r?\n(\r?\n)+", options);
        private static readonly Regex OldStyleMethodSeparatorEndPreprocessorDirectives = new Regex(@"(\r?\n){2,}[ \t]*///////+[ \t]*\r?\n[ \t]*(?=(#endif|#endregion))", options);

        public CSharpCompressors(Options options)
        {
            Options = options;

            _emptyParamRe = new Regex($@"[ \t]+/// <param name\s*=\s*""[^""]*"">\s*(\w*\s*){{0,{Options.RemoveParamNameUptoNWords}}}\.?\s*</param>[^\n]*\n", CSharpCompressors.options);
            _singleLineEmptySummaryRe = new Regex($@"[ \t]+/// <summary>\s*(\w*\s*){{0,{Options.RemoveSummaryUptoNWords}}}\.?\s*</summary>[^\n]*\n", CSharpCompressors.options);

        }

        public string Compress(string content)
        {
            content = OldStyleMethodSeparators(content); // must be before #region removal

            if (Options.RemoveRegions)
                content = RegionStartEnd.Replace(content, "");

            content = CompressProperties(content);
            content = VersionHistory.Replace(content, "");

            content = CompressParam(content);
            content = EmptyReturns.Replace(content, "");
            content = CompressSummary(content);

            content = RemoveTags(content);

            content = CompressCurlyBracketNewlines(content);

            return content;
        }

        private string RemoveTags(string content)
        {
            if (_removeTagRegexs == null)
            {
                string RemoveTagString = (@"[ \t]*///[ \t]*<[ \t]*{0}.*?</{0}>[ \t]*\r?\n");
                _removeTagRegexs = Options.RemoveTags
                    .Select(x => new Regex((string.Format(RemoveTagString, x)), options))
                    .ToArray();
            }

            foreach (var tag in _removeTagRegexs)
            {
                content = tag.Replace(content, "");
            }

            return content;
        }

        private static string OldStyleMethodSeparators(string content)
        {
            content = OldStyleMethodSeparator.Replace(content, Environment.NewLine + Environment.NewLine);
            content = OldStyleMethodSeparatorPreprocessorDirectives.Replace(content, Environment.NewLine);
            content = OldStyleMethodSeparatorEndPreprocessorDirectives.Replace(content, Environment.NewLine + Environment.NewLine);
            return content;
        }

        private static string CompressProperties(string content)
        {
            content = MultilinePropertiesGetSet.Replace(content, @" { get; ${modifier}set; }");
            content = MultilinePropertiesSetGet.Replace(content, @" { get; ${modifier}set; }");
            return content;
        }


        private string CompressSummary(string content)
        {
            content = SingleLineSummary.Replace(content, "${indent}/// <summary> ${comment} </summary>" + Environment.NewLine);
            content = _singleLineEmptySummaryRe.Replace(content, "");
            content = MultilineEmptySummary.Replace(content, "");

            return content;
        }

        private string CompressParam(string content)
        {
            content = _emptyParamRe.Replace(content, "");
            content = EmptyTypeParam.Replace(content, "");

            return content;
        }


        private string CompressCurlyBracketNewlines(string content)
        {
            content = EmptyNewlineAfterCurlyStart.Replace(content, "${curly}");
            content = EmptyLinesBeforeCurlyEnd.Replace(content, "\n${indentedCurly}");

            return content;
        }
    }
}
