marker (reload)
: reload (reload) clearstack "12-task.fs" included ;

( ==================== Map data in one cell ==================== )
\ 0xFHHXXXXYYYY => F - flags | H - height | X - x coord | Y - y coord
: [comp!] >r invert and swap r> lshift or ;

0 value comp-mask
0 value comp-offset
: x 0xFFFF to comp-mask [ 2 8 * ] literal to comp-offset ;
: y 0xFFFF to comp-mask 0 to comp-offset ;
: from-x 0xFFFF to comp-mask [ 6 8 * ] literal to comp-offset ;
: from-y 0xFFFF to comp-mask [ 4 8 * ] literal to comp-offset ;
: distance 0xFFFF to comp-mask [ 4 8 * ] literal to comp-offset ;
: height 0xFF to comp-mask [ 6 8 * ] literal to comp-offset ;
: target? 0x1 to comp-mask [ 7 8 * ] literal to comp-offset ;
: start? 0x1 to comp-mask [ 7 8 * 1 + ] literal to comp-offset ;

: comp-erase-mask comp-mask comp-offset lshift invert ;
: shift-value comp-mask and comp-offset lshift ;
: comp-set ( val n -- ) comp-erase-mask and swap shift-value or ;
: comp! ( val comp -- ) dup >r @ comp-set r> ! ;
: comp comp-offset rshift comp-mask and ;
: comp@ @ comp ;

( ==================== Parsing ==================== )

