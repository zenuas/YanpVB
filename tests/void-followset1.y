%token A B C X

%%

s  : A b C
   | b X

b  : void dummy
   | b B

void :
dummy :

%%

(0)
$ACCEPT : . s $END
s       : . A b C
s       : . b X
b       : . void dummy
b       : . b B
void    : . [B, X]

(1)
$ACCEPT : s . $END

(2)
$ACCEPT : s $END .

(3)
s    : A . b C
b    : . void dummy
b    : . b B
void : . [B, C]

(4)
s : A b C . [$END]

(5)
s : b X . [$END]

(6)
b     : void . dummy
dummy : . [B, C, X]

(7)
b : void dummy . [B, C, X]

(8)
b : b B . [B, C, X]

(9)
b : b . B
s : b . X

(10)
b : b . B
s : A b . C
