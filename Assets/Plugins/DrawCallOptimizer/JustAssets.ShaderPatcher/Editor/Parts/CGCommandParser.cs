using System;
using System.Collections.Generic;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    public static class CGCommandParser
    {
        public static IEnumerable<IPart> Parse(StringSpan formula, int indentLevel, string lineEnding, ICollection<string> additionalMacros)
        {
            if (formula.IsAtEnd)
                yield break;

            var macroWords = new List<string>(additionalMacros) {"INTERNAL_DATA"};

            var parts = new List<IPart>();

            var whitespace = "";
            var wordCache = "";
            do
            {
                var peek = formula.IsAtEnd ? '\0' : formula.Peek();
                if (Char.IsWhiteSpace(peek))
                {
                    whitespace += peek;
                    formula.Offset++;
                    continue;
                }

                if (whitespace.CountOccurrences(lineEnding) >= 2)
                {
                    Formula submit = Submit(parts, lineEnding, indentLevel);
                    if (submit != null)
                        yield return submit;

                    yield return new Formula(new List<IPart>(), lineEnding, indentLevel);
                }

                var whitespaceBeforeLastWord = whitespace;
                whitespace = "";

                if (!String.IsNullOrWhiteSpace(wordCache))
                {
                    switch (peek)
                    {
                        case '(':
                        {
                            formula.Offset++;
                            var parameters = formula.ReadBalanced(')', '(');
                            var substring = parameters.Substring(0, parameters.Length - 1);
                            parts.Add(new MethodHeader(wordCache, substring, lineEnding, macroWords, 0));
                            break;
                        }
                        default:
                        {
                            parts.Add(new Variable(wordCache));
                            break;
                        }
                    }

                    if (macroWords.Contains(wordCache) && whitespaceBeforeLastWord.Any(x => x == '\r' || x == '\n'))
                    {
                        //parts.Add(new CommandEndToken(indentLevel, lineEnding));
                        Formula submit = Submit(parts, lineEnding, indentLevel);
                        if (submit != null)
                            yield return submit;
                    }

                    wordCache = null;
                    continue;
                }

                if (peek == '#')
                {
                    Formula submit = Submit(parts, lineEnding, indentLevel);
                    if (submit != null)
                        yield return submit;
                    yield return new Formula(new List<IPart> {new Untouched(formula.ReadLine().RemoveLineEnding(), 0)},
                        lineEnding, 0);

                    continue;
                }

                if (peek == '{')
                {
                    formula.ReadBeforeAndStay(c => Char.IsWhiteSpace(c) && c != '\r' && c != '\n');

                    formula.Offset++;
                    var methodBodyText = formula.ReadBalanced('}', '{');
                    methodBodyText = methodBodyText.Substring(0, methodBodyText.Length - 1);
                    bool isStructDefinition = !formula.IsAtEnd && formula.Peek() == ';';

                    var groups = parts.GroupBy(x => x.GetType()).ToDictionary(x => x.Key, x => x.ToList());

                    var methodRequirements = new List<Type> {typeof(Variable), typeof(MethodHeader), typeof(SyntaxToken)};
                    var methodMandatory = new List<Type> {typeof(Variable), typeof(MethodHeader)};

                    if (groups.Keys.Except(methodRequirements).Any() 
                        || methodMandatory.Except(groups.Keys).Any() 
                        || groups.TryGetValue(typeof(SyntaxToken), out var tokens) && (tokens.Count > 1 || tokens[0].Cast<SyntaxToken>().Token != ":"))
                    {
                        if (isStructDefinition)
                        {
                            parts.Clear();
                            yield return new Struct(groups, methodBodyText, macroWords, lineEnding, indentLevel);
                        }
                        else
                        {
                            Formula submit = Submit(parts, lineEnding, indentLevel);
                            if (submit != null)
                                yield return submit;
                            yield return new Formula(new List<IPart> {new Content(methodBodyText, indentLevel, lineEnding, macroWords)}, lineEnding, indentLevel);
                        }
                    }
                    else
                    {
                        parts.Clear();
                        yield return new Method(groups, new Content(methodBodyText, indentLevel, lineEnding, macroWords), lineEnding, indentLevel);
                    }

                    if (isStructDefinition)
                        formula.Read();

                    wordCache = null;
                    continue;
                }

                if (formula.TryReadNumeric(out var numeric))
                {
                    parts.Add(new Numeric(numeric, indentLevel));
                    continue;
                }

                if (peek == '.')
                {
                    formula.Offset++;
                    parts.Add(new MemberAccessor());
                    continue;
                }

                wordCache = formula.ReadWhile();

                if (String.IsNullOrWhiteSpace(wordCache))
                {
                    formula.Offset++;
                    switch (peek)
                    {
                        case '(':
                        {
                            var parameters = formula.ReadBalanced(')', '(');
                            var substring = parameters.Substring(0, parameters.Length - 1);
                            parts.Add(new Brackets(new StringSpan(substring), lineEnding, macroWords, 0));
                            break;
                        }
                        case ';':
                            parts.Add(new CommandEndToken(";"));
                            Formula submit = Submit(parts, lineEnding, indentLevel);
                            if (submit != null)
                                yield return submit;

                            break;
                        default:
                            var readWhile = peek + formula.ReadWhile("[\\+\\-*/=]+");

                            if (readWhile == "//")
                            {
                                Formula submitNow = Submit(parts, lineEnding, indentLevel);
                                if (submitNow != null)
                                    yield return submitNow;
                                yield return new Formula(
                                    new List<IPart> {new Untouched(readWhile + formula.ReadToLineEnd().RemoveLineEnding(), 0)},
                                    lineEnding, indentLevel);
                            }
                            else
                                parts.Add(new SyntaxToken(readWhile));
                            break;
                    }
                }
            } while (!formula.IsAtEnd || !String.IsNullOrWhiteSpace(wordCache));

            if (parts.Count > 0)
            {
                yield return new Formula(parts, lineEnding, indentLevel);
            }
        }

        private static Formula Submit(List<IPart> parts, string lineEnding, int indentLevel)
        {
            if (parts.Count == 0)
                return null;

            var formula = new Formula(parts, lineEnding, indentLevel);

            if (formula.Serialize().All(char.IsWhiteSpace))
                throw new ArgumentException();

            parts.Clear();
            return formula;
        }
    }
}