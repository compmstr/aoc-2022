marker (reload)

: reload (reload) "1-task.fs" included ;

\ \ Stores calories, then elf index
\ variable max 1 cells allot
\ \ Stores calories, then elf index
\ variable current 1 cells allot

\ : calories ;
\ : elf 1 cells + ;

\ 0 current calories !
\ 0 current elf !

\ : number? ( addr -- f ) \ truthy if addr points to a digit character
\     c@ digit?
\     \ If true, than it's ( digit true ), otherwise it's just false
\     dup if
\         \ ." Got digit, swap/dropping" cr
\         \ .s cr
\         swap drop
\     else
\         \ ." No digit, leaving flag in place" cr
\         \ .s cr
\     endif
\ ;

\ : whitespace? ( c -- f )
\     false
\     over 9 = or \ tab
\     over 10 = or \ line feed
\     over 12 = or \ form feed
\     over 13 = or \ cr
\     over 32 = or \ space
\     swap drop
\ ;

\ : trim-string ( c-addr u1 -- c-addr u1 ) \ return the substring after any prefix whitespace
\     \ cr ." trim-string" cr
\     dup 0 > if
\         begin
\             \ cr .s cr
\             over c@
\             whitespace? \ c-addr u1 whitespace?
\             over 0 <=   \ c-addr u1 whitespace? empty?
\             or
\         while
\                 1 - swap 1 + swap \ sub 1 from count, add one to addr
\         repeat
\     endif
\ ;

\ : test-input "1000\n2000\n3000\n\n4000\n\n5000\n6000\n\n7000\n8000\n9000\n\n10000" ;

\ : next-elf? ( c-addr u1 -- f ) \ true if at two newlines
\     dup 2 >= if
\         over dup \ c-addr u1 c-addr c-addr
\         c@ swap 1+ c@ \ c-addr u1 c1 c2
\         dup 10 = -rot \ c-addr u1 newline? c1 c2
\         = and \ c-addr u1 newline-and-equal?
\     else
\         false \ Less than two characters in string, can't be at double newline
\     endif
\ ;

\ : read-row ( fd -- f ) \ Returns true if a line was read into PAD
\     dup file-eof? invert if
\         dup pad 84 rot read-line throw \ len more?
\         drop dup = 0 invert \ has-data?
\     else
\         false
\     endif
\     ;

\ \ Returns:
\ : read-elf ( fd -- count )
\     0 current calories !
\     current elf @ 1+ current elf !
\     begin
\         dup read-row
\     while
\             pad number? if
\                 pad swap 0.0 2swap \ 0 0 <pad> <line-length>
\                 >number 2drop d>s
\                 \ cr ." Got a number: " dup . cr
\                 \ Add to current calorie count
\                 current calories @ + current calories !
\             else
\                 cr ." No number!" cr abort
\             endif
\     repeat
\     2drop \ drop fd and last 0 count
\     current calories @
\ ;

\ ==================== New attempt ====================

\ Stores calories, then elf index
variable max 1 cells allot
\ Stores calories, then elf index
variable current 1 cells allot

: calories ;
: elf 1 cells + ;

0 current calories !
0 current elf !
0 max calories !
0 max elf !

variable current-line
0 current-line !

: elves 2 * cells ;

variable top 3 elves allot

: read-row ( fd -- u f ) \ u is number of chars read, flag is false for EOF
    current-line @ 1+ current-line !
    pad 84 rot read-line throw
;

: next-elf 0 current calories ! current elf @ 1+ current elf ! ;

: pad-has-number?
    pad c@ '0' < pad c@ '9' > or if
        ." Invalid input at line: " current-line ? cr pad swap type cr
        abort" INVALID INPUT"
    endif
;

: has-extra-input?
    0<> if
        ." Invalid input at line (extra characters): " current-line ? cr type cr
        abort" INVALID INPUT"
    endif
    drop
;

: read-elf ( fd -- calories )
    next-elf
    begin
        dup read-row
        over 0<> and
    while
            pad-has-number?
            pad swap 0. 2swap \ 0 0 pad #pad
            >number
            has-extra-input? d>s
            current calories @ + current calories !
    repeat
    2drop
    current calories @
    \ ." Elf: " current elf ? ." has " current calories ? ." calories" cr
;

: update-max ( calories -- )
    max calories @ > if
        current calories @ max calories !
        current elf @ max elf !
    endif
;

variable top-changed?

: update-top ( calories elf -- )
    false top-changed? !
    3 0
    ?do
        top-changed? @ if
            \ Cache old value for next loop
            top i elves + 2@
            \ Save current value to top elves[i]
            2swap top i elves + 2!
        else
            over \ calories elf calories
            top i elves + calories @ > if
                true top-changed? !
                top i elves + 2@
                2swap swap top i elves + 2!
            endif
        endif
    loop
    2drop
;

: .top-elves
    0
    3 0
    ?do
        top i elves + 2@ swap
        ." Elf: " . ." | Calories: " . cr
        top i elves + calories @ +
    loop
    ." Top 3 total: " . cr
;

: read ( c-addr u -- u1 u2 )
    0. max 2!
    0. current 2!
    top 3 elves erase
    r/o open-file throw
    begin
        dup dup file-position throw \ fd fd <position double>
        rot file-size throw d<
    while
            dup read-elf
            dup update-max
            current elf @ update-top
    repeat
    cr ." Elf: " max elf ? ." has the most calories with: " max calories ? cr
    close-file throw
;

\ "1-input" r/o open-file throw

\ TODO - keep track of top 3