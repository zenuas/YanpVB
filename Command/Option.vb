Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Reflection


Namespace Command

    Public Class [Option]

        <CommandLine("i", "input")>
        Public Overridable Property Input As TextReader = Nothing

        <CommandLine("v"c, "verbose")>
        Public Overridable Property VerboseOutput As TextWriter = Nothing

        <CommandLine("c"c, "csv-output")>
        Public Overridable Property CsvOutput As TextWriter = Nothing

        <CommandLine("g"c, "graphviz-output")>
        Public Overridable Property GraphvizOutput As TextWriter = Nothing

        <CommandLine("b"c, "basepath")>
        Public Overridable Property BasePath As String = Path.GetDirectoryName(Me.GetType.Assembly.Location)

        <CommandLine("t"c, "template")>
        Public Overridable Property Template As String = "vb"

        <CommandLine("p"c, "prefix")>
        Public Overridable Property Prefix As String = ""

        <CommandLine("h", "help")>
        Public Overridable Sub Help()

            Dim opt_map = Parser.GetCommand(Me)
            For Each key In opt_map.Keys

                Dim method = opt_map(key)
                If method.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase) Then

                    Console.WriteLine($"  --{key}")
                Else
                    Console.WriteLine($"  -{key}, --{method.Name}")
                End If
            Next

            System.Environment.Exit(0)
        End Sub

        <CommandLine("V", "version")>
        Public Overridable Sub Version()

            Console.WriteLine($"yanp {Assembly.GetExecutingAssembly.GetName.Version}")

            System.Environment.Exit(0)
        End Sub

    End Class

End Namespace
