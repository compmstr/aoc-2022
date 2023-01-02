marker (reload)
: reload (reload) "8-task.fs" included ;

\ page reload cr go

0 value fd-in
: open "8-input" r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;

99 value size
variable grid
0 grid !

: reset-grid
    grid @ dup 0<> if free throw else drop endif
    size size * allocate throw grid ! ;

size size * 8 / 1 + constant bitset-size
create bitset bitset-size allot
bitset bitset-size erase
: c-or! ( n addr -- ) >r r@ c@ or r> c! ;
: set-bit ( n -- ) 8 /mod bitset + 1 rot lshift swap c-or! ;
: bit-set? ( n -- f ) 8 /mod bitset + c@ 1 rot lshift swap and 0<> ;
: reset-bitset bitset bitset-size erase ;
: bitset-count
    0 bitset-size bitset + bitset do
        8 0 do j c@ 1 i lshift and 0<> if 1 + endif loop loop ;

defer read
: (read) open 0
    begin dup grid @ + size 1 + fd-in read-line throw
    while drop size + repeat 2drop close ;
' (read) is read

: grid-idx ( x y -- u ) size * + ;
: grid-char ( x y -- addr ) grid-idx grid @ + c@ ;

0 value direction
: left 0 to direction ;
: right 1 to direction ;
: top 2 to direction ;
: bottom 3 to direction ;

\ =============== Part 1 ===============

\ Have a last height for each direction
create (max-height) 4 cells allot
: max-height (max-height) direction cells + ;

\ Have a coord calculator for each direction
: left-coord ( x y -- idx ) grid-idx ;
: right-coord ( x y -- idx ) size * swap size 1- swap - + ;
: top-coord ( y x -- idx ) swap size * + ;
: bottom-coord ( y x -- idx ) swap size 1- swap - size * + ;
create (coord) ' left-coord , ' right-coord , ' top-coord , ' bottom-coord ,
: coord (coord) direction cells + @ execute ;

: [rc] 0 max-height ! ;
: reset-check left [rc] right [rc] top [rc] bottom [rc] ;
: part1 2dup coord max-height @ over grid @ + c@ 2dup >= if
        2drop drop else max max-height ! set-bit endif ;
: init-height -1 max-height ! ;

\ =============== Part 2 ===============
\ Have a mover for each direction
\   All are ( x y -- x2 y2 f )
\   The flag is true until you're past the edge
: left-mover ( x y -- x2 y f ) swap 1+ swap over size < ;
: right-mover ( x y -- x2 y f ) swap 1- swap over 0>= ;
: top-mover ( x y -- x y2 f ) 1+ dup size < ;
: bottom-mover ( x y -- x y2 f ) 1- dup 0>= ;
create (mover) ' left-mover , ' right-mover , ' top-mover , ' bottom-mover ,
: mover (mover) direction cells + @ execute ;

variable max-score 0 max-score !
: score 2drop * * * max-score @ max max-score ! ;
variable blocked
: (part2) 2dup grid-char >r 0 -rot false blocked !
    begin mover blocked @ 0= and
    while rot 1+ -rot \ add to the score
            2dup grid-char r@ >= if true blocked ! endif
    repeat 2drop rdrop ;
: step 2dup (part2) -rot ;
: part2 left step right step top step bottom step score ;

create last-coords 2 cells allot

: check
    size 0 do
        left init-height
        right init-height
        top init-height
        bottom init-height
        size 0 do
            i j
            2dup last-coords 2!
            left part1 right part1 top part1 bottom part1
            part2
        loop
    loop
;

: reset reset-grid reset-bitset reset-check ;
: (init) reset read ;

: go (init) check cr ." Visible trees: " bitset-count . cr ." Max score: " max-score @ . cr ;

\ ==================== Tests ====================

: .bitset
    size 0 do
        i 1+ 2 .r space
        size 0 do
            i j left-coord bit-set? if i j grid-char emit else [char] . emit endif
        loop
        cr
    loop
;

: coord-test
    0 0 left coord assert( 0 0 grid-idx = )
    0 0 top coord assert( 0 0 grid-idx = )
    0 0 right coord assert( size 1- 0 grid-idx = )
    0 0 bottom coord assert( 0 size 1- grid-idx = )
    1 0 left coord assert( 1 0 grid-idx = )
    1 0 top coord assert( 0 1 grid-idx = )
    1 0 right coord assert( size 2 - 0 grid-idx = )
    1 0 bottom coord assert( 0 size 2 - grid-idx = )
    0 1 left coord assert( 0 1 grid-idx = )
    0 1 right coord assert( size 1- 1 grid-idx = )
    0 1 top coord assert( 1 0 grid-idx = )
    0 1 bottom coord assert( 1 size 1 - grid-idx = )
;

: example-read "3037325512653323354935390" grid @ swap cmove ;
: example-go
    5 to size
    reset-grid
    ['] example-read is read
    go
;
: example-end
    99 to size
    ['] (read) is read
;