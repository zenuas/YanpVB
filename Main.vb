Imports System
Imports System.IO


Public Class Main

    Public Shared Sub Main(args() As String)

        Dim y As Syntax
        If args.Length = 0 Then

            y = Parse(Console.In)
        Else

            Using in_ As New StreamReader(args(0))

                y = Parse(in_)
            End Using
        End If

        Dim nodes = Generator.LR0(y)
        Dim resolve = Generator.LALR1(y, nodes)

#If DEBUG Then
        nodes.Do(
            Sub(x, i)
                Console.WriteLine($"state {i}")
                Console.WriteLine("    " + x.ToString.Replace(Environment.NewLine, Environment.NewLine + "    "))
                Console.WriteLine()
            End Sub)
#End If

        Dim err As System.CodeDom.Compiler.CompilerErrorCollection = Nothing
        Dim engine As New Microsoft.VisualStudio.TextTemplating.Engine
        Dim host As New TextTemplatingEngineHostBinder With {
                .LogErrors = Sub(e) err = e,
                .AppDomain = AppDomain.CurrentDomain,
                .ResolveAssemblyReference =
                    Function(s)

                        If File.Exists(s) Then Return s

                        Dim asm = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location)
                        If File.Exists(Path.Combine(asm, s)) Then Return Path.Combine(asm, s)

                        Return Nothing
                    End Function
            }
        host.Session("Syntax") = y
        host.Session("Nodes") = nodes
        For Each f In Directory.GetFiles("template.vb")

            host.TemplateFile = f
            Dim out = engine.ProcessTemplate(File.ReadAllText(f), host)
            If err?.HasErrors Then

                For Each e In err

                    Console.Error.WriteLine(e)
                Next
                Exit For
            Else
                Console.WriteLine(out)
            End If
        Next

#If DEBUG Then
        Console.WriteLine("push any key...")
        Console.ReadKey()
#End If
    End Sub

    Public Shared Function Parse(in_ As TextReader) As Syntax

        Dim y As New Syntax
        Dim lex As New Lexer With {.Reader = in_}
        Parser.ParseDeclaration(y, lex)
        Parser.ParseGrammar(y, lex)
        y.FooterCode.Append(lex.Reader.ReadToEnd)
        Return y
    End Function

End Class
