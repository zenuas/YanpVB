Imports System.Collections.Generic


Public Class ResolveConflict

    Public Overridable Property LookAHead As Dictionary(Of Node, List(Of String))
    Public Overridable Property Follow As Dictionary(Of String, List(Of String))

End Class
