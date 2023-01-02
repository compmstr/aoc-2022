marker (reload)
: reload (reload) clearstack "11-task.fs" included ;

\ TODO For some reason, after running, reload 'fails' twice then works
\      This has only happened since I switched to an xt for ape-op

: skip 1+ swap 1- swap ;
: parse-number parse-name s>number? assert( 0<> ) d>s ;
: drop-name parse-name 2drop ;
: *! ( n addr -- ) dup @ rot * swap ! ;
: pivot 2dup < if swap endif rot 2dup < if swap endif drop ;

( ==================== Data storage ==================== )

struct
    cell% 40 * field ape-items
    cell% field ape-op
    cell% field ape-mod
    cell% field ape-true
    cell% field ape-false
    cell% field ape-inspect-count
end-struct ape%
ape% 8 * %allot here constant end-apes constant apes
: reset-apes apes end-apes apes - erase ; reset-apes

0 value (current-ape)
: ape apes swap ape% %size * + ;
: current-ape (current-ape) ape ;
: cape-true current-ape ape-true @ ;
: cape-false current-ape ape-false @ ;

: next-item ( ape-addr -- addr ) ape-items dup 40 cells + swap +do
    i @ 0= if i leave endif cell +loop ;
: add-item ( n ape-addr -- ) next-item ! ;
: last-item ( appe-addr -- addr ) >r r@ next-item cell - dup assert( r> ape-items >= ) ;
: remove-items ( ape-addr -- ) ape-items 40 cells erase ;

\ Note: XT needs to not add things to the stack
: ape-items-loop ( xt ape-addr -- ) dup next-item swap ape-items +do
    i over execute cell +loop drop ;

( ==================== Input file parsing ==================== )

variable lcm 1 lcm !
0 value max-ape
: ape-loop ( xt -- ) max-ape 1+ 0 do i to (current-ape) dup execute loop drop ;
: monkey [char] : parse s>number? assert( 0<> )
    d>s dup to (current-ape) max-ape max to max-ape drop-name ;
: starting noop ;
: parse-item ( "number" -- n f ) #0. parse-name >number 0<> swap drop -rot d>s swap ;
: items: begin parse-item while current-ape add-item repeat current-ape add-item ;
: operation: drop-name drop-name ;
: (op-arg) parse-name 2dup "old" compare 0= ;

\ This was fun, turns 'old <op> (old|<number>)' into a noname xt that gets stored as ape-op
: num-arg #0. 2swap >number 2drop d>s ;
: op-arg (op-arg) if 2drop ['] dup compile, else num-arg postpone literal endif ;
: old noname :
    parse-name find-name      \ get op
    op-arg compile, postpone ; \ get and compile arg, finish noname
    latestxt current-ape ape-op ! ; \ store the noname to ape-op

\ refill is needed to pull in next line of input for parsing
\ skips 'if (true|false): throw to monkey' and gets number
: parse-if refill assert( 0<> ) 5 0 do drop-name loop parse-number ;
: update-lcm lcm @ over * lcm ! ;
: test: drop-name drop-name parse-number update-lcm current-ape ape-mod !
    parse-if current-ape ape-true ! parse-if current-ape ape-false ! ;

( ==================== Do monkey business ==================== )

defer lower-worry
: lower-worry1 3 / ;
: lower-worry2 lcm @ mod ;
' lower-worry1 is lower-worry

: run-op ( item -- updated ) current-ape ape-op @ execute lower-worry ;
: update-item dup @ run-op dup rot ! ;
: ape-test current-ape ape-mod @ mod 0= ;
: monkey-see 1 current-ape ape-inspect-count +! ;
: inspect ( item-addr -- ) monkey-see update-item \ ( updated )
    dup ape-test if cape-true else cape-false endif ( updated dest-ape )
    ape add-item ;

: monkey-do ['] inspect current-ape ape-items-loop current-ape remove-items ;
: round ['] monkey-do ape-loop ;
: rounds 0 do round loop ;

( ==================== Part 1 ==================== )

: (inspect-stats) ." Ape " (current-ape) . ." inspected items " current-ape ape-inspect-count @ . ." times." cr ;
: inspect-stats ['] (inspect-stats) ape-loop ;

: monkey-business 0 0 max-ape 1+ 0 do i ape ape-inspect-count @ pivot loop * ;
20 value rounds#
: part1 20 to rounds# ['] lower-worry1 is lower-worry ;
: part2 10000 to rounds# ['] lower-worry2 is lower-worry ;

: report monkey-business ." Monkey Business: " . cr inspect-stats ;

: reset-lcm 1 lcm ! ;
: reset reset-apes 0 to max-ape 0 to (current-ape) reset-lcm ;
defer input-file
: (real) "11-input" ; : real ['] (real) is input-file ;
: (example) "11-example" ; : example ['] (example) is input-file ;

: go reset input-file included rounds# rounds report ;
real part1

( ==================== helpers ==================== )
: @. @ . ;
: .ape-items ['] @. swap ape ape-items-loop cr ;
: .cape-items (current-ape) dup . ." => " .ape-items ;
: .apes-items ['] .cape-items ape-loop ;
: .ape ape ape% %size dump ;
: .cape (current-ape) .ape ;