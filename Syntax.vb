Imports System.Text
Imports System.Collections.Generic


Public Class Syntax

    Public Overridable Property HeaderCode As New StringBuilder
    Public Overridable Property FooterCode As New StringBuilder
    Public Overridable Property Start As String = Nothing
    Public Overridable Property [Default] As String = ""
    Public Overridable Property Defines As New Dictionary(Of String, String)

    ''' <summary>
    ''' %(left|right|nonassoc|type) &lt;type&gt; xxx
    ''' </summary>
    Public Overridable Property Declas As New Dictionary(Of String, Declarate)

    ''' <summary>
    ''' xxx : [xxx ... [| [xxx ...]]]
    ''' </summary>
    Public Overridable Property Grammars As New List(Of GrammarLine)

    Public Overridable Function GetDefine(name As String, Optional default_value As String = "") As String

        If Not Me.Defines.ContainsKey(name) Then Return default_value
        Return Me.Defines(name)
    End Function

End Class
