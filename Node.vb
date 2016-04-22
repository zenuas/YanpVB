Imports System.Collections.Generic


Public Class Node

    Public Overridable Property Name As String
    Public Overridable ReadOnly Property Lines As New List(Of GrammarLineIndex)
    Public Overridable ReadOnly Property NextNodes As New List(Of Node)

    Public Overrides Function ToString() As String

        Return String.Join(System.Environment.NewLine, Me.Lines.Map(Function(x) x.ToString))
    End Function
End Class
