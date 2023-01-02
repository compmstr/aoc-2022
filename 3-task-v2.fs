marker (reload)
: reload (reload) "3-task-v2.fs" included ;

128 8 / constant bitmask-size \ 128 bits (one for each ascii char)
: bitmask create here bitmask-size allot bitmask-size erase ;
: bitmask-clear bitmask-size erase ;
bitmask bitmask1
bitmask bitmask2
: bitmasks-clear bitmask1 bitmask-clear bitmask2 bitmask-clear ;

: compartments ( c-a u -- c-a2 u2 c-a3 u3 )
    2 / 2dup swap over + swap ;
: andc! ( addr c -- ) over c@ and swap c! ;
: orc! ( addr c -- ) over c@ or swap c! ;
: read-compartment ( bitmask ca u -- )
    over + swap ?do \ i is char* into string
        dup i c@ 8 / + \ ( bm bm+offset )
        1 i c@ 8 mod lshift orc! loop drop ; \ get bit within byte to flip, and set it
: 2exp 8 0 ?do 1 i lshift over and if drop i unloop exit endif loop ;
: (intersect) ( bitmask1 bitmask2 -- ) \ stores in bitmask1
    bitmask-size 0 ?do 2dup i + c@ swap i + swap andc! loop 2drop ;
: intersect bitmask1 bitmask2 (intersect) ;

: bitmask-char ( bitmask -- char ) \ first character set in bitmask
    bitmask-size 0 ?do
        i over + c@ dup 0<> if
            2exp i 8 * +
            unloop swap drop exit
        else drop
        endif
    loop
;
: (priority) ( c -- n )
    dup [char] Z <= if [char] A - 26 + else [char] a - endif 1 + ;
: priority bitmask1 bitmask-char (priority) ;
: rucksack
    compartments
    bitmask1 -rot read-compartment
    bitmask2 -rot read-compartment
    intersect priority
;

0 value fd-in
: open "3-input" r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;

false value more?
: line fd-in read-line throw to more? ;
: (init) open 0 true to more? ;

: part1
    (init)
    begin pad 80 line more?
    while bitmasks-clear pad swap rucksack + repeat
    drop close ;

: readmask pad 80 line pad swap read-compartment ;
: read1 bitmask1 readmask ;
: read2 bitmask2 bitmask-clear bitmask2 readmask intersect ;
: part2
    (init)
    begin bitmasks-clear read1 more?
    while read2 read2 priority + repeat ;
