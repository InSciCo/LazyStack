namespace LazyStack
{
    public  class ProjectInfo
    {
        public ProjectInfo(string solutionFolder, string path, string relativePath)
        {
            SolutionFolder = solutionFolder;
            Path = path;
            RelativePath = relativePath;
        }
        public string SolutionFolder { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }
    }
}
