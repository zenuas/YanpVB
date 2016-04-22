%token ID

%%

expr    : left '=' right
        | right
left    : right
right   : num
        | num '+' right
        | num '-' right
num     : sign ID
sign    : void
        | '-'
void    :
