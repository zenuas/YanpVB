Imports System


<Serializable()>
Public Class Token

    Public Overridable ReadOnly Property SymbolType As TokenTypes
    Public Overridable ReadOnly Property Name As String

    Public Sub New(symbol_type As TokenTypes, Optional name As String = "")

        Me.SymbolType = symbol_type

        If Me.SymbolType <> TokenTypes.EOF AndAlso name = "" Then Throw New ArgumentException("symbol without blank", "name")
        If Me.SymbolType = TokenTypes.EOF AndAlso name <> "" Then Throw New ArgumentException("eof with blank", "name")

        If Me.SymbolType = TokenTypes.CharValue Then

            Me.Name = String.Format("'{0}'", name)
        Else
            Me.Name = name
        End If
    End Sub

End Class
