<#@ import namespace="System" #>
<#@ import namespace="System.Collections.Generic" #>
<#@ import namespace="System.Text.RegularExpressions" #>
<#@ import namespace="Yanp.Extensions" #>
<#
Dim cs_reserved_word = {
		"abstract",
		"as",
		"base",
		"bool",
		"break",
		"byte",
		"case",
		"catch",
		"char",
		"checked",
		"class",
		"const",
		"continue",
		"decimal",
		"default",
		"delegate",
		"do",
		"double",
		"else",
		"enum",
		"event",
		"explicit",
		"extern",
		"false",
		"finally",
		"fixed",
		"float",
		"for",
		"foreach",
		"goto",
		"if",
		"implicit",
		"in",
		"int",
		"interface",
		"internal",
		"is",
		"lock",
		"long",
		"namespace",
		"new",
		"null",
		"object",
		"operator",
		"out",
		"override",
		"params",
		"private",
		"protected",
		"public",
		"readonly",
		"ref",
		"return",
		"sbyte",
		"sealed",
		"short",
		"sizeof",
		"stackalloc",
		"static",
		"string",
		"struct",
		"switch",
		"this",
		"throw",
		"true",
		"try",
		"typeof",
		"uint",
		"ulong",
		"unchecked",
		"unsafe",
		"ushort",
		"using",
		"virtual",
		"void",
		"volatile",
		"while"
	}.ToHash_ValueDerivation(Function (x) True)

Dim used_term As New Dictionary(Of String, Integer)
Dim sorted_terms As List(Of String) = Syntax.Declas.Keys.SortToList(
	Function(a, b)
		
		Dim ax = Syntax.Declas(a)
		Dim bx = Syntax.Declas(b)
		
		If ax.IsTerminalSymbol AndAlso Not bx.IsTerminalSymbol Then Return -1
		If Not ax.IsTerminalSymbol AndAlso bx.IsTerminalSymbol Then Return 1

		If a.StartsWith("$") AndAlso Not b.StartsWith("$") Then Return 1
		If Not a.StartsWith("$") AndAlso b.StartsWith("$") Then Return -1

		Return String.Compare(a, b)
	End Function)
Dim terms = sorted_terms.ToHash_ValueDerivation(Function(x, i) i)
Dim csterms = sorted_terms.ToHash_ValueDerivation(
	Function(x)
		
		x = Regex.Replace(x, "^'|'$", "")
		x = Regex.Replace(x, "\$(?=[a-zA-Z])", "_")
		x = Regex.Replace(x, "[^_0-9a-zA-Z]", Function (c) "__x" + Convert.ToInt32(c.Value.Chars(0)).ToString("X2"))
		
		If used_term.ContainsKey(x) Then
			
			used_term(x) += 1
			Return String.Format("{0}_{1}", x, used_term(x))
		End If
		
		used_term(x) = 0
		Return If(cs_reserved_word.ContainsKey(x), String.Format("@{0}", x), x)
	End Function)
#>
