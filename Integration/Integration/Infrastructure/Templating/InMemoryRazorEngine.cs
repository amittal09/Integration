using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Razor;
using Vertica.Integration.Infrastructure.Templating.AttributeParsing;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Templating
{
	public sealed class InMemoryRazorEngine
	{
		private readonly static Regex ModelTypeFinder;

		static InMemoryRazorEngine()
		{
			InMemoryRazorEngine.ModelTypeFinder = new Regex("(?:@model\\s\\w.*?\\n)");
		}

		public InMemoryRazorEngine()
		{
		}

		public static string Execute(string razorTemplate)
		{
			return InMemoryRazorEngine.Execute<object>(razorTemplate, null, null, new Assembly[0]);
		}

		public static string Execute<TModel>(string razorTemplate, TModel model, dynamic viewBag = null, params Assembly[] referenceAssemblies)
		{
			string str;
			razorTemplate = InMemoryRazorEngine.ModelTypeFinder.Replace(razorTemplate, string.Empty, 1);
			RazorEngineHost razorEngineHost = new RazorEngineHost(new CSharpRazorCodeLanguage());
			razorEngineHost.DefaultNamespace = "RazorOutput";
			razorEngineHost.DefaultClassName ="Template";
			razorEngineHost.NamespaceImports.Add("System");
			razorEngineHost.DefaultBaseClass = typeof(InMemoryRazorEngine.RazorTemplateBase<TModel>).FullName;
			RazorTemplateEngine razorTemplateEngine = new RazorTemplateEngine(razorEngineHost);
			using (StringReader stringReader = new StringReader(razorTemplate))
			{
				GeneratorResults generatorResult = razorTemplateEngine.GenerateCode(stringReader);
				CompilerParameters compilerParameter = new CompilerParameters()
				{
					GenerateInMemory = true
				};
				compilerParameter.ReferencedAssemblies.Add(typeof(InMemoryRazorEngine).Assembly.Location);
				compilerParameter.ReferencedAssemblies.Add(typeof(IHtmlString).Assembly.Location);
				compilerParameter.ReferencedAssemblies.AddRange((
					from  x in referenceAssemblies.EmptyIfNull<Assembly>()
					select x.Location).ToArray<string>());
				CompilerResults compilerResult = (new CSharpCodeProvider()).CompileAssemblyFromDom(compilerParameter, new CodeCompileUnit[] { generatorResult.GeneratedCode });
				if (compilerResult.Errors.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (CompilerError error in compilerResult.Errors)
					{
						stringBuilder.Append(string.Format("Line: {0}\t Col: {1}\t Error: {2}\r\n", error.Line, error.Column, error.ErrorText));
					}
					throw new InvalidOperationException(stringBuilder.ToString());
				}
				Type type = compilerResult.CompiledAssembly.GetExportedTypes().Single<Type>();
				object obj = Activator.CreateInstance(type);
				type.GetProperty("Model").SetValue(obj, model, null);
				type.GetProperty("ViewBag").SetValue(obj, viewBag, (dynamic)null);
				type.GetMethod("Execute").Invoke(obj, null);
				str = ((StringBuilder)type.GetProperty("OutputBuilder").GetValue(obj, null)).ToString();
			}
			return str;
		}

		public abstract class RazorTemplateBase<TModel>
		{
			public HtmlHelper Html
			{
				get;
				private set;
			}

			public TModel Model
			{
				get;
				set;
			}

			public StringBuilder OutputBuilder
			{
				get;
			}

			public dynamic ViewBag
			{
				get;
				set;
			}

			protected RazorTemplateBase()
			{
				this.OutputBuilder = new StringBuilder();
				this.Html = new HtmlHelper();
			}

			public abstract void Execute();

			public virtual void Write(object value)
			{
				this.OutputBuilder.Append(value);
			}

			public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
			{
				bool flag = true;
				bool flag1 = false;
				if (values.Length == 0)
				{
					this.WritePositionTaggedLiteral(prefix);
					this.WritePositionTaggedLiteral(suffix);
					return;
				}
				AttributeValue[] attributeValueArray = values;
				for (int i = 0; i < (int)attributeValueArray.Length; i++)
				{
					AttributeValue attributeValue = attributeValueArray[i];
					PositionTagged<object> value = attributeValue.Value;
					bool? nullable = null;
					if (value.Value is bool)
					{
						nullable = new bool?((bool)value.Value);
					}
					if (value.Value != null && (!nullable.HasValue || nullable.Value))
					{
						string str = value.Value as string ?? value.Value.ToString();
						if (nullable.HasValue)
						{
							str = name;
						}
						if (!flag)
						{
							this.WritePositionTaggedLiteral(attributeValue.Prefix);
						}
						else
						{
							this.WritePositionTaggedLiteral(prefix);
							flag = false;
						}
						if (!attributeValue.Literal)
						{
							this.Write(str);
						}
						else
						{
							this.WriteLiteral(str);
						}
						flag1 = true;
					}
				}
				if (flag1)
				{
					this.WritePositionTaggedLiteral(suffix);
				}
			}

			public virtual void WriteLiteral(object value)
			{
				this.OutputBuilder.Append(value);
			}

			private void WritePositionTaggedLiteral(string value)
			{
				if (value == null)
				{
					return;
				}
				this.WriteLiteral(value);
			}

			private void WritePositionTaggedLiteral(PositionTagged<string> value)
			{
				this.WritePositionTaggedLiteral(value.Value);
			}
		}
	}
}