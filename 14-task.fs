marker (reload)
: reload (reload) clearstack "14-task.fs" included ;

: parse-number ( c-addr u -- c-addr' u' u )
    #0. 2swap >number 2swap d>s ;
\ Parses "xxx,yyy" into x and y coords
: parse-coords ( c-addr u -- c-addr' u' x y )
    parse-number -rot 1 /string parse-number -rot 2swap ;

0 value max-y
50 constant point-buffer#
create point-buffer point-buffer# 2 * cells allot
: point ( n -- addr ) 2 * cells point-buffer + ;
variable point# 0 point# !
: update-max-y ( y -- y ) max-y over max to max-y ;
\ Parse line of points and store into point-buffer
: parse-line ( c-addr u -- )
    0 point# ! begin dup 0> while
            parse-coords update-max-y point# @ 2 * cells point-buffer + 2!
            1 point# +! 4 /string repeat 2drop ;

0 value fd-in false value file-more?
: example s" 14-example" ;
: real s" 14-input" ;
: open r/o open-file throw to fd-in true to file-more? ;
: close fd-in close-file throw false to file-more? ;
create line-buffer 1024 allot
: load-line line-buffer 1024 fd-in read-line throw to file-more? line-buffer swap parse-line ;

: sample s" 498,4 -> 498,6 -> 496,6" ;

\ Board is from 483,16 -> 544, 164
500 constant board-size
-300 constant x-offset 0 constant y-offset
create board board-size dup * allot board board-size dup * erase
: assert-coords ( x y -- )
    y-offset + 0 board-size within
    swap x-offset + 0 board-size within
    and dup 0= if .s cr endif assert( 0<> ) ;
: board-addr ( x y -- board-addr )
    2dup assert-coords
    y-offset + board-size * swap x-offset + + board + ;

: x-coords ( x1 y1 x2 y2 -- x1 x2 ) drop swap drop ;
: y-coords ( x1 y1 x2 y2 -- y1 y2 ) swap drop rot drop ;
: horiz? ( x1 y1 x2 y2 -- f ) x-coords <> ;
: pivot ( u1 u2 -- umax umin ) 2dup < if swap endif ;
: .coord ( x y -- x y ) cr 2dup swap . . ;
: horiz-line ( x1 y1 x2 y2 -- )
    dup >r x-coords pivot swap 1+ swap +do
        true i j board-addr c! loop rdrop ;
: vert-line ( x1 y1 x2 y2 -- )
    over >r y-coords pivot swap 1+ swap +do
        true j i board-addr c! loop rdrop ;
: draw-line ( -- ) \ call after parse-line
    point# @ 1 +do i 1- point 2@ i point 2@
        2over 2over horiz? if
            horiz-line else vert-line endif loop ;
: draw-file
    begin file-more? while
            load-line draw-line repeat ;

variable sand-count
defer move
: (s) ( x y -- x y' ) 1+ ; : s ['] (s) is move ;
: (sw) ( x y -- x' y' ) 1+ swap 1 - swap ; : sw ['] (sw) is move ;
: (se) ( x y -- x' y' ) 1+ swap 1+ swap ; : se ['] (se) is move ;
: empty? ( x y -- f ) board-addr c@ 0= ;
: try-move ( x y -- x' y' f )
    2dup move 2dup empty? if ( x y x' y' )
        2swap 2drop true
    else 2drop false endif ;
\ Tries to move the sand at x,y one unit
\   Returns updated position, and a flag indicating whether it came to rest
\   If it comes to rest, the returned point is back at the origin
: turn ( x y -- x' y' f )
    s try-move if false exit endif
    sw try-move if false exit endif
    se try-move if false exit
    else board-addr true swap c!
        500 0 true endif ;

: reset 0 sand-count ! 0 to max-y board board-size dup * erase ;
: init ( -- 500 0 ) reset open draw-file 500 0 ;
: part1 init begin turn
        if 1 sand-count +! endif \ check if sand came to rest
    dup [ board-size 2 - ] literal >= until 2drop \ check for falling into abyss
    ." Total sand: " sand-count ? ;

: draw-floor x-offset negate max-y 2 + board-size x-offset negate 1- + over horiz-line ;
: part2 init draw-floor
    begin turn
        if 1 sand-count +! endif
    500 0 empty? 0= until 2drop
    ." Total sand: " sand-count ? ;

( ==================== debug helpers ==================== )
: .point ( addr -- ) 2@ swap . ." ," . ;
: .points ( -- ) cr point# @ 0 +do
        point-buffer i 2 * cells + .point cr loop ;

( ==================== input analysis ==================== )
\ I did this to avoid having a very sparse (but large) array for cells
2variable min-coords
2variable max-coords
: update-min ( x y -- )
    2dup 0 = swap 0 = or if ." Found 0 coord" .points cr endif
    min-coords 2@ rot ( x mx my y )
    min -rot min swap min-coords 2! ;
: update-max ( x y -- )
    max-coords 2@ rot ( x mx my y )
    max -rot max swap max-coords 2! ;
: analyze
    10000 10000 min-coords 2! 0 0 max-coords 2!
    open begin file-more? while load-line
            point# @ 0 +do
                point-buffer i 2 * cells + 2@
                2dup update-min update-max
            loop
    repeat ." Min: " min-coords .point ."  && Max: " max-coords .point cr ;

: time ( xt -- ) utime 2>r execute utime 2r> d- d>s 1000 / . ." ms" ;