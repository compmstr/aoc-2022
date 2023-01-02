\ using some tricks from:
\ https://gitlab.cs.washington.edu/fidelp/advent-of-code-2022/-/blob/main/advent-2022-01.fs

marker (reload)
: reload (reload) "1-task-v2.fs" included ;

variable cur-calories
variable max-calories ( for part A )
create top-three 3 cells allot ( for part b )

0 value fd-in
defer elf-done
variable fd-not-eof
: open "1-input" r/o open-file throw to fd-in true fd-not-eof ! ;
: get-line pad 80 fd-in read-line throw fd-not-eof ! ;
: to-number pad swap 0. 2swap >number 2drop d>s ;
: get-number
    get-line to-number
    dup if cur-calories +! else drop elf-done endif ;
: close fd-in close-file throw 0 to fd-in ;

: (init) 0 max-calories ! 0 cur-calories !
    top-three 3 cells erase ;
: (go) open true begin fd-not-eof @ while get-number repeat drop close ;

: (partA)
    cur-calories @ max-calories @ max max-calories ! ;

: 3@ ( a -- n n n )
    dup 2@ rot 2 cells + @ ;

: go
    (go) cr
    ." Part A:" max-calories ? cr
    ." Part B:" top-three 3@ + + . ;

: pivot ( a b -- min max ) 2dup > if swap then ;

: (partB)
    top-three 3@
    cur-calories @
    pivot top-three !
    pivot top-three 1 cells + !
    pivot top-three 2 cells + !
    drop ;

: (parts) (partA) (partB) 0 cur-calories ! ;
' (parts) is elf-done