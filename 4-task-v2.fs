marker (reload)
: reload (reload) "4-task-v2.fs" included ;

: b.s base @ >r 2 base ! .s r> base ! ;

\ Using doubles since all of the bit values are < 100, and 2 cells gives me 128 bits to work with
: dand rot swap and -rot and swap ;
: dor rot swap or -rot or swap ;

: number ( c-a u - c-a2 u2 n ) 0. 2swap >number 2swap d>s ;
: advance ( c-a u - c-a2 u2 ) 1 - swap 1 + swap ;
: set-bits ( lo hi - du ) 0. 2swap swap 1 - ?do 1. i dlshift dor loop ;

\ read one pair of numbers separated by a character
: (read) number -rot advance number -rot 2swap set-bits 2swap ;
: read (read) advance (read) 2drop ;

: 4dup 3 pick 3 pick 3 pick 3 pick ;

\ check if a AND b matches either a or b
: contains? read 4dup dand 2dup 2>r d= -rot 2r> d= or ;
\ check if a AND b <>0
: intersects? read dand d0<> ;

0 value fd-in
: open "4-input" r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;
: read pad 80 fd-in read-line throw ;

: part1 0 open
    begin read
    while pad swap contains? if 1 + endif repeat drop ;

: part2 0 open
    begin read
    while pad swap intersects? if 1 + endif repeat drop ;