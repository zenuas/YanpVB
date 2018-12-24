Imports System
Imports System.Collections.Generic


Public Class Generator

    Public Shared Function LR0(y As Syntax) As List(Of Node)

        If y.Grammars.Count = 0 Then Throw New ParseException("no grammar has been specified")
        If y.Grammars.FindFirstOrNull(Function(x) x.Name = y.Start) Is Nothing Then Throw New ParseException($"the start symbol {y.Start} is undefined")

        ' $ACCEPT : y.Start $END
        Dim accept As New Declarate With {.Name = "$ACCEPT", .IsTerminalSymbol = False}
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

            Dim retry = False
            For Each p In nodes

                For Each head In p.NextNodes.Where(Function(x) first.ContainsKey(x.Name)).Map(Function(x) first(x.Name)).Flatten.ToList

                    head.Lines.Each(Sub(x) If Not p.Lines.Contains(x) Then p.Lines.Add(x))
                    For Each head_next In head.NextNodes.Where(Function(x) Not p.NextNodes.Contains(x))

                        p.NextNodes.Add(head_next)
                        retry = True
                    Next
                Next
            Next

            If Not retry Then Exit Do
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
                x.NextNodes.Each(Sub(p) mark_proc(p))
            End Sub
        mark_proc(nodes(0)) ' $ACCEPT

        Return nodes.Where(Function(x) mark.ContainsKey(x)).ToList
    End Function

    Public Shared Function LALR1(y As Syntax, nodes As List(Of Node)) As Dictionary(Of Node, Dictionary(Of GrammarLine, HashSet(Of String)))

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

        Dim first = nodes.ToHash_ValueDerivation(Function(x) x.NextNodes.Map(Function(p) p.Name).Where(Function(name) Not nullable.ContainsKey(name)).ToHashSet)
        Dim lookahead = nodes.ToHash_ValueDerivation(Function(x) New Dictionary(Of GrammarLine, HashSet(Of String)))
        Dim search_head =
            Function(p As Node, reduce As String) As List(Of String)

                Dim hash As New HashSet(Of String) From {reduce}
                Do While True

                    For Each lin In p.Lines.Where(Function(x) Not hash.Contains(x.Line.Name) AndAlso
                                                              x.Index < x.Line.Grams.Count AndAlso
                                                              hash.Contains(x.Line.Grams(x.Index).Name) AndAlso
                                                              x.Line.Grams.Range(x.Index + 1).And(Function(g) nullable.ContainsKey(g.Name) AndAlso nullable(g.Name)))

                        hash.Add(lin.Line.Name)
                        Continue Do
                    Next

                    Exit Do
                Loop

                Return hash.ToList
            End Function
        Dim search_next =
            Function(p As Node, head As String) As HashSet(Of String)

                Dim hash As New HashSet(Of String)
                Dim next_first As Action(Of Node, String) =
                    Sub(p2, head2)

                        p2.NextNodes.Where(Function(x) x.Name.Equals(head2)).Each(
                            Sub(n)

                                first(n).Each(Sub(x) hash.Add(x))
                                n.NextNodes.Where(Function(x) nullable.ContainsKey(x.Name) AndAlso nullable(x.Name)).Each(Sub(x) next_first(n, x.Name))
                            End Sub)
                    End Sub
                next_first(p, head)
                Return hash
            End Function

        For Each p In nodes

            For Each index In p.Lines

                If index.Index < index.Line.Grams.Count Then Continue For

                Dim line = index.Line
                Dim reduce = line.Name
                lookahead(p).Add(line, New HashSet(Of String))
                search_head(p, reduce).Each(Sub(head) search_next(p, head).Each(Sub(x) lookahead(p)(line).Add(x)))
            Next
        Next

        Dim follow = nullable.Cdr.Map(Function(x) x.Key).ToHash_ValueDerivation(Function(x) New HashSet(Of String))
        nodes.Each(Sub(x) If follow.ContainsKey(x.Name) Then follow(x.Name) = follow(x.Name).Join(first(x)).ToHashSet)
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

        lookahead.Values.Each(Sub(look) look.ToList.Each(Sub(head) If head.Value.IsNull AndAlso follow.ContainsKey(head.Key.Name) Then look(head.Key) = follow(head.Key.Name)))

        Return lookahead
    End Function

    Public Shared Function LALRParser(
            y As Syntax,
            nodes As List(Of Node),
            lookahead As Dictionary(Of Node, Dictionary(Of GrammarLine, HashSet(Of String)))
        ) As Tuple(Of List(Of Dictionary(Of String, ParserAction)), Dictionary(Of Integer, List(Of String)), Dictionary(Of Integer, GrammarLine))

        Dim conflict As New Dictionary(Of Integer, List(Of String))
        Dim anyreduce As New Dictionary(Of Integer, GrammarLine)
        Return Tuple.Create(nodes.Map(
            Function(p, i)

                Dim line As New Dictionary(Of String, ParserAction)
                For Each shift In p.NextNodes

                    line(shift.Name) = New ShiftAction With {.Next = shift}
                Next

                Dim add_conflict =
                    Sub(e As String)

                        If Not conflict.ContainsKey(i) Then conflict.Add(i, New List(Of String))
                        conflict(i).Add(e)
                    End Sub

                Dim add_action =
                    Sub(name As String, reduce As GrammarLine)

                        If Not line.ContainsKey(name) Then

                            line(name) = New ReduceAction With {.Reduce = reduce}

                        ElseIf TypeOf line(name) Is ShiftAction Then

                            Dim decla = y.Declas(name)
                            Select Case _
                                If(reduce.Priority > decla.Priority, AssocTypes.Right,
                                If(reduce.Priority < decla.Priority, decla.Assoc,
                                reduce.Assoc))

                                Case AssocTypes.Left

                                Case AssocTypes.Right
                                    line(name) = New ReduceAction With {.Reduce = reduce}

                                Case Else
                                    ' shift/reduce conflict
                                    add_conflict($"shift/reduce conflict ([shift] {CType(line(name), ShiftAction).Next.Name}, [reduce] {reduce.ToString})")

                            End Select
                        Else

                            ' reduce/reduce conflict
                            add_conflict($"reduce/reduce conflict ([reduce] {CType(line(name), ReduceAction).Reduce.ToString}, [reduce] {reduce.ToString})")
                        End If
                    End Sub

                For Each r In lookahead(p)

                    Dim reduce = r.Key
                    If r.Value.IsNull Then

                        ' any reduce
                        anyreduce.Add(i, reduce)
                        y.Declas.Values.Where(Function(x) x.IsTerminalSymbol).Each(Sub(x) add_action(x.Name, reduce))
                        Continue For
                    End If
                    r.Value.Each(Sub(x) add_action(x, reduce))
                Next
                Return line
            End Function).ToList, conflict, anyreduce)
    End Function

End Class
