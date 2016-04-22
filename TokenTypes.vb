Public Enum TokenTypes

    Token
    Numeric
    CharValue
    ''' <summary>&lt;xxx&gt;</summary>
    DeclareType
    ''' <summary>%xxx</summary>
    ReserveToken
    ''' <summary>%{</summary>
    InlineStart
    ''' <summary>%%</summary>
    PartEnd
    EOF

    ''' <summary>;</summary>
    Char_Grammar_Separator
    ''' <summary>:</summary>
    Char_Grammar_Start
    ''' <summary>|</summary>
    Char_Grammar_Continue
    ''' <summary>=</summary>
    Char_Grammar_Eq
    ''' <summary>{</summary>
    Char_Grammar_ActionStart ' {
End Enum
