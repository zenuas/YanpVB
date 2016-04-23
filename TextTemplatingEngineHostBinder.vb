Imports System
Imports System.CodeDom.Compiler
Imports System.Collections
Imports System.Collections.Generic
Imports System.Runtime.Serialization
Imports System.Text
Imports Microsoft.VisualStudio.TextTemplating


Public Class TextTemplatingEngineHostBinder
    Implements ITextTemplatingEngineHost, ITextTemplatingSessionHost


    Public Overridable Property LogErrors As Action(Of CompilerErrorCollection) = Nothing
    Public Overridable ReadOnly Property StandardAssemblyReferences As IList(Of String) = New List(Of String) Implements ITextTemplatingEngineHost.StandardAssemblyReferences
    Public Overridable ReadOnly Property StandardImports As IList(Of String) = New List(Of String) Implements ITextTemplatingEngineHost.StandardImports
    Public Overridable Property TemplateFile As String Implements ITextTemplatingEngineHost.TemplateFile
    Public Overridable Property FileExtension As String = ""
    Public Overridable Property OutputEncoding As Encoding = Encoding.UTF8
    Public Overridable Property GetHostOption As Func(Of String, Object) = Nothing
    Public Overridable Property LoadIncludeText As Func(Of String, String) = Nothing
    Public Overridable Property AppDomain As AppDomain = Nothing
    Public Overridable Property ResolveAssemblyReference As Func(Of String, String) = Nothing
    Public Overridable Property ResolveDirectiveProcessor As Func(Of String, Type) = Nothing
    Public Overridable Property ResolveParameterValue As Func(Of String, String, String, String) = Nothing
    Public Overridable Property ResolvePath As Func(Of String, String) = Nothing
    Public Overridable Property Session As ITextTemplatingSession = New TextTemplatingSession Implements ITextTemplatingSessionHost.Session

    Public Function CreateSession() As ITextTemplatingSession Implements ITextTemplatingSessionHost.CreateSession

        Return Me.Session
    End Function

    Public Overridable Sub LogErrors_(errors As CompilerErrorCollection) Implements ITextTemplatingEngineHost.LogErrors

        Me.LogErrors?(errors)
    End Sub

    Public Overridable Sub SetFileExtension(extension As String) Implements ITextTemplatingEngineHost.SetFileExtension

        Me.FileExtension = extension
    End Sub

    Public Overridable Sub SetOutputEncoding(encoding As Encoding, fromOutputDirective As Boolean) Implements ITextTemplatingEngineHost.SetOutputEncoding

        Me.OutputEncoding = encoding
    End Sub

    Public Overridable Function GetHostOption_(optionName As String) As Object Implements ITextTemplatingEngineHost.GetHostOption

        Return Me.GetHostOption?(optionName)
    End Function

    Public Overridable Function LoadIncludeText_(requestFileName As String, ByRef content As String, ByRef location As String) As Boolean Implements ITextTemplatingEngineHost.LoadIncludeText

        If Me.LoadIncludeText Is Nothing Then Return False
        content = Me.LoadIncludeText(requestFileName)
        location = requestFileName
        Return Not String.IsNullOrEmpty(content)
    End Function

    Public Overridable Function ProvideTemplatingAppDomain(content As String) As AppDomain Implements ITextTemplatingEngineHost.ProvideTemplatingAppDomain

        Return Me.AppDomain
    End Function

    Public Overridable Function ResolveAssemblyReference_(assemblyReference As String) As String Implements ITextTemplatingEngineHost.ResolveAssemblyReference

        Return Me.ResolveAssemblyReference?(assemblyReference)
    End Function

    Public Overridable Function ResolveDirectiveProcessor_(processorName As String) As Type Implements ITextTemplatingEngineHost.ResolveDirectiveProcessor

        Return Me.ResolveDirectiveProcessor?(processorName)
    End Function

    Public Overridable Function ResolveParameterValue_(directiveId As String, processorName As String, parameterName As String) As String Implements ITextTemplatingEngineHost.ResolveParameterValue

        Return Me.ResolveParameterValue?(directiveId, processorName, parameterName)
    End Function

    Public Overridable Function ResolvePath_(path As String) As String Implements ITextTemplatingEngineHost.ResolvePath

        Return Me.ResolvePath?(path)
    End Function

End Class
