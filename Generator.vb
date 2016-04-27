Imports System
Imports System.Collections.Generic


Public Class Generator

    Public Shared Function LR0(y As Syntax) As List(Of Node)

        If y.Grammars.Count = 0 Then Throw New ParseException("no grammar has been specified")
        If y.Grammars.FindFirstOrNull(Function(x) x.Name = y.Start) Is Nothing Then Throw New ParseException($"the start symbol {y.Start} is undefined")

        ' $ACCEPT : y.Start $END
        Dim accept As New Declarate With {.Name = "$ACCEPT"}
        Dim end_ As New Declarate With {.Name = "$END"}
        Dim accept_line As New GrammarLine With {.Name = accept.Name}
        accept_line.Grams.Add(y.Declas(y.Start))
        y.Declas.Add(accept.Name, accept)
        y.Declas.Add(end_.Name, end_)
        accept_line.Grams.Add(end_)
        y.Grammars.Insert(0, accept_line)

        Dim first = y.Grammars.
            Map(Function(x) x.Name).
            SortToList(Function(a, b) a.CompareTo(b)).
            Unique.
            ToHash_ValueDerivation(Function(x) New List(Of Node))

        ' first
        Dim nodes As New List(Of Node)
        For Each g In y.Grammars

            Dim prev As Node = Nothing
            For i = 0 To g.Grams.Count

                Dim p As New Node With {.Name = If(i = 0, g.Name, g.Grams(i - 1).Name)}
                p.Lines.Add(New GrammarLineIndex With {.Line = g, .Index = i})
                If i = 0 Then

                    first(g.Name).Add(p)
                Else
                    prev.NextNodes.Add(p)
                End If
                nodes.Add(p)
                prev = p
            Next
        Next

        ' next
        Do While True

            For Each p In nodes

                For Each head In p.NextNodes.Where(Function(x) first.ContainsKey(x.Name)).Map(Function(x) first(x.Name)).Flatten

                    head.Lines.Do(Sub(x) If p.Lines.FindFirstOrNull(Function(line) x.Equals(line)) Is Nothing Then p.Lines.Add(x))
                    For Each head_next In head.NextNodes.Where(Function(x) Not p.NextNodes.Contains(x))

                        p.NextNodes.Add(head_next)
                        Continue Do
                    Next
                Next
            Next

            Exit Do
        Loop

        ' merge
        Dim to_id = nodes.ToHash_ValueDerivation(Function(x) x.GetHashCode.ToString)
        Do While True

            For Each p In nodes

                For i = 0 To p.NextNodes.Count - 2

                    For j = i + 1 To p.NextNodes.Count - 1

                        If p.NextNodes(i).Name = p.NextNodes(j).Name Then

                            Dim left_node = p.NextNodes(i)
                            Dim right_node = p.NextNodes(j)
                            Dim key = String.Join(","c, to_id(left_node).Split(","c).Join(to_id(right_node).Split(","c)).SortToList)

                            Dim new_node = nodes.FindFirstOrNull(Function(x) to_id(x) = key)
                            If new_node Is Nothing Then

                                new_node = New Node With {.Name = left_node.Name}
                                new_node.Lines.AddRange(left_node.Lines.Join(right_node.Lines).SortToList(Function(a, b) a.GetHashCode - b.GetHashCode).Unique)
                                new_node.NextNodes.AddRange(left_node.NextNodes.Join(right_node.NextNodes).SortToList(Function(a, b) a.GetHashCode - b.GetHashCode).Unique)

                                to_id(new_node) = key
                                nodes.Add(new_node)
                            End If

                            p.NextNodes.Remove(left_node)
                            p.NextNodes.Remove(right_node)
                            p.NextNodes.Add(new_node)

                            Continue Do
                        End If
                    Next
                Next
            Next

            Exit Do
        Loop


        ' mark & sweep
        Dim mark As New Dictionary(Of Node, Boolean)
        Dim mark_proc As Action(Of Node) =
            Sub(x)

                If mark.ContainsKey(x) Then Return
                mark.Add(x, True)
                x.NextNodes.Do(Sub(p) mark_proc(p))
            End Sub
        mark_proc(nodes(0)) ' $ACCEPT

        Return nodes.Where(Function(x) mark.ContainsKey(x)).ToList
    End Function

    Public Shared Function LALR1(y As Syntax, nodes As List(Of Node)) As Tuple(Of Dictionary(Of Node, List(Of String)), Dictionary(Of String, List(Of String)))

        Dim nullable = y.Grammars.Map(Function(x) x.Name).ToHash_ValueDerivation(Function(x) False)
        Do While True

            For Each g In y.Grammars

                If nullable(g.Name) Then Continue For
                If g.Grams.Where(Function(x) Not nullable.ContainsKey(x.Name) OrElse Not nullable(x.Name)).IsNull Then

                    nullable(g.Name) = True
                    Continue Do
                End If
            Next

            Exit Do
        Loop

        Dim lookahead = nodes.ToHash_ValueDerivation(Function(x) New List(Of String))
        Do While True

            For Each p In nodes

                For Each next_ In p.NextNodes

                    Dim name = next_.Name
                    If nullable.ContainsKey(name) AndAlso nullable(name) Then

                        For Each ahead In lookahead(next_).Where(Function(x) Not lookahead(p).Contains(x))

                            lookahead(p).Add(ahead)
                            Continue Do
                        Next

                    ElseIf Not nullable.ContainsKey(name) AndAlso Not lookahead(p).Contains(name) Then

                        lookahead(p).Add(name)
                        Continue Do
                    End If
                Next
            Next

            Exit Do
        Loop

        Dim follow = nullable.Cdr.Map(Function(x) x.Key).ToHash_ValueDerivation(Function(x) New List(Of String))
        nodes.Do(Sub(x) If follow.ContainsKey(x.Name) Then follow(x.Name) = follow(x.Name).Join(lookahead(x)).SortToList.Unique.ToList)
        Do While True

            For Each p In nodes

                For Each line In p.Lines.Where(Function(x) x.Index > 0 AndAlso x.Line.Grams.Count = x.Index AndAlso follow.ContainsKey(x.Line.Grams(x.Index - 1).Name))

                    Dim name = line.Line.Grams(line.Index - 1).Name
                    For Each a In follow(line.Line.Name).Where(Function(x) Not follow(name).Contains(x))

                        follow(name).Add(a)
                        Continue Do
                    Next
                Next
            Next

            For Each p In nodes

                For Each line In p.Lines.Where(Function(x) x.Index > 0 AndAlso x.Line.Grams.Count > x.Index AndAlso follow.ContainsKey(x.Line.Grams(x.Index - 1).Name) AndAlso nullable.ContainsKey(x.Line.Grams(x.Index).Name))

                    Dim name = line.Line.Grams(line.Index - 1).Name
                    For Each a In follow(line.Line.Grams(line.Index).Name).Where(Function(x) Not follow(name).Contains(x))

                        follow(name).Add(a)
                        Continue Do
                    Next
                Next
            Next

            Exit Do
        Loop

        Return Tuple.Create(lookahead, follow)
    End Function

    Public Shared Function LALRParser(y As Syntax, nodes As List(Of Node), lookahead As Dictionary(Of Node, List(Of String)), follow As Dictionary(Of String, List(Of String))) As List(Of Dictionary(Of String, ParserAction))

        Return nodes.Map(
            Function(p)

                Dim line As New Dictionary(Of String, ParserAction)
                For Each shift In p.NextNodes

                    line(shift.Name) = New ShiftAction With {.Next = shift}
                Next

                For Each reduce In p.Lines.Where(Function(x) x.Index = x.Line.Grams.Count)

                    ' any reduce
                    If Not follow.ContainsKey(reduce.Line.Name) Then Continue For

                    For Each r In follow(reduce.Line.Name)

                        If Not line.ContainsKey(r) Then

                            line(r) = New ReduceAction With {.Reduce = reduce.Line}

                        ElseIf TypeOf line(r) Is ShiftAction Then

                            Select Case _
                                If(reduce.Line.Priority > y.Declas(r).Priority, AssocTypes.Left,
                                If(reduce.Line.Priority < y.Declas(r).Priority, AssocTypes.Right,
                                reduce.Line.Assoc))

                                Case AssocTypes.Left
                                    line(r) = New ReduceAction With {.Reduce = reduce.Line}

                                Case AssocTypes.Right

                                Case Else
                                    ' shift/reduce conflict
                                    System.Diagnostics.Debug.Fail("shift/reduce conflict")

                            End Select
                        Else

                            ' reduce/reduce conflict
                            System.Diagnostics.Debug.Fail("reduce/reduce conflict")
                        End If
                    Next
                Next
                Return line
            End Function).ToList
    End Function

End Class
