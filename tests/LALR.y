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

%%

(0)
$ACCEPT : . expr $END
expr : . left '=' right
expr : . right
left : . right
right : . num
right : . num '+' right
right : . num '-' right
num : . sign ID
sign : . void
sign : . '-'
void : . [ID]

(1)
$ACCEPT : expr . $END

(2)
$ACCEPT : expr $END .

(3)
expr : left . '=' right

(4)
expr : left '=' . right
right : . num
right : . num '+' right
right : . num '-' right
num : . sign ID
sign : . void
sign : . '-'
void : . [ID]

(5)
expr : left '=' right .

(6)
right : num '+' . right
right : . num
num : . sign ID
right : . num '+' right
right : . num '-' right
sign : . void
sign : . '-'
void : . [ID]

(7)
right : num '+' right .

(8)
right : num '-' . right
right : . num
num : . sign ID
right : . num '+' right
right : . num '-' right
sign : . void
sign : . '-'
void : . ID

(9)
right : num '-' right .

(10)
num : sign . ID

(11)
num : sign ID .

(12)
sign : void .

(13)
sign : '-' .

(14)
expr : right . [$END]
left : right . ['=']

(15)
right : num . '-' right
right : num . '+' right
right : num .
