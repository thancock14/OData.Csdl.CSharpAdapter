using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using Microsoft.OData.Edm;

namespace EdmModelConverter
{
    class Program
    {
        static void Main(string[] args)
        { 
            CSharpGenerator gen = new CSharpGenerator(Fetcher.GetAsStringAsyc().Result);
            IEdmModel model = gen.GetModel();
            Console.WriteLine(gen.GetCsharpGeneratedClass(model));
        }
    }
}
