marker (reload)
: reload (reload) clearstack "11-task.fs" included ;

: skip 1+ swap 1- swap ;
: parse-number parse-name s>number? assert( 0<> ) d>s ;
: drop-name parse-name 2drop ;
: *! ( n addr -- ) dup @ rot * swap ! ;

( ==================== Data storage ==================== )

struct
    cell% 40 * field ape-items
    cell% field ape-op
    cell% field ape-op-arg
    cell% field ape-mod
    cell% field ape-true
    cell% field ape-false
end-struct ape%
ape% 8 * %allot here constant end-apes constant apes
apes end-apes apes - erase

0 value (current-ape)
: ape apes swap ape% %size * + ;
: current-ape (current-ape) ape ;
: .ape ape% %size dump ;
: .cape current-ape .ape ;
: cape-true current-ape ape-true @ ;
: cape-false current-ape ape-false @ ;

: next-item ( ape-addr -- addr ) ape-items dup 40 cells + swap +do
    i @ 0= if i leave endif cell +loop ;
: add-item ( n ape-addr -- ) next-item ! ;
: last-item ( appe-addr -- addr ) >r r@ next-item cell - dup assert( r> ape-items >= ) ;
: remove-items ( ape-addr -- ) ape-items 40 cells erase ;

( ==================== Input file parsing ==================== )

: monkey [char] : parse s>number? assert( 0<> ) d>s to (current-ape) drop-name ;
: starting noop ;
: parse-item ( "number" -- n f ) #0. parse-name >number 0<> swap drop -rot d>s swap ;
: items: begin parse-item while current-ape add-item repeat current-ape add-item ;
: operation: drop-name drop-name ;
: parse-op bl parse find-name current-ape ape-op ! ;
: parse-arg parse-name 2dup "old" compare 0=
    if 2drop -1 \ 'old', put a negative number as an indicator
    else #0. 2swap >number 2drop d>s endif \ a number, store it
    current-ape ape-op-arg ! ; \ save the arg
: old parse-op parse-arg ;
\ refill is needed to pull in next line of input
\ skips 'if (true|false): throw to monkey' and gets number
: parse-if refill assert( 0<> ) 5 0 do drop-name loop parse-number ;
: test: drop-name drop-name parse-number current-ape ape-mod !
    parse-if current-ape ape-true ! parse-if current-ape ape-false ! ;

( ==================== Run one ape ==================== )
include 11-example
0 to (current-ape)

: run-op ( item -- updated )
    current-ape ape-op-arg @ current-ape ape-op @ execute 3 / ;
: update-item @ run-op dup rot ! ;
: ape-test current-ape ape-mod @ mod 0= ;
: inspect ( item-addr -- ) update-item \ ( updated )
    dup ape-test if cape-true else cape-false endif ( updated dest-ape )
    ape add-item ;

: monkey-do current-ape next-item current-ape ape-items +do
    inspect cell +loop remove-items ;