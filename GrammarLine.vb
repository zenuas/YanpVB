Imports System.Collections.Generic


Public Class GrammarLine

    Public Overridable Property Name As String
    Public Overridable Property Assoc As AssocTypes = AssocTypes.Type
    Public Overridable Property Priority As Integer
    Public Overridable Property Action As String = Nothing

    Public Overridable ReadOnly Property HasAction As Boolean
        Get
            Return Me.Action IsNot Nothing
        End Get
    End Property

    Public Overridable ReadOnly Property Grams As New List(Of Declarate)

    Public Overrides Function ToString() As String

        Return $"{Me.Name} :{String.Join("", Me.Grams.Map(Function(x) $" {x.Name}"))}"
    End Function
End Class
