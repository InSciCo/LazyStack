namespace LazyStack
{
    public  class ProjectInfo
    {
        public ProjectInfo(string solutionFolder, string path, string relativePath, string folderPath)
        {
            SolutionFolder = solutionFolder;
            Path = path;
            RelativePath = relativePath;
            FolderPath = folderPath;
        }
        public string SolutionFolder { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderPath { get; set; }
    }
}
