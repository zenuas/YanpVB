%token A B C X

%%

s  : void1 A
   | void2 B

void1  : void11
void11 :
void2  : void22
void22 :

%%

(0)
$ACCEPT : . s $END
s       : . void1 A
s       : . void2 B
void1   : . void11
void2   : . void22
void11  : . [A}
void22  : . [B]

(1)
$ACCEPT : s . $END

(2)
$ACCEPT : s $END .

(3)
s : void1 . A

(4)
s : void1 A .

(5)
s : void2 . B

(6)
s : void2 B .

(7)
void1 : void11 .

(8)
void2 : void22 .
