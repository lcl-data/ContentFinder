
namespace ContentFinder
{
    class ContentUsage
    {
        /// <summary>
        /// how this content be used?
        /// </summary>
        public string usage { get; set; }
        /// <summary>
        ///  where is the content ?
        /// </summary>
        public Position position { get; set; }

        public class Position
        {
            public string Filename { get; set; }
            public int LineNum { get; set; }
            public string FileSuffix { get; set; }
            public Position(string file, int line)
            {
                this.Filename = file;
                this.LineNum = line;
                this.FileSuffix = new System.IO.FileInfo(file).Extension.ToUpperInvariant();
            }
        }

        public ContentUsage(string usage, Position pos)
        {
            this.usage = usage;
            this.position = pos;
        }

        public ContentUsage(string usage, string fileName, int line)
        {
            this.usage = usage;
            this.position = new Position(fileName, line);
        }
    }
}
