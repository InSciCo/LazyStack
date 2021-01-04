using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace TestXmlMerge
{
    class Program
    {
        static void Main(string[] args)
        {
            var file1Path = @"C:\Users\tmay\source\repos\LazyStack\TestXmlMerge\XMLFile1.xml";
            var file2Path = @"C:\Users\tmay\source\repos\LazyStack\TestXmlMerge\XMLFile2.xml";
            var file3Path = @"C:\Users\tmay\source\repos\LazyStack\TestXmlMerge\XMLFile3.xml";

            var xml1 = XDocument.Load(file1Path);
            var xml2 = XDocument.Load(file2Path);

            var xmls = new List<XDocument>
            {
                 XDocument.Load(file1Path),
                 XDocument.Load(file2Path)
             };

            var xml1descendants = xml1.Descendants("Project");
            var xml2descendants = xml2.Descendants("ItemGroup");

            var combined = xml1descendants.Union(xml2descendants);
            ;
            //var result = xml1.Descendants("Project").Union(xml2descendents);
            //new XDocument(result).Save(file3Path);


            //var result = new XDocument(
            //    new XElement("Project", xmls.Descendants("Project")));
            //result.Save(file3Path);

            //Console.WriteLine("Hello World!");
            //Console.ReadLine();
        }
    }


}
