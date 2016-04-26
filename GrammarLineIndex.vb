Imports System
Imports System.Text

Public Class GrammarLineIndex
    Implements IEquatable(Of GrammarLineIndex)

    Public Overridable Property Line As GrammarLine
    Public Overridable Property Index As Integer

    Public Overridable Overloads Function Equals(other As GrammarLineIndex) As Boolean Implements IEquatable(Of GrammarLineIndex).Equals

        Return Me.Line Is other.Line AndAlso Me.Index = other.Index
    End Function

    Public Overrides Function ToString() As String

        Dim s As New StringBuilder($"{Me.Line.Name} :")
        For i = 0 To Me.Line.Grams.Count

            If i = Me.Index Then s.Append(" .")
            If i < Me.Line.Grams.Count Then s.Append($" {Me.Line.Grams(i).Name}")
        Next
        Return s.ToString
    End Function

End Class
