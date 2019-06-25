using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;

namespace EdmModelConverter
{
    public class CSharpGenerator
    {
        private string _metadataString;
        public CSharpGenerator(string metadata)
        {
            this._metadataString = metadata;
        }

        public IEdmModel GetModel()
        {
            var stringReader = new StringReader(_metadataString);
            var reader = XmlReader.Create(stringReader);
            var model = CsdlReader.Parse(reader);
            return model;
        }

        public string GetCsharpGeneratedClass(IEdmModel model)
        {
            StringBuilder str = new StringBuilder();
            foreach(var entity in model.EntityContainer.EntitySets())
                str.Append(ProcessEntity(entity.EntityType()));

            IEnumerable<IEdmSchemaType> schemaElements = model.SchemaElements.OfType<IEdmSchemaType>();
            foreach (var type in schemaElements.ToList())
            {
                if(type.TypeKind == EdmTypeKind.Complex)
                    str.Append(ProcessComplexType(type));
            }
                
            return str.ToString();
        }

        private string ProcessComplexType(IEdmSchemaType complexType)
        {
            StringBuilder str = new StringBuilder();
             var structuredType = complexType as IEdmStructuredType;
            var properties = structuredType.Properties();
            str.Append(complexType.Name);
            return str.ToString();
        }

        private string ProcessEntity(IEdmEntityType entity)
        {
            StringBuilder str = new StringBuilder();
            str.Append($@"public class {entity.Name}");
            str.Append(Environment.NewLine);
            str.Append("{");
            str.Append(Environment.NewLine);

            foreach (var prop in entity.DeclaredProperties)
            {
                string t = "";
                IEdmTypeReference edmType = prop.Type;
                if (edmType.IsPrimitive())
                {
                    t = ClrDictionary[edmType.PrimitiveKind()];
                    if (edmType.IsNullable)
                        t += "?";
                }
                else if (edmType.IsEnum()
                        || edmType.IsComplex()
                        || edmType.IsEntity()
                        || edmType.IsEntityReference()
                        || !edmType.IsCollection()
                       )
                {
                    t = edmType.FullName();
                }
                else if (edmType.IsCollection())
                {
                    IEdmCollectionTypeReference col = edmType.AsCollection();
                    var elementTypeReference = col.ElementType();
                    if (elementTypeReference.IsPrimitive())
                    {
                        var primitiveClr = ClrDictionary[elementTypeReference.PrimitiveKind()];
                        t = $"List<{primitiveClr}>";
                    }
                    else
                    {
                        t = $"List<{elementTypeReference.Definition.FullTypeName()}>";
                    }
                }

                if (t == "")
                    t = prop.Type.FullName();

                if (prop.IsKey())
                    str.Append($"[Key]{Environment.NewLine}");
                str.Append($"public {t} {prop.Name} {{ get; set; }} ");
                if (prop.PropertyKind == EdmPropertyKind.Navigation)
                    str.Append("//Navigation Property");
                str.Append(Environment.NewLine);
            }

            str.Append($@"
                }}");
            str.Append(Environment.NewLine);
            return str.ToString();
        }


        internal readonly Dictionary<EdmPrimitiveTypeKind, string> ClrDictionary =
           new Dictionary<EdmPrimitiveTypeKind, string>
           {
            {EdmPrimitiveTypeKind.Int32, "int"},
            {EdmPrimitiveTypeKind.String, "string"},
            {EdmPrimitiveTypeKind.Binary, "byte[]"},
            {EdmPrimitiveTypeKind.Decimal, "decimal"},
            {EdmPrimitiveTypeKind.Int16, "short"},
            {EdmPrimitiveTypeKind.Single, "float"},
            {EdmPrimitiveTypeKind.Boolean, "bool"},
            {EdmPrimitiveTypeKind.Double, "double"},
            {EdmPrimitiveTypeKind.Guid, "Guid"},
            {EdmPrimitiveTypeKind.Byte, "byte"},
            {EdmPrimitiveTypeKind.Int64, "long"},
            {EdmPrimitiveTypeKind.SByte, "sbyte"},
            {EdmPrimitiveTypeKind.Stream, "Stream"},
            {EdmPrimitiveTypeKind.Geography, "Geography"},
            {EdmPrimitiveTypeKind.GeographyPoint, "GeographyPoint"},
            {EdmPrimitiveTypeKind.GeographyLineString, "GeographyLineString"},
            {EdmPrimitiveTypeKind.GeographyPolygon, "GeographyMultiPolygon"},
            {EdmPrimitiveTypeKind.GeographyCollection, "GeographyCollection"},
            {EdmPrimitiveTypeKind.GeographyMultiPolygon, "GeographyMultiPolygon"},
            {EdmPrimitiveTypeKind.GeographyMultiLineString, "GeographyMultiLineString"},
            {EdmPrimitiveTypeKind.GeographyMultiPoint, "GeographyMultiPoint"},
            {EdmPrimitiveTypeKind.Geometry, "Geometry"},
            {EdmPrimitiveTypeKind.GeometryPoint, "GeometryPoint"},
            {EdmPrimitiveTypeKind.GeometryLineString, "GeometryLineString"},
            {EdmPrimitiveTypeKind.GeometryPolygon, "GeometryPolygon"},
            {EdmPrimitiveTypeKind.GeometryCollection, "GeometryCollection"},
            {EdmPrimitiveTypeKind.GeometryMultiPolygon, "GeometryMultiPolygon"},
            {EdmPrimitiveTypeKind.GeometryMultiLineString, "GeometryMultiLineString"},
            {EdmPrimitiveTypeKind.GeometryMultiPoint, "GeometryMultiPoint"},
            {EdmPrimitiveTypeKind.DateTimeOffset, "DateTimeOffset"},
            {EdmPrimitiveTypeKind.Duration, "TimeSpan"},
            {EdmPrimitiveTypeKind.Date, "Microsoft.OData.Edm.Library.Date"}, //DateTime not supported
            {EdmPrimitiveTypeKind.TimeOfDay, "Microsoft.OData.Edm.Library.TimeOfDay"}
       };
    }
}