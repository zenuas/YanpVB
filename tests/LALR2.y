%token ID

%%

expr    : left '=' right
        | right
left    : right
right   : ID

