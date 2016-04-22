Imports System


<Serializable()>
Public Class Declarate

    Public Overridable Property Name As String
    Public Overridable Property Assoc As AssocTypes
    Public Overridable Property Priority As Integer
    Public Overridable Property Type As String

    Public Overrides Function ToString() As String

        Return Me.Name
    End Function

End Class
