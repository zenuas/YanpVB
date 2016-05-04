<#@ import namespace="System" #>
<#@ import namespace="System.Collections.Generic" #>
<#@ import namespace="System.Text.RegularExpressions" #>
<#@ import namespace="Yanp.ArrayExtension" #>
<#
Dim vb_reserved_word = {
		"AddHandler",
		"AddressOf",
		"Alias",
		"And",
		"AndAlso",
		"As",
		"Boolean",
		"ByRef",
		"Byte",
		"ByVal",
		"Call",
		"Case",
		"Catch",
		"CBool",
		"CByte",
		"CChar",
		"CDate",
		"CDec",
		"CDbl",
		"Char",
		"CInt",
		"Class",
		"CLng",
		"CObj",
		"Const",
		"Continue",
		"CSByte",
		"CShort",
		"CSng",
		"CStr",
		"CType",
		"CUInt",
		"CULng",
		"CUShort",
		"Date",
		"Decimal",
		"Declare",
		"Default",
		"Delegate",
		"Dim",
		"DirectCast",
		"Do",
		"Double",
		"Each",
		"Else",
		"ElseIf",
		"End",
		"EndIf",
		"Enum",
		"Erase",
		"Error",
		"Event",
		"Exit",
		"False",
		"Finally",
		"For",
		"Friend",
		"Function",
		"Get",
		"GetType",
		"Global",
		"GoSub",
		"GoTo",
		"Handles",
		"If",
		"Implements",
		"Imports",
		"In",
		"Inherits",
		"Integer",
		"Interface",
		"Is",
		"IsNot",
		"Let",
		"Lib",
		"Like",
		"Long",
		"Loop",
		"Me",
		"Mod",
		"Module",
		"MustInherit",
		"MustOverride",
		"MyBase",
		"MyClass",
		"Namespace",
		"Narrowing",
		"New",
		"Next",
		"Not",
		"Nothing",
		"NotInheritable",
		"NotOverridable",
		"Object",
		"Of",
		"On",
		"Operator",
		"Option",
		"Optional",
		"Or",
		"OrElse",
		"Overloads",
		"Overridable",
		"Overrides",
		"ParamArray",
		"Partial",
		"Private",
		"Property",
		"Protected",
		"Public",
		"RaiseEvent",
		"ReadOnly",
		"ReDim",
		"REM",
		"RemoveHandler",
		"Resume",
		"Return",
		"SByte",
		"Select",
		"Set",
		"Shadows",
		"Shared",
		"Short",
		"Single",
		"Static",
		"Step",
		"Stop",
		"String",
		"Structure",
		"Sub",
		"SyncLock",
		"Then",
		"Throw",
		"To",
		"True",
		"Try",
		"TryCast",
		"TypeOf",
		"Variant",
		"Wend",
		"UInteger",
		"ULong",
		"UShort",
		"Using",
		"When",
		"While",
		"Widening",
		"With",
		"WithEvents",
		"WriteOnly",
		"Xor"
	}.Map(Function (x) x.ToUpper()).ToHash_ValueDerivation(Function (x) True)

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
Dim vbterms = sorted_terms.ToHash_ValueDerivation(
	Function(x)
		
		x = Regex.Replace(x, "^'|'$", "")
		x = Regex.Replace(x, "\$(?=[a-zA-Z])", "_")
		x = Regex.Replace(x, "[^_0-9a-zA-Z]", Function (c) "__x" + Convert.ToInt32(c.Value.Chars(0)).ToString("X2"))
		
		Dim xupper = x.ToUpper()
		
		If used_term.ContainsKey(xupper) Then
			
			used_term(xupper) += 1
			Return String.Format("{0}_{1}", x, used_term(xupper))
		End If
		
		used_term(xupper) = 0
		Return If(vb_reserved_word.ContainsKey(xupper), String.Format("[{0}]", x), x)
	End Function)
#>
