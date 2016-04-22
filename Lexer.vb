Imports System
Imports System.IO


Public Class Lexer

    Public Overridable Property Reader As TextReader
    Public Overridable Property StoreToken As Token = Nothing
    Public Overridable Property LineNo As Integer = 1
    Public Overridable Property LineColumn As Integer = 1


    Public Overridable Function ReadText(Optional eofmark As String = "") As String

        If Me.StoreToken IsNot Nothing Then Throw New ParseException(Me.LineNo, Me.LineColumn, "token buffer is not null")

        Dim text As New System.Text.StringBuilder
        Do While Not Me.EndOfStream

            Dim line As String = Me.ReadLine
            If line = eofmark Then Return text.ToString

            text.AppendLine(line)
        Loop

        If eofmark <> "" Then Throw New ParseException(Me.LineNo, Me.LineColumn, "missing eof mark")
        Return text.ToString
    End Function

    Public Overridable Function EndOfToken() As Boolean

        Return (Me.PeekToken().SymbolType = TokenTypes.EOF)
    End Function

    Public Overridable Function PeekToken() As Token

        If Me.StoreToken IsNot Nothing Then Return Me.StoreToken
        If Me.EndOfStream() Then

            Me.StoreToken = New Token(TokenTypes.EOF)
            Return Me.StoreToken
        End If

        Dim buf As New System.Text.StringBuilder
        Dim type = TokenTypes.EOF
        Do
            Dim c As Char = Me.NextChar()
            Select Case True
                Case c = "#"c

                    Me.ReadLine()

                Case c = "/"c

                    Me.ReadChar()
                    If Me.EndOfStream() OrElse Me.ReadChar() <> "/" Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse error (/)")
                    Me.ReadLine()

                Case c = ";"c

                    type = TokenTypes.Char_Grammar_Separator
                    buf.Append(Me.ReadChar())
                    Exit Do

                Case c = ":"c

                    type = TokenTypes.Char_Grammar_Start
                    buf.Append(Me.ReadChar())
                    Exit Do

                Case c = "|"c

                    type = TokenTypes.Char_Grammar_Continue
                    buf.Append(Me.ReadChar())
                    Exit Do

                Case c = "="c

                    type = TokenTypes.Char_Grammar_Eq
                    buf.Append(Me.ReadChar())
                    Exit Do

                Case c = "{"c

                    type = TokenTypes.Char_Grammar_ActionStart
                    buf.Append(Me.ReadChar())
                    Exit Do

                Case Char.IsWhiteSpace(c)

                    Me.ReadChar()

                Case c = "%"c

                    Me.ReadChar()
                    If Me.EndOfStream() Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse error (%)")

                    buf.Append(c)
                    c = Me.ReadChar()
                    If c = "%"c Then

                        type = TokenTypes.PartEnd
                        buf.Append(c)
                        Exit Do

                    ElseIf c = "{"c Then

                        type = TokenTypes.InlineStart
                        buf.Append(c)
                        Exit Do

                    ElseIf Me.IsAlpha(c) Then

                        type = TokenTypes.ReserveToken
                        buf.Append(c)

                        Do While Not Me.EndOfStream()

                            c = Me.NextChar()
                            If Me.IsAlpha(c) Then

                                buf.Append(c)
                                Me.ReadChar()

                            Else

                                Exit Do
                            End If
                        Loop

                        Exit Do

                    Else
                        Throw New ParseException(Me.LineNo, Me.LineColumn, "parse error (%)")
                    End If

                Case Me.IsDigit(c)

                    type = TokenTypes.Numeric
                    buf.Append(c)
                    Me.ReadChar()
                    Do While Not Me.EndOfStream()

                        c = Me.NextChar()
                        If Me.IsDigit(c) Then

                            buf.Append(c)
                            Me.ReadChar()

                        ElseIf Not Me.IsWord(c) Then

                            Exit Do

                        Else
                            Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse digit error ({c})")
                        End If
                    Loop

                    Exit Do

                Case c = "'"c

                    type = TokenTypes.CharValue
                    Me.ReadChar()
                    If Me.EndOfStream Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse char error EOF")

                    c = Me.ReadChar
                    If Me.EndOfStream Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse char error EOF")

                    If c = "\"c Then

                        c = Me.ReadChar
                        If Me.EndOfStream Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse char error EOF")
                        Select Case c

                            Case "'"c,
                                """"c,
                                "\"c

                                buf.Append(c)

                            Case "t"c : buf.Append(Chars.Tab)
                            Case "r"c : buf.Append(Chars.Cr)
                            Case "n"c : buf.Append(Chars.Lf)

                            Case Else
                                Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse escape char error ({c})")
                        End Select

                    Else

                        buf.Append(c)
                    End If

                    If Me.ReadChar <> "'"c Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse char error")

                    Exit Do

                Case c = "<"c

                    type = TokenTypes.DeclareType
                    Me.ReadChar()

                    If Me.EndOfStream Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse type error")
                    c = Me.ReadChar
                    buf.Append(c)

                    If c = "_"c Then

                        Do
                            If Me.EndOfStream Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse type error EOF")

                            c = Me.ReadChar
                            If c = "_"c Then

                                buf.Append(c)

                            ElseIf Me.IsAlpha(c) Then

                                buf.Append(c)
                                Exit Do

                            Else

                                Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse type error ({c})")
                            End If
                        Loop

                    ElseIf Not Me.IsAlpha(c) Then

                        Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse type error ({c})")
                    End If

                    Do While Not Me.EndOfStream()

                        c = Me.NextChar()
                        If Me.IsWord(c) Then

                            buf.Append(c)
                            Me.ReadChar()

                        ElseIf c = ">"c Then

                            Me.ReadChar()
                            Exit Do

                        Else
                            Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse type error ({c})")
                        End If
                    Loop

                    Exit Do

                Case c = "_"c OrElse Me.IsAlpha(c)

                    type = TokenTypes.Token
                    buf.Append(c)
                    Me.ReadChar()
                    If c = "_"c Then

                        Do
                            If Me.EndOfStream Then Throw New ParseException(Me.LineNo, Me.LineColumn, "parse alpha error EOF")

                            c = Me.ReadChar
                            If c = "_"c Then

                                buf.Append(c)

                            ElseIf Me.IsAlpha(c) Then

                                buf.Append(c)
                                Exit Do

                            Else

                                Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse alpha error ({c})")
                            End If
                        Loop
                    End If

                    Do While Not Me.EndOfStream()

                        c = Me.NextChar()
                        If Me.IsWord(c) Then

                            buf.Append(c)
                            Me.ReadChar()

                        Else

                            Exit Do
                        End If
                    Loop

                    Exit Do

                Case Else

                    Throw New ParseException(Me.LineNo, Me.LineColumn, $"parse error ({c})")
            End Select

        Loop While Not Me.EndOfStream()

        Me.StoreToken = New Token(type, buf.ToString)
        Return Me.StoreToken
    End Function

    Public Overridable Function IsWord(c As Char) As Boolean

        Return (c = "_"c OrElse Me.IsAlpha(c) OrElse Me.IsDigit(c))
    End Function

    Public Overridable Function IsAlpha(c As Char) As Boolean

        Return ((c >= "a"c AndAlso c <= "z"c) OrElse (c >= "A"c AndAlso c <= "Z"c))
    End Function

    Public Overridable Function IsDigit(c As Char) As Boolean

        Return (c >= "0"c AndAlso c <= "9"c)
    End Function

    Public Overridable Function NextToken() As Token

        If Me.PeekToken().SymbolType = TokenTypes.EOF Then Throw New InvalidOperationException("NextToken called end-of-token")
        Return Me.PeekToken()
    End Function

    Public Overridable Function ReadToken() As Token

        If Me.PeekToken().SymbolType = TokenTypes.EOF Then Throw New InvalidOperationException("ReadToken called end-of-token")

        Dim t = Me.StoreToken
        Me.StoreToken = Nothing
        Return t
    End Function

    Public Overridable Function EndOfStream() As Boolean

        Return (Me.Reader.Peek() = -1)
    End Function

    Public Overridable Function NextChar() As Char

        If Me.EndOfStream() Then Throw New InvalidOperationException("NextChar called end-of-stream")
        Return Char.ConvertFromUtf32(Me.Reader.Peek())(0)
    End Function

    Public Overridable Function ReadChar() As Char

        If Me.EndOfStream() Then Throw New InvalidOperationException("ReadChar called end-of-stream")
        Dim c = Chars.ToChar(Me.Reader.Read())
        If c = Chars.Lf OrElse (c = Chars.Cr AndAlso Chars.ToChar(Me.Reader.Peek()) <> Chars.Lf) Then

            Me.LineColumn = 1
            Me.LineNo += 1
        Else
            Me.LineColumn += 1
        End If
        Return c
    End Function

    Public Overridable Function ReadLine() As String

        If Not Me.EndOfStream Then

            Me.LineColumn = 1
            Me.LineNo += 1
        End If
        Return Me.Reader.ReadLine
    End Function

End Class