256 constant line-buffer#
create line-buffer line-buffer# allot line-buffer line-buffer# erase
create map-buffer 6000 cells allot map-buffer 6000 cells erase
0 value fd-in
defer input-file
: (real) "12-input" ; : real ['] (real) is input-file ;
: (example) "12-example" ; : example ['] (example) is input-file ;

: open input-file r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;
0 value map-width 0 value map-height
variable line
0 value map-buffer-entry
variable start-cell 0 start-cell !
variable target-cell 0 target-cell !
: map-buffer-advance map-buffer-entry cell + to map-buffer-entry ;
: flags case
        'E' of 1 map-buffer-entry target? comp!
            map-buffer-entry target-cell ! 'z' endof
        'S' of 1 map-buffer-entry start? comp!
            map-buffer-entry start-cell ! 'a' endof
        dup endcase ;
: x! map-buffer-entry x comp! ;
: y! map-buffer-entry y comp! ;
: height! map-buffer-entry height comp! ;
: save-char ( idx -- ) line-buffer over + c@ flags height! x! line @ y! ;
: save ( line-len -- ) dup to map-width 0 do i save-char map-buffer-advance loop ;
: read-buffer-line line-buffer line-buffer# fd-in read-line throw ;

: reset 0 line ! 0 start-cell ! 0 to map-height
    0 to map-width map-buffer to map-buffer-entry ;
: read reset open begin read-buffer-line
    while save 1 line +! repeat drop close line @ to map-height ;

: cell>coords ( cell -- x y ) dup x comp swap y comp ;
: in-map? ( x y -- f ) dup 0>= swap map-height < and
    swap dup 0>= swap map-width < and and ;
: coords>offset ( x y -- offset ) map-width * + cells ;
: offset>coords ( offset -- x y ) map-width /mod ;
: buffer-lookup ( addr x y -- content | 0 )
    2dup in-map? if coords>offset + @ else drop 2drop 0 endif ;
: map-store ( cell -- ) dup cell>coords coords>offset map-buffer + ! ;
: coords>cell ( x y -- map-cell | 0 ) map-buffer -rot buffer-lookup  ;

( ==================== Debug tools ==================== )

: .yn 0<> if ." Y" else ." N" endif ;
: .map-cell
    dup >r x comp . r@ y comp . ." = " r@ height comp emit space
    ." D: " r@ distance comp . space
    ." E:" r@ target? comp .yn space ." S:" r@ start? comp .yn cr rdrop ;
: .map-cell@ @ .map-cell ;

: .map-line ( n -- ) cr map-width * cells map-buffer + map-width 0
    do dup i cells + @ .map-cell loop drop ;

( ==================== Navigation ==================== )

: 2add ( x1 y1 x2 y2 -- x' y' ) rot + -rot + swap ;
\ All directions are ( cell -- cell cell' )
: direction rot dup >r cell>coords 2add coords>cell r> swap ;
: up 0 -1 direction ; : down 0 1 direction ;
: left -1 0 direction ; : right 1 0 direction ;

create move-stack 6000 cells allot move-stack 6000 cells erase
move-stack value move-tos
: move-stack? move-tos move-stack - 0> ;
: (push-move) ( x1 y1 x2 y2 -- )
    0 y comp-set x comp-set from-y comp-set from-x comp-set move-tos !
    move-tos cell + to move-tos ;
: push-move ( source target -- ) swap cell>coords rot cell>coords (push-move) ;
: (pop-move) ( -- x1 y1 x2 y2 )
    assert( move-stack? ) move-tos cell - to move-tos move-tos
    dup >r from-x comp@ r@ from-y comp@ r@ x comp@ r> y comp@
    move-tos cell erase ;
: pop-move ( -- source target )
    (pop-move) 2swap coords>cell -rot coords>cell ;

: climbable? ( source target -- f ) height comp swap comp 1+ <= ;
: faster? ( source target -- f ) distance comp dup 0= if 2drop true
    else swap comp 1+ > endif ;
: valid-move? 2dup climbable? -rot 2dup faster? -rot 2swap and ;
: add-move ( source target -- )
    dup 0= if 2drop exit endif
    valid-move? if push-move else 2drop endif ;
: moves ( source --) dup >r up add-move r@ down add-move
    r@ left add-move r> right add-move ;
: inc-dist ( source target -- )
    swap distance comp 1+ swap distance comp-set dup map-store ;
: do-move ( source target -- )
    \ dup .map-cell
    valid-move? if inc-dist moves
    else 2drop endif ;

: .move-stack ." Move-stack:" cr move-stack begin dup @ 0<> while
            2 spaces dup from-x comp@ . dup from-y comp@ . ." => "
            dup x comp@ . dup y comp@ . cr cell + repeat drop ;

( ==================== Running ==================== )

: initial-cell 'a' 0 height comp-set ;
defer init

: result target-cell @ @ distance comp 1- ." Shortest path: " . cr ;
: go read init begin move-stack? while pop-move do-move repeat result ;

: part1-init initial-cell start-cell @ @ do-move ;
: part1 ['] part1-init is init go ;

\ Have the start go from each 'a' point, and find the shortest
: low-cell? height comp [char] a = ;
: mark-start initial-cell swap do-move ;
: part2-init map-width map-height * 0 do
        map-buffer i cells + @ dup low-cell?
        if mark-start else drop endif loop ;
: part2 ['] part2-init is init go ;

( ==================== Navigation first attempt ==================== )
\ This ended up just running in circles, and seems too complicated

\ struct
\     cell% field path-cell
\     cell% field path-prev
\     cell% field path-length
\     cell% 4 * field path-candidates
\ end-struct path-node%
\ create pnode-buffer 6000 cells allot pnode-buffer 6000 cells erase
\ : .pnode cr ." Path node: " dup path-cell .map-cell@ ." - length: " dup path-length @ . cr
\     ." Candidates: " cr path-candidates begin dup @ 0<>
\     while 2 spaces dup .map-cell@ cell + repeat drop ;

\ : seen? ( node target -- f )
\     >r path-prev @ begin dup 0<> while
\             dup path-cell @ r@ = if drop rdrop true exit endif
\             path-prev @ repeat rdrop ;
\ : next-candidate ( node -- addr )
\     path-candidates begin dup @ 0<> while cell + repeat ;
\ : terrain? ( node target -- f ) height comp swap path-cell comp@ 1+ <= ;
\ : at-target? ( node target -- f ) drop path-cell target? comp@ ;
\ : valid-candidate ( node target -- f ) 2dup seen?
\     0= -rot 2dup terrain? -rot at-target? 0= and and ;
\ : add-candidate ( node x y -- ) map-buffer-lookup dup 0<> if ( node target -- )
\         2dup valid-candidate if swap next-candidate ! else 2drop endif
\     else 2drop endif
\ ;
\ : node-coords ( node -- x y ) path-cell @ dup x comp swap y comp ;
\ : up dup node-coords 1- ; : down dup node-coords 1+ ;
\ : left dup node-coords swap 1- swap ; : right dup node-coords swap 1+ swap ;
\ : add-candidates ( node -- )
\     dup up add-candidate dup down add-candidate
\     dup left add-candidate dup right add-candidate ;
\ : length! ( parent node -- )
\     swap dup 0<> if path-length @ 1+ else drop 0 endif swap path-length ! ;
\ : store-node ( node -- ) dup node-coords buffer-offset pnode-buffer + ! ;
\ : pnode-lookup ( x y -- addr ) pnode-buffer -rot buffer-lookup ;
\ : node->stored node-coords pnode-lookup ;
\ : shorter-path? path-length @ over path-length @ < ;
\ \ Remove candidate moves from less efficient move in same cell
\ : clear-old ( old-node -- ) free throw ;
\ : add-to-buffer ( node -- ) dup >r node->stored dup 0<> if
\         r@ shorter-path? if clear-old r> store-node exit endif
\     else drop r> store-node exit endif drop rdrop ;
\ : make-node ( parent cell -- node )
\     @ path-node% %allocate throw dup >r path-node% %size erase
\     r@ path-cell ! dup r@ length! r@ path-prev ! r@ add-candidates
\     r@ add-to-buffer r> ;
\ : start-node 0 start-cell @ make-node ;

\ \ create start node, then for each candidate node, recurse to realize moves
\ : (navigate) ( node -- )
\     \ cr ." (navigate) " .s dup path-cell .map-cell@
\     dup path-candidates dup 4 cells + swap +do
\     i @ 0<> if dup i make-node recurse endif cell +loop 2drop ;
\ : navigate start-node (navigate) ;

\ : part1 target-cell @ @ map-cell-coords pnode-lookup .pnode ;