%left ELSE

%%
stmt : if
     | '1'
if : IF expr stmt
   | IF expr stmt ELSE stmt
expr : '2'
