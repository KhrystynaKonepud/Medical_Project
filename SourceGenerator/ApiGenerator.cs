using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medical_center.SourceGenerator
{
    [Generator]
    public class ApiGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will collect classes with GenerateApi attribute
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            // Add the attribute to the compilation first
            context.AddSource("GenerateApiAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8));

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(classDeclaration);

                if (classSymbol == null)
                    continue;

                // Check if class has [GenerateApi] attribute
                var hasAttribute = classSymbol.GetAttributes().Any(ad =>
                    ad.AttributeClass?.Name == "GenerateApiAttribute" ||
                    ad.AttributeClass?.Name == "GenerateApi");

                if (!hasAttribute)
                    continue;

                // Generate DTO
                var dtoSource = GenerateDto(classSymbol);
                context.AddSource($"{classSymbol.Name}Dto.g.cs", SourceText.From(dtoSource, Encoding.UTF8));

                // Generate Mapper
                var mapperSource = GenerateMapper(classSymbol);
                context.AddSource($"{classSymbol.Name}Mapper.g.cs", SourceText.From(mapperSource, Encoding.UTF8));

                // Generate Controller
                var controllerSource = GenerateController(classSymbol);
                context.AddSource($"{classSymbol.Name}Controller.g.cs", SourceText.From(controllerSource, Encoding.UTF8));
            }
        }

        private string GenerateDto(INamedTypeSymbol classSymbol)
        {
            var className = classSymbol.Name;
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var properties = classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !IsNavigationProperty(p))
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}Dto");
            sb.AppendLine("    {");

            foreach (var prop in properties)
            {
                var typeName = prop.Type.ToDisplayString();
                // Make reference types nullable for safety
                var nullableSuffix = prop.Type.IsReferenceType && !typeName.EndsWith("?") ? "?" : "";
                sb.AppendLine($"        public {typeName}{nullableSuffix} {prop.Name} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateMapper(INamedTypeSymbol classSymbol)
        {
            var className = classSymbol.Name;
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var properties = classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !IsNavigationProperty(p))
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine($"using {namespaceName};");
            sb.AppendLine($"using {namespaceName}.Generated;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class {className}Mapper");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static {className}Dto ToDto(this {className} entity)");
            sb.AppendLine("        {");
            sb.AppendLine($"            if (entity == null) return null;");
            sb.AppendLine();
            sb.AppendLine($"            return new {className}Dto");
            sb.AppendLine("            {");

            for (int i = 0; i < properties.Count; i++)
            {
                var prop = properties[i];
                var comma = i < properties.Count - 1 ? "," : "";
                sb.AppendLine($"                {prop.Name} = entity.{prop.Name}{comma}");
            }

            sb.AppendLine("            };");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateController(INamedTypeSymbol classSymbol)
        {
            var className = classSymbol.Name;
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var pluralName = className + "s"; // Simple pluralization
            var varName = char.ToLower(className[0]) + className.Substring(1);

            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine($"using {namespaceName};");
            sb.AppendLine($"using {namespaceName}.Generated;");
            sb.AppendLine("using Medical_center.Data;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Generated.Controllers");
            sb.AppendLine("{");
            sb.AppendLine("    [ApiController]");
            sb.AppendLine($"    [Route(\"api/generated/[controller]\")]");
            sb.AppendLine($"    public class {pluralName}Controller : ControllerBase");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly ApplicationDbContext _context;");
            sb.AppendLine();
            sb.AppendLine($"        public {pluralName}Controller(ApplicationDbContext context)");
            sb.AppendLine("        {");
            sb.AppendLine("            _context = context;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // GET all
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine($"        public async Task<ActionResult<IEnumerable<{className}Dto>>> GetAll()");
            sb.AppendLine("        {");
            sb.AppendLine($"            var items = await _context.{pluralName}");
            sb.AppendLine("                .Select(x => x.ToDto())");
            sb.AppendLine("                .ToListAsync();");
            sb.AppendLine("            return Ok(items);");
            sb.AppendLine("        }");
            sb.AppendLine();

            // GET by id
            sb.AppendLine("        [HttpGet(\"{id}\")]");
            sb.AppendLine($"        public async Task<ActionResult<{className}Dto>> GetById(int id)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var {varName} = await _context.{pluralName}.FindAsync(id);");
            sb.AppendLine($"            if ({varName} == null) return NotFound();");
            sb.AppendLine($"            return Ok({varName}.ToDto());");
            sb.AppendLine("        }");
            sb.AppendLine();

            // POST
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine($"        public async Task<ActionResult<{className}Dto>> Create({className}Dto dto)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var {varName} = new {className}");
            sb.AppendLine("            {");

            var properties = classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                           !IsNavigationProperty(p) &&
                           p.Name != "Id")
                .ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                var prop = properties[i];
                var comma = i < properties.Count - 1 ? "," : "";
                sb.AppendLine($"                {prop.Name} = dto.{prop.Name}{comma}");
            }

            sb.AppendLine("            };");
            sb.AppendLine($"            _context.{pluralName}.Add({varName});");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine($"            return CreatedAtAction(nameof(GetById), new {{ id = {varName}.Id }}, {varName}.ToDto());");
            sb.AppendLine("        }");
            sb.AppendLine();

            // PUT
            sb.AppendLine("        [HttpPut(\"{id}\")]");
            sb.AppendLine($"        public async Task<IActionResult> Update(int id, {className}Dto dto)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (id != dto.Id) return BadRequest();");
            sb.AppendLine($"            var {varName} = await _context.{pluralName}.FindAsync(id);");
            sb.AppendLine($"            if ({varName} == null) return NotFound();");
            sb.AppendLine();

            foreach (var prop in properties)
            {
                sb.AppendLine($"            {varName}.{prop.Name} = dto.{prop.Name};");
            }

            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("            return NoContent();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // DELETE
            sb.AppendLine("        [HttpDelete(\"{id}\")]");
            sb.AppendLine("        public async Task<IActionResult> Delete(int id)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var {varName} = await _context.{pluralName}.FindAsync(id);");
            sb.AppendLine($"            if ({varName} == null) return NotFound();");
            sb.AppendLine($"            _context.{pluralName}.Remove({varName});");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("            return NoContent();");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private bool IsNavigationProperty(IPropertySymbol property)
        {
            var typeName = property.Type.ToDisplayString();

            // Navigation properties are typically:
            // 1. Collection types (ICollection, IList, etc.)
            if (typeName.Contains("ICollection") || typeName.Contains("IList"))
                return true;

            // 2. Reference to other entities (not primitive types)
            if (property.Type.TypeKind == TypeKind.Class &&
                !property.Type.SpecialType.ToString().Contains("String") &&
                property.Type.SpecialType == SpecialType.None)
                return true;

            return false;
        }

        private const string AttributeSource = @"#nullable enable
using System;

namespace Medical_center.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateApiAttribute : Attribute
    {
        public string Version { get; set; } = ""1.0"";
        public bool IncludeNavigations { get; set; } = false;
    }
}";

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
                    classDeclaration.AttributeLists.Count > 0)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}
