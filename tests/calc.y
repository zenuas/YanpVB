%default Integer

%left  '+' '-'
%left  '*' '/'
%right '^'

%%

stmt : expr          {Console.WriteLine($1)}
     | stmt expr     {Console.WriteLine($2)}

expr : expr '+' expr {$$ = $1 + $3}
     | expr '-' expr {$$ = $1 - $3}
     | expr '*' expr {$$ = $1 * $3}
     | expr '/' expr {$$ = $1 \ $3}
     | expr '^' expr {$$ = CInt(Math.Pow($1, $3))}
     | '(' expr ')'  {$$ = $2}
     | NUM
