Imports System
Imports System.IO


Public Class Main

    Public Shared Sub Main(args() As String)

        Dim opt As New Command.Option
        args = Command.Parser.Parse(opt, args)
        If args.Length > 0 AndAlso opt.Input Is Nothing Then opt.Input = New StreamReader(args(0))

        Dim y As New Syntax
        Dim lex As New Lexer With {.Reader = opt.Input}
        Parser.ParseDeclaration(y, lex)
        Parser.ParseGrammar(y, lex)
        y.FooterCode.Append(lex.Reader.ReadToEnd)
        Dim nodes = Generator.LR0(y)
        Dim resolve = Generator.LALR1(y, nodes)
        Dim table = Generator.LALRParser(y, nodes, resolve.Item1, resolve.Item2)

        Dim dir_templates = Path.Combine(opt.BasePath, $"template.{opt.Template}")
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
                    End Function,
               .LoadIncludeText =
                    Function(s)

                        Return File.ReadAllText(Path.Combine(dir_templates, s))
                    End Function
            }
        host.Session("Syntax") = y
        host.Session("Nodes") = nodes
        host.Session("ParserTable") = table.Item1
        host.Session("ParserError") = table.Item2

        Dim create_template =
            Function(template As String, output As TextWriter)

                host.TemplateFile = template
                Dim out = engine.ProcessTemplate(File.ReadAllText(template), host)
                If err?.HasErrors Then

                    For Each e In err

                        Console.Error.WriteLine(e)
                    Next
                    System.Diagnostics.Debug.Fail("template compile error")
                    Return False

                Else

                    If output Is Nothing Then

                        Using x As New StreamWriter($"{opt.Prefix}{Path.GetFileNameWithoutExtension(template)}{host.FileExtension}")

                            x.Write(out)
                        End Using
                    Else
                        output.Write(out)
                    End If
                    Return True
                End If
            End Function

        For Each f In Directory.GetFiles(dir_templates, "*.tt")

            If Not create_template(f, Nothing) Then Exit For
        Next

        If opt.VerboseOutput IsNot Nothing Then create_template(Path.Combine(opt.BasePath, "verbose.tt"), opt.VerboseOutput)
        If opt.CsvOutput IsNot Nothing Then create_template(Path.Combine(opt.BasePath, "csv.tt"), opt.CsvOutput)
        If opt.GraphvizOutput IsNot Nothing Then create_template(Path.Combine(opt.BasePath, "graph.tt"), opt.GraphvizOutput)

    End Sub

End Class
