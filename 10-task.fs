marker (reload)
: reload (reload) "10-task.fs" included ;

( ==================== Interpreter ==================== )

: parse-number bl parse s>number? assert( 0<> ) d>s ;
variable cycle
variable x
0 value on-cycle

: cycle+ 1 cycle +! on-cycle ?dup-if execute endif ;

: noop cycle+ ;
: addx parse-number cycle+ cycle+ x +! ;

( ==================== Part 1 ==================== )
variable signal-sum
create breaks 20 c, 60 c, 100 c, 140 c, 180 c, 220 c,
here value breaks-end
: break cycle @ x @ * signal-sum +! ;
: (part1) breaks-end breaks do cycle @ i c@ = if break unloop exit endif loop ;
: part1 ['] (part1) to on-cycle ;
: part1-result ." Signal sum: " signal-sum ? ;

( ==================== Part 2 ==================== )
: draw? cycle @ 1- 40 mod x @ - abs 1 <= ;
: draw draw? if [char] # emit else [char] . emit endif ;
: newline cycle @ 40 mod 0= if cr endif ;
: (part2) draw newline ;
: part2 ['] (part2) to on-cycle ;

( ==================== Main ==================== )
: (both) (part1) (part2) ;
: both ['] (both) to on-cycle ;

: reset 0 cycle ! 1 x ! 0 signal-sum ! ;
: go both reset "10-input" included cr part1-result ;

( ==================== Debugging ==================== )
: .dbg ." Cycle: " cycle ? ." -- x: " x ?  ." -- draw: " draw? . cr ;
