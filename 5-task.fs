marker (reload)

: reload (reload) "5-task.fs" included ;

variable line
variable max-line
variable stacks
variable #stacks

: stack ( n -- addr )
    cells stacks @ +
;

: clear-stacks
    stacks @ if
        #stacks @ 0 ?do
            i stack @ free throw
        loop
        0 stacks !
        0 #stacks !
    endif
;

: dump-stack ( n -- )
    stack @ 72 dump
;

variable found?

: w? ( c -- f ) 32 <= ;
: skip ( c-addr u -- c-addr u )
    begin
        over c@ w?
        over 0<> and
    while
            1 -
            swap 1 +
            swap
    repeat
;

0 value fd-in

: open
    "5-input" r/o open-file throw to fd-in
;

variable <last-number>
: last-number ( c-addr u -- n )
    0 <last-number> !
    begin
        skip 0. 2swap >number
        2swap d>s dup 0<>
    while
            <last-number> !
    repeat
    drop 2drop <last-number> @
;

: reset
    0. fd-in reposition-file throw
    0 line !
    0 max-line !
    false found? !
    clear-stacks
;

: allocate-array
    \ Allocate an array of #stacks pointers
    #stacks @ cells dup
    ." Allocating " dup . ." bytes " #stacks @ . ." cells of space for stacks" cr
    allocate throw stacks !
    stacks @ swap 42 fill
;

: allocate-stacks
    \ Calculate the max string size
    #stacks @ line @ *
    ." Each stack will hold up to " dup . ." characters" cr
    \ Set up each of the stack strings
    #stacks @ 0 ?do
        dup allocate throw
        2dup swap erase
        i stack !
    loop
    drop
;

: (allocate)
    allocate-array
    allocate-stacks
;

: init
    open reset
    begin
        pad 84 fd-in read-line throw
        found? @ invert and
        \ ." Handling line: " line @ . ." -- " .s cr
    while
            pad swap last-number dup 0<> if
                #stacks !
                true found? !
            else
                drop
                1 line +!
            endif
    repeat
    \ drop last count
    drop
    line @ 1 - max-line !
    (allocate)
    \ Empty line is skipped by last run through begin..while loop
;

: stack-char
    over c@ [char] [ = if
        over 1+ c@ true
    else
        false
    endif
;

: next-stack
    4 -
    swap 4 +
    swap
;

: row-done?
    dup 0 <=
;

: number?
    skip 0. 2swap >number 2swap d>s 0<>
;

: stack-end ( i -- addr ) \ return the first 0-value character in the stack
    stack @
    begin
        dup c@ 0<>
    while
            1 +
    repeat
;

: push-stack ( c i -- )
    \ ." PUSH-STACK: " 2dup 1 + . ." " emit cr
    stack-end !
;

: pop-stack ( i -- c )
    dup stack @ swap stack-end
    1 - \ go back to the last non-zero character
    dup -rot > if
        abort" stack underflow"
    endif
    dup c@ swap
    0 swap c!
;

variable cur-stack
: build-line ( c-addr u -- )
    0 cur-stack !
    begin
        row-done? invert
    while
            stack-char if
                cur-stack @ stack @   \ start of stack string
                max-line @ line @ - + \ offset based on total number of characters
                c!
            endif
            next-stack
            1 cur-stack +!
    repeat
    2drop
;
: build
    0 line !
    0. fd-in reposition-file throw
    begin
        pad 84 fd-in read-line throw
        >r pad swap number? invert r> and
    while
            build-line
            1 line +!
    repeat
    2drop
;

: parse-number
    parse-name 0. 2swap >number
    assert( 0= )
    drop d>s
;

: move
    parse-number
;

: from
    parse-number 1-
;

: to
    parse-number 1- ( count from to )
    \ Task 1 version -- push/pop one at a time
    \ swap
    \ rot 0 ?do
    \     2dup pop-stack swap push-stack
    \ loop
    \ 2drop
    \ TODO task 2 -- move blocks of letters instead of single letters
    stack-end
    -rot stack-end over - \ dest count source
    dup >r \ save copy of original stack-end on from to return stack
    -rot \ source dest count
    dup >r \ save copy of count to return stack
    cmove \ move characters
    r> r> \ count stack-end
    swap erase
;

: run
    init build
    begin
        pad 84 fd-in read-line throw
    while
            dup 0<> if
                pad swap
                evaluate
            else
                drop
            endif
            1 line +!
    repeat
    drop
    ." Result:" cr
    #stacks @ 0 ?do
        i pop-stack emit
    loop
;