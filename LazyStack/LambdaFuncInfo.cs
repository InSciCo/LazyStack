using YamlDotNet.RepresentationModel;
namespace LazyStack
{
    public class LambdaFuncInfo
    {
        public LambdaFuncInfo(bool found, YamlMappingNode properties)
        {
            Found = found;
            Properties = properties;
        }
        public bool Found { get; set; }
        public YamlMappingNode Properties { get; set; }
    }
}
