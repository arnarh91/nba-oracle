using System;
using NbaOracle.ValueObjects;

namespace NbaOracle.Infrastructure.FileSystem
{
    public class FileContent
    {
        public FileContent(string url, string filePath, string content)
        {
            RetrievedTime = SystemTime.Now();
            Url = url;
            FilePath = filePath;
            Content = content;
        }

        public DateTime RetrievedTime { get; }
        public string Url { get; }
        public string FilePath { get; }
        public string Content { get; }
    }
}