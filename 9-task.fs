marker (reload)
: reload (reload) "9-task.fs" included ;

: parse-number #0. bl parse >number 2drop d>s ;

: point create here 2 cells allot 2 cells erase ;
: point- ( x1 y1 x2 y2 -- x y ) rot swap - -rot - swap ;
: x ;
: y 1 cells + ;
: clear-point 2 cells erase ;

( ========== Unique point tracking ========== )
7000 constant points#
create points points# 2 * cells allot
variable points-stored 0 points-stored !
: points-top points-stored @ 2 * cells points + ;

false value found?
: add-point
    false to found?
    points-top points +do
        i 2@ 2over d= if true to found? leave endif
    2 cells +loop
    found? 0= if points-top 2! 1 points-stored +! else 2drop endif
    points-stored @ points# assert( < ) ; \ Make sure we're not full

( ========== Rope links ========== )
\ The links of the rope, each one should follow the previous one
10 value link#
create links link# 2 * cells allot
: link ( idx -- addr ) 2 * cells links + ;
\ Head and tail are just the start and end of the links
0 link value head
link# 1- link value tail
: clear-links links link# 2 * cells erase ;

: add-tail tail 2@ add-point ;

( ========== Movement ========== )
( 0 - up, 1 - right, 2 - down, 3 - left )
0 value direction

variable move-count 0 move-count !
: move-up 1 swap y +! ;
: move-right 1 swap x +! ;
: move-down -1 swap y +! ;
: move-left -1 swap x +! ;
create (move) ' move-up , ' move-right , ' move-down , ' move-left ,
: move head (move) direction cells + @ execute 1 move-count +! ;

( ========== Following ========== )
\ Following is required if either diff component is >1 (abs)
\ Movement is clamped to 1 per 'turn', in the direction of the diff
0 value link-a
0 value link-b
point link-diff
: diff link-a 2@ link-b 2@ point- link-diff 2! ;
: must-move? link-diff 2@ abs 1 > swap abs 1 > or ;
: follow-clamp -1 max 1 min ;
: follow-move
    link-diff x @ follow-clamp link-b x +!
    link-diff y @ follow-clamp link-b y +!
;
: (follow) diff must-move? if follow-move add-tail endif ;
: follow link# 1- 0 do i link to link-a i 1+ link to link-b (follow) loop ;

( ========== Commands to parse from the input =================== )
: (dir) to direction parse-number 0 do move follow loop ;
: u 0 (dir) ;
: r 1 (dir) ;
: d 2 (dir) ;
: l 3 (dir) ;

: reset clear-links 0 move-count +! ;
: init reset add-tail ;

: go init "9-input" included ." Unique positions: " points-stored @ . cr ." Moves: " move-count @ . ;

( ==================== Test stuff ==================== )

: .points points-top points +do i 2@ . . cr 2 cells +loop ;

\ example bounds are -389,-99 -> 1,172
point lo-bounds point hi-bounds
: update-bounds
    dup x @ lo-bounds x @ min lo-bounds x !
    dup y @ lo-bounds y @ min lo-bounds y !
    dup x @ hi-bounds x @ max hi-bounds x !
    dup y @ hi-bounds y @ max hi-bounds y !
    drop
;
: .bounds hi-bounds 2@ lo-bounds 2@ ." lo: " . . ." -- hi: " . . cr ;
