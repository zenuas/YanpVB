Imports System.Text
Imports System.Collections.Generic


Public Class Parser

    Public Shared Sub ParseDeclaration(y As Syntax, lex As Lexer)

        Dim priority = 0

        Dim read_assoc =
            Sub(assoc As AssocTypes, pri As Integer, istype As Boolean)

                If lex.EndOfToken Then Throw New ParseException(lex.LineNo, lex.LineColumn, $"assoc {assoc.ToString} with token")

                Dim type = ""
                If lex.NextToken.SymbolType = TokenTypes.DeclareType Then

                    type = lex.ReadToken.Name

                ElseIf istype Then

                    Throw New ParseException(lex.LineNo, lex.LineColumn, $"assoc {assoc.ToString} with <decla-type>")
                End If

                If lex.PeekToken.SymbolType <> TokenTypes.Token AndAlso lex.PeekToken.SymbolType <> TokenTypes.CharValue Then Throw New ParseException(lex.LineNo, lex.LineColumn, $"assoc {assoc.ToString} with token")
                Do
                    Dim token = lex.ReadToken
                    y.Declas.Add(token.Name, New Declarate With {.Name = token.Name, .Assoc = assoc, .Priority = pri, .Type = type})

                Loop While lex.PeekToken.SymbolType = TokenTypes.Token OrElse lex.PeekToken.SymbolType = TokenTypes.CharValue

            End Sub

        Do While Not lex.EndOfToken

            Dim token As Token = lex.ReadToken

            Select Case token.SymbolType

                Case TokenTypes.PartEnd

                    Exit Do

                Case TokenTypes.InlineStart

                    y.HeaderCode.AppendLine(lex.ReadText("%}"))

                Case TokenTypes.ReserveToken

                    priority += 1

                    Select Case token.Name
                        Case "%token" : read_assoc(AssocTypes.Type, priority, False)
                        Case "%left" : read_assoc(AssocTypes.Left, priority, False)
                        Case "%right" : read_assoc(AssocTypes.Right, priority, False)
                        Case "%nonassoc" : read_assoc(AssocTypes.Nonassoc, priority, False)
                        Case "%type" : read_assoc(AssocTypes.Type, priority, True)
                        Case "%union"
                        Case "%extern"
                        Case "%default"

                            If lex.EndOfToken Then Throw New ParseException(lex.LineNo, lex.LineColumn, "%default with token")
                            y.Default = lex.ReadToken.Name

                        Case "%define"

                            If lex.EndOfToken Then Throw New ParseException(lex.LineNo, lex.LineColumn, "%define with name")
                            Dim name = lex.ReadToken.Name

                            If lex.EndOfToken Then Throw New ParseException(lex.LineNo, lex.LineColumn, "%define with value")
                            y.Defines.Add(name, lex.ReadToken.Name)

                        Case "%start"

                            If lex.EndOfToken Then Throw New ParseException(lex.LineNo, lex.LineColumn, "%start with token")
                            token = lex.ReadToken
                            If token.SymbolType <> TokenTypes.Token Then Throw New ParseException(lex.LineNo, lex.LineColumn, "%start with token")
                            y.Start = token.Name

                        Case Else
                            Throw New ParseException(lex.LineNo, lex.LineColumn, $"unkown token ({token.Name})")
                    End Select

                Case Else

                    Throw New ParseException(lex.LineNo, lex.LineColumn, "bad sequence declaration token")
            End Select
        Loop
    End Sub

    Public Shared Sub ParseGrammar(y As Syntax, lex As Lexer)

        Dim action_count = 0

        Dim get_register_decla =
            Function(token As Token) As Declarate

                If Not y.Declas.ContainsKey(token.Name) Then y.Declas.Add(token.Name, New Declarate With {.Name = token.Name, .Assoc = AssocTypes.Type})
                Return y.Declas(token.Name)
            End Function

        Do While Not lex.EndOfToken

            Dim token = lex.ReadToken

            Select Case token.SymbolType

                Case TokenTypes.Token

                    If y.Start = "" Then y.Start = token.Name
