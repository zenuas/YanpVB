Imports System


Public Class ParseException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New($"parser read: {message}")

    End Sub

    Public Sub New(lineno As Integer, column As Integer, message As String)
        MyBase.New($"parser read({lineno}, {column}): {message}")

    End Sub

End Class
