%default Integer

%left  '+' '-'
%left  '*' '/'
%right '^'

%%

expr : expr '+' expr {$$ = $1 + $3}
     | expr '-' expr {$$ = $1 - $3}
     | expr '*' expr {$$ = $1 * $3}
     | expr '/' expr {$$ = $1 \ $3}
     | expr '^' expr {$$ = CInt(Math.Pow($1, $3))}
     | '(' expr ')'  {$$ = $2}
     | NUM
