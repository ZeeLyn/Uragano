using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyModel;
using Uragano.Abstractions;

namespace Uragano.DynamicProxy
{
	public class ProxyGenerator : IProxyGenerator
	{
		public object Create<TProxy>(Type type) where TProxy : DispatchProxy
		{
			var callExpression = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { type, typeof(TProxy) });
			return Expression.Lambda<Func<object>>(callExpression).Compile()();
		}

		public object GenerateProxy(IEnumerable<Type> interfaces)
		{
			var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(i => i.IsDynamic == false);

			var types = assemblies.Select(p => p.GetType());
			types = types.Except(interfaces);
			foreach (var type in types)
			{
				assemblies = assemblies.Append(type.Assembly);
			}

			var trees = interfaces.Select(GenerateProxyTree).ToList();
			var text = trees.First().GetText();
			File.WriteAllText(@"F:\Github\Uragano\src\Uragano.DynamicProxy\Imp.cs", text.ToString());
			using (var stream = CompilationUtilitys.CompileClientProxy(trees,
				assemblies.Select(x => MetadataReference.CreateFromFile(x.Location))
					.Concat(new[]
					{
						MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location)
					})))
			{
				var assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
				byte[] bytes = new byte[stream.Length];

				stream.Read(bytes, 0, bytes.Length);

				// 设置当前流的位置为流的开始 

				stream.Seek(0, SeekOrigin.Begin);
				using (var fs = new FileStream("E://proxy.dll", FileMode.Create))
				{
					fs.Write(bytes, 0, bytes.Length);
					fs.Flush();
				}
				return assembly.GetExportedTypes();
			}
		}


		private SyntaxTree GenerateProxyTree(Type @interface)
		{
			var syntax = SyntaxFactory.CompilationUnit()
				.WithUsings(SyntaxFactory.List(new[]
				{
					SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")),
					SyntaxFactory.UsingDirective(GenerateQualifiedNameSyntax("System.Threading.Tasks")),
					SyntaxFactory.UsingDirective(GenerateQualifiedNameSyntax("System.Collections.Generic")),
					SyntaxFactory.UsingDirective(GenerateQualifiedNameSyntax(typeof(DynamicProxyAbstract).Namespace)),
					SyntaxFactory.UsingDirective(GenerateQualifiedNameSyntax(@interface.Namespace))
				}))
				.WithMembers(GenerateNamespace(@interface));

			return syntax.NormalizeWhitespace().SyntaxTree;
		}

		/// <summary>
		/// Generate namespace
		/// </summary>
		/// <returns></returns>
		private SyntaxList<MemberDeclarationSyntax> GenerateNamespace(Type type)
		{
			var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(GenerateQualifiedNameSyntax("Uragano.DynamicProxy.Imp"));
			return GenerateClass(namespaceDeclaration, type);
		}

		private SyntaxList<MemberDeclarationSyntax> GenerateClass(NamespaceDeclarationSyntax namespaceDeclaration, Type type)
		{
			var className = type.Name.TrimStart('I') + "_Proxy";

			// base class
			var baseList = SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(new SyntaxNodeOrToken[]{
				SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("DynamicProxyAbstract")),//Inherit DynamicProxyAbstract class
				SyntaxFactory.Token(SyntaxKind.CommaToken),
				SyntaxFactory.SimpleBaseType(GenerateQualifiedNameSyntax(type))//Inherit proxy interface
			}));
			var classDeclaration = SyntaxFactory.ClassDeclaration(className)
				.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
				.WithBaseList(baseList)
				.WithMembers(GenerateMethods(type, className));
			return SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceDeclaration.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDeclaration)));
		}

		private SyntaxList<MemberDeclarationSyntax> GenerateMethods(Type type, string className)
		{
			var typeAttr = type.GetCustomAttribute<ServiceRouteAttribute>();
			var routePrefix = typeAttr == null ? $"{type.Namespace}/{type.Name}" : typeAttr.Route;
			var methods = type.GetMethods().ToList();

			var s = methods.Select(p => GenerateMethod(routePrefix, p)).ToList();
			s.Add(GenerateConstructorDeclaration(className));
			return SyntaxFactory.List(s);
		}

		private MemberDeclarationSyntax GenerateMethod(string routePrefix, MethodInfo methodInfo)
		{
			var methodAttr = methodInfo.GetCustomAttribute<ServiceRouteAttribute>();
			var serviceRoute = $"{routePrefix}/{(methodAttr == null ? methodInfo.Name : methodAttr.Route)}";
			var returnDeclaration = GenerateType(methodInfo.ReturnType);
			var args = new List<SyntaxNodeOrToken>();
			var argDeclarations = new List<SyntaxNodeOrToken>();
			foreach (var arg in methodInfo.GetParameters())
			{
				if (arg.ParameterType.IsGenericType)
					argDeclarations.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(arg.Name)).WithType(GenerateType(arg.ParameterType)));
				else
					argDeclarations.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(arg.Name)).WithType(GenerateQualifiedNameSyntax(arg.ParameterType)));

				argDeclarations.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));

				//args.Add(SyntaxFactory.IdentifierName(arg.Name));
				args.Add(SyntaxFactory.InitializerExpression(
					SyntaxKind.ComplexElementInitializerExpression,
					SyntaxFactory.SeparatedList<ExpressionSyntax>(
						new SyntaxNodeOrToken[]{
							SyntaxFactory.LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								SyntaxFactory.Literal(arg.Name)),
							SyntaxFactory.Token(SyntaxKind.CommaToken),
							SyntaxFactory.IdentifierName(arg.Name)})));
				//args.Add(SyntaxFactory.InitializerExpression());
				args.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
			}

			if (args.Any())
			{
				args.RemoveAt(args.Count - 1);
				argDeclarations.RemoveAt(argDeclarations.Count - 1);
			}

			MethodDeclarationSyntax methodDeclaration;

			if (methodInfo.ReturnType == typeof(void))
			{
				methodDeclaration = SyntaxFactory.MethodDeclaration(
						SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
						SyntaxFactory.Identifier(methodInfo.Name));
			}
			else
			{
				methodDeclaration = SyntaxFactory
					.MethodDeclaration(returnDeclaration, SyntaxFactory.Identifier(methodInfo.Name));
			}

			if (methodInfo.ReturnType.BaseType == typeof(Task))
			{
				methodDeclaration = methodDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
					SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));
			}
			else
			{
				methodDeclaration = methodDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
			}

			methodDeclaration = methodDeclaration.WithParameterList(
				SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(argDeclarations)));


			ExpressionSyntax expressionSyntax = null;
			StatementSyntax statementSyntax = null;

			//Sync method
			if (methodInfo.ReturnType.BaseType != typeof(Task))
			{
				if (methodInfo.ReturnType == typeof(void))
				{
					expressionSyntax = SyntaxFactory.IdentifierName("Invoke");

				}
				else
				{
					expressionSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Invoke"))
						.WithTypeArgumentList(((GenericNameSyntax)returnDeclaration).TypeArgumentList);
				}
			}
			else //Async method
			{
				if (methodInfo.ReturnType == typeof(Task))
				{
					expressionSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeAsync"));
				}
				else
				{
					expressionSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeAsync"))
						.WithTypeArgumentList(((GenericNameSyntax)returnDeclaration).TypeArgumentList);
				}
			}


			var invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax).WithArgumentList(SyntaxFactory.ArgumentList(
				SyntaxFactory.SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
				{
					//SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("System.Object[]"))).WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,SyntaxFactory.SeparatedList<ExpressionSyntax>(args)))),
					//SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("System.Object[]")),express)),
					SyntaxFactory.Argument(
						SyntaxFactory.ObjectCreationExpression(
								SyntaxFactory.GenericName(
										SyntaxFactory.Identifier("Dictionary"))
									.WithTypeArgumentList(
										SyntaxFactory.TypeArgumentList(
											SyntaxFactory.SeparatedList<TypeSyntax>(
												new SyntaxNodeOrToken[]
												{
													SyntaxFactory.PredefinedType(
														SyntaxFactory.Token(SyntaxKind.StringKeyword)),
													SyntaxFactory.Token(SyntaxKind.CommaToken),
													SyntaxFactory.PredefinedType(
														SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
												}))))
							.WithInitializer(
								SyntaxFactory.InitializerExpression(
									SyntaxKind.CollectionInitializerExpression,
									SyntaxFactory.SeparatedList<ExpressionSyntax>(
										args)))),
					SyntaxFactory.Token(SyntaxKind.CommaToken),
					SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,SyntaxFactory.Literal(serviceRoute)))

				})));
			if (methodInfo.ReturnType.BaseType != typeof(Task))
			{
				expressionSyntax = SyntaxFactory.InvocationExpression(invocationExpression);
			}
			else
			{
				expressionSyntax = SyntaxFactory.AwaitExpression(invocationExpression);
			}


			if (methodInfo.ReturnType != typeof(Task) && methodInfo.ReturnType != typeof(void))
				statementSyntax = SyntaxFactory.ReturnStatement(expressionSyntax);
			else
				statementSyntax = SyntaxFactory.ExpressionStatement(expressionSyntax);

			return methodDeclaration.WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList(statementSyntax)));
		}

		private ConstructorDeclarationSyntax GenerateConstructorDeclaration(string className)
		{
			return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(className))
				.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
				//.WithParameterList(
				//	SyntaxFactory.ParameterList(
				//		SyntaxFactory.SeparatedList<ParameterSyntax>(
				//			new SyntaxNodeOrToken[]
				//			{
				//				SyntaxFactory.Parameter(SyntaxFactory.Identifier("remoteServiceCaller")).WithType(SyntaxFactory.IdentifierName("IRemoteServiceCaller"))
				//			}
				//		)
				//	)
				//)
				//.WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
				//	SyntaxFactory.ArgumentList(
				//		SyntaxFactory.SeparatedList<ArgumentSyntax>(
				//			new SyntaxNodeOrToken[]
				//			{
				//				SyntaxFactory.Argument(SyntaxFactory.IdentifierName("remoteServiceCaller"))
				//			}
				//		)
				//	)
				//))
				.WithBody(SyntaxFactory.Block());
		}
		private TypeSyntax GenerateType(Type type)
		{
			if (!type.IsGenericType)
				return GenerateQualifiedNameSyntax(type);
			var list = new List<SyntaxNodeOrToken>();
			foreach (var genericType in type.GetGenericArguments())
			{
				list.Add(genericType.IsGenericType ? GenerateType(genericType) : GenerateQualifiedNameSyntax(genericType.FullName));
				list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
			}

			var typeArgumentList = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(list.Take(list.Count - 1)));
			return SyntaxFactory.GenericName(type.Name.Substring(0, type.Name.IndexOf('`'))).WithTypeArgumentList(typeArgumentList);
		}



		#region QualifiedNameSyntax

		private QualifiedNameSyntax GenerateQualifiedNameSyntax(Type type)
		{
			return GenerateQualifiedNameSyntax($"{type.Namespace}.{type.Name}");
		}

		private QualifiedNameSyntax GenerateQualifiedNameSyntax(string fullName)
		{
			return GenerateQualifiedNameSyntax(fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
		}

		private QualifiedNameSyntax GenerateQualifiedNameSyntax(IEnumerable<string> names)
		{
			var identifierNames = names.Select(SyntaxFactory.IdentifierName).ToArray();

			QualifiedNameSyntax left = null;
			for (var i = 0; i < identifierNames.Length - 1; i++)
			{
				left = left == null
					? SyntaxFactory.QualifiedName(identifierNames[i], identifierNames[i + 1])
					: SyntaxFactory.QualifiedName(left, identifierNames[i + 1]);
			}
			return left;
		}
		#endregion
	}
}
