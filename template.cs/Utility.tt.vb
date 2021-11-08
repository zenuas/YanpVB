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

Dim ascii As New Dictionary(Of Integer, String) From {
		{0, "__NUL"},
		{1, "__SOH"},
		{2, "__STX"},
		{3, "__ETX"},
		{4, "__EOT"},
		{5, "__ENQ"},
		{6, "__ACK"},
		{7, "__BEL"},
		{8, "__BS"},
		{9, "__HT"},
		{10, "__LF"},
		{11, "__VT"},
		{12, "__FF"},
		{13, "__CR"},
		{14, "__SO"},
		{15, "__SI"},
		{16, "__DLE"},
		{17, "__DC1"},
		{18, "__DC2"},
		{19, "__DC3"},
		{20, "__DC4"},
		{21, "__NAK"},
		{22, "__SYN"},
		{23, "__ETB"},
		{24, "__CAN"},
		{25, "__EM"},
		{26, "__SUB"},
		{27, "__ESC"},
		{28, "__FS"},
		{29, "__GS"},
		{30, "__RS"},
		{31, "__US"},
		{32, "__Space"},
		{33, "__ExclamationMark"},
		{34, "__QuotationMark"},
		{35, "__NumberSign"},
		{36, "__DollarSign"},
		{37, "__PercentSign"},
		{38, "__Ampersand"},
		{39, "__Apostrophe"},
		{40, "__LeftParenthesis"},
		{41, "__RightParenthesis"},
		{42, "__Asterisk"},
		{43, "__PlusSign"},
		{44, "__Comma"},
		{45, "__HyphenMinus"},
		{46, "__FullStop"},
		{47, "__Slash"},
		{58, "__Colon"},
		{59, "__Semicolon"},
		{60, "__LessThanSign"},
		{61, "__EqualsSign"},
		{62, "__GreaterThanSign"},
		{63, "__QuestionMark"},
		{64, "__AtSign"},
		{91, "__LeftSquareBracket"},
		{92, "__Backslash"},
		{93, "__RightSquareBracket"},
		{94, "__Caret"},
		{95, "__Underscore"},
		{96, "__GraveAccent"},
		{123, "__LeftCurlyBracket"},
		{124, "__VerticalBar"},
		{125, "__RightCurlyBracket"},
		{126, "__Tilde"},
		{127, "__DEL"}
	}

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
		x = Regex.Replace(x, "[^_0-9a-zA-Z]", Function (c) If(ascii.ContainsKey(Convert.ToInt32(c.Value.Chars(0))), ascii(Convert.ToInt32(c.Value.Chars(0))), "__x" + Convert.ToInt32(c.Value.Chars(0)).ToString("X2")))
		
		If used_term.ContainsKey(x) Then
			
			used_term(x) += 1
			Return String.Format("{0}_{1}", x, used_term(x))
		End If
		
		used_term(x) = 0
		Return If(cs_reserved_word.ContainsKey(x), String.Format("@{0}", x), x)
	End Function)
#>
