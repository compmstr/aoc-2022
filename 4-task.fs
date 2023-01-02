marker (reload)

: reload (reload) "4-task.fs" included ;

: skip-char ( c-addr1 u1 -- c-addr2 u2 )
    1 - swap 1 + swap
;

variable ranges 3 cells allot
: low1 ;
: hi1 1 cells + ;
: low2 2 cells + ;
: hi2 3 cells + ;

\ Contain is when one set of numbers is fully within the other
: 1-contains ( addr - f )
    dup low1 @ over low2 @ <=
    swap dup hi1 @ swap hi2 @ >= and
;

: 2-contains ( addr - f )
    dup low2 @ over low1 @ <=
    swap dup hi2 @ swap hi1 @ >= and
;

: contains? ( addr -- f ) \ true if low1 <= low2 and hi1 >= hi2 (and reversed)
    dup 1-contains
    swap 2-contains or
;

\ Overlap is if any numbers are the same in the sets
: 1-overlaps ( addr -- f )
    dup low1 @ over low2 @ >=
    swap dup low1 @ swap hi2 @ <= and
;

: 2-overlaps ( addr -- f )
    dup low2 @ over low1 @ >=
    swap dup low2 @ swap hi1 @ <= and
;

: overlaps? ( addr -- f ) \ true if the two ranges overlap at all
    dup contains? swap \ contains? addr
    dup 1-overlaps \ contains? addr 1-overlaps
    swap 2-overlaps or or
;

\ Grab the next number from the passed in string
: parse-num ( c-addr u -- c-addr2 u2 n )
    0. 2swap >number 2swap d>s
;

\ parse a "lo-hi,lo-hi" line and check for overlaps
: check-line ( c-addr u -- overlaps? )
    parse-num ranges low1 !
    \ skip '-'
    skip-char parse-num ranges hi1 !
    \ skip ','
    skip-char parse-num ranges low2 !
    \ skip '-'
    skip-char parse-num ranges hi2 !
    2drop
    ranges overlaps?
;

variable overlap-pairs

\ Check for overlaps in each line of the passed in file
: check-lines ( c-addr u -- overlap-pairs )
    0 overlap-pairs !
    r/o open-file throw
    begin
        dup pad 84 rot read-line throw
    while
            pad swap check-line if
                1 overlap-pairs +!
            endif
    repeat
    drop \ drop last character count
    close-file throw
    overlap-pairs @
;