ReStart_:
                    If lex.PeekToken.SymbolType <> TokenTypes.Char_Grammar_Start Then Throw New ParseException(lex.LineNo, lex.LineColumn, "gram start separater")
                    lex.ReadToken()

                    Dim create_gram = Function(t As Token) New GrammarLine With {.Name = t.Name, .Assoc = get_register_decla(t).Assoc, .Priority = get_register_decla(t).Priority}
                    Dim line = create_gram(token)
                    y.Grammars.Add(line)

                    Dim trim_action =
                        Sub()

                            Dim last_line = y.Grammars(y.Grammars.Count - 1)
                            If last_line.HasAction Then

                                line.Action = last_line.Action
                                line.Grams.RemoveAt(line.Grams.Count - 1)
                                y.Grammars.Remove(last_line)
                                y.Declas.Remove(last_line.Name)
                                action_count -= 1
                            End If
                        End Sub


                    Do While Not lex.EndOfToken

                        Dim t = lex.PeekToken
                        Select Case t.SymbolType

                            Case TokenTypes.PartEnd

                                trim_action()
                                Exit Do

                            Case TokenTypes.Char_Grammar_Separator

                                trim_action()
                                lex.ReadToken()
                                Exit Do

                            Case TokenTypes.Char_Grammar_Continue

                                trim_action()
                                lex.ReadToken()
                                line = create_gram(token)
                                y.Grammars.Add(line)

                            Case TokenTypes.Char_Grammar_ActionStart

                                lex.ReadToken()
                                action_count += 1
                                Dim action As New Declarate With {.Name = $"{{{action_count}}}", .Assoc = AssocTypes.Type}
                                line.Grams.Add(action)
                                Dim action_line As New GrammarLine With {.Name = action.Name, .Action = ReadAction(lex)}
                                y.Grammars.Add(action_line)
                                y.Declas.Add(action.Name, action)

                            Case TokenTypes.Token,
                                TokenTypes.CharValue

                                lex.ReadToken()
                                If lex.PeekToken.SymbolType = TokenTypes.Char_Grammar_Start Then

                                    trim_action()
                                    token = t
                                    GoTo ReStart_
                                Else

                                    Dim x = get_register_decla(t)
                                    If line.Priority < x.Priority Then

                                        line.Priority = x.Priority
                                        line.Assoc = x.Assoc
                                    End If
                                    line.Grams.Add(x)
                                End If

                            Case TokenTypes.Char_Grammar_Eq,
                                TokenTypes.ReserveToken

                                If t.SymbolType = TokenTypes.ReserveToken Then

                                    If t.Name <> "%prec" Then Throw New ParseException(lex.LineNo, lex.LineColumn, "bad sequence grammar %prec token")
                                End If
                                lex.ReadToken()

                                Dim prec = lex.PeekToken
                                If prec.SymbolType <> TokenTypes.Token AndAlso prec.SymbolType <> TokenTypes.CharValue Then Throw New ParseException(lex.LineNo, lex.LineColumn, "bad sequence grammar prec token")
                                line.Assoc = get_register_decla(prec).Assoc
                                line.Priority = get_register_decla(prec).Priority
                                lex.ReadToken()

                            Case Else

                                Throw New ParseException(lex.LineNo, lex.LineColumn, "bad sequence grammar token")
                        End Select
                    Loop

                Case TokenTypes.PartEnd

                    Return

                Case TokenTypes.InlineStart

                    y.HeaderCode.AppendLine(lex.ReadText("%}"))

                Case TokenTypes.Char_Grammar_Separator

                    ' nothing

                Case Else

                    Throw New ParseException(lex.LineNo, lex.LineColumn, "bad sequence grammar token")
            End Select
        Loop
    End Sub

    Public Shared Function ReadAction(lex As Lexer) As String

        'If lex.ReadChar <> "{"c Then Throw New ParseException(lex.LineNo, lex.LineColumn, "action read start")

        Dim buf As New StringBuilder

        Dim indent = 0
        Dim instr As Char? = Nothing
        Do While Not lex.EndOfStream

            Dim c = lex.ReadChar

            Select Case c
                Case """"c, "'"c

                    buf.Append(c)
                    If instr.HasValue AndAlso instr.Value = c Then

                        instr = Nothing

                    ElseIf Not instr.HasValue Then

                        instr = c
                    End If

                Case "\"c

                    buf.Append(c)
                    If instr.HasValue Then

                        If lex.EndOfStream Then Throw New ParseException(lex.LineNo, lex.LineColumn, "action read EOF")
                        buf.Append(lex.ReadChar)
                    End If

                Case "{"c

                    buf.Append(c)
                    indent += 1

                Case "}"c

                    If Not instr.HasValue Then

                        If indent <= 0 Then Return buf.ToString
                        indent -= 1
                    End If
                    buf.Append(c)

                Case Else

                    buf.Append(c)
            End Select

        Loop

        Throw New ParseException(lex.LineNo, lex.LineColumn, "action read EOF")
    End Function

End Class
