using System;
using System.Linq;
using AngleSharp.Dom;
using NbaOracle.Extensions;

namespace NbaOracle.Infrastructure.AngleSharp;

public static class DocumentExtensions
    {
        public static string GetTextContent(this IElement element, string querySelector)
        {
            return element.QuerySelector(querySelector)?.TextContent;
        }
        
        public static string GetLastTextContent(this IElement element, string querySelector)
        {
            return element.QuerySelector(querySelector)?.TextContent?.Trim().Split(' ').Last();
        }
        
        public static string GetAttributeFromElement(this IElement element, string querySelector, string attributeName)
        {
            return element.QuerySelector(querySelector)?.GetAttribute(attributeName);
        }

        public static DateOnly GetAttributeFromElementAndRemoveLastCharactersAsDate(this IElement element, string querySelector, string attributeName, int numberOfTrailingCharactersToRemove)
        {
            var value = element.GetAttributeFromElement(querySelector, attributeName);
            value = value[..^numberOfTrailingCharactersToRemove];
            return DateOnly.FromDateTime(ParsingMethods.ToDate(value, "yyyyMMdd"));
        }

        public static string GetAttributeFromElementAndTakeLeadingCharacters(this IElement element, string querySelector, string attributeName, int numberOfLeadingCharactersToTake)
        {
            var value = element.GetAttributeFromElement(querySelector, attributeName);
            return value?.Substring(0, numberOfLeadingCharactersToTake);
        }

        public static DateTime GetAttributeFromElementAsDate(this IElement element, string querySelector, string attributeName)
        {
            var value = element.GetAttributeFromElement(querySelector, attributeName);
            return ParsingMethods.ToDate(value, "yyyyMMdd");
        }

        public static int GetTextContentAsInt(this IElement element, string querySelector)
        {
            return element.ParseTextContent(querySelector, ParsingMethods.ToInt);
        }

        public static decimal GetTextContentAsDecimal(this IElement element, string querySelector)
        {
            return element.ParseTextContent(querySelector, ParsingMethods.ToDecimal);
        }
        
        public static int GetTextContentAsIntAndRemoveSpecialCharacters(this IElement element, string querySelector)
        {
            var value = element.GetTextContent(querySelector);
            value = value.Replace(",", "");
            return string.IsNullOrWhiteSpace(value) ? default : ParsingMethods.ToInt(value);
        }

        public static DateTime ToDate(this IElement element, string querySelector)
        {
            return element.ParseTextContent(querySelector, x => ParsingMethods.ToDate(x, "yyyy"));
        }

        private static T ParseTextContent<T>(this IElement element, string querySelector, Func<string, T> parseFunction)
        {
            var value = element.GetTextContent(querySelector);
            return string.IsNullOrWhiteSpace(value) ? default : parseFunction(value);
        }
    }