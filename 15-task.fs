marker (reload)
: reload (reload) clearstack "15-task.fs" included ;

0 value fd-in
false value file-more?
defer input-file
: (example) s" 15-example" ; : example ['] (example) is input-file ;

: open input-file r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;

require llist.fs

struct
    cell% 2 * field sensor-coords
    cell% 2 * field beacon-coords
end-struct sensor%

: parse-number ( c-addr u -- c-addr' u' n )
    over c@ [char] - = if 1 /string -1 else 1 endif >r
    #0. 2swap >number 2swap d>s r> * ;

: next-num ( c-addr u -- c-addr' u' ) [char] = scan 1 /string ;
: line-coords ( c-addr u -- sensor-x sensor-y beacon-x beacon-y )
    4 0 do next-num parse-number -rot loop 2drop ;
: parse-line ( c-addr u -- addr )
    line-coords sensor% %allocate throw >r
    r@ beacon-coords 2!
    r@ sensor-coords 2!
    r> ;

variable sensors 0 sensors !
1024 constant line-buffer#
create line-buffer line-buffer# allot
: for-each-sensor-node ( xt -- )
    sensors @ begin dup 0<> while
        dup node-next @ >r
        over execute r>
    repeat 2drop ;
: .coords ( x y -- ) swap 1 .r [char] , emit 1 .r ;
: .sensor ( addr -- )
    >r ." Sensor at: " r@ sensor-coords 2@ .coords
    ."  -- Beacon at: " r> beacon-coords 2@ .coords cr ;
: .sensor-node ( addr -- ) node-data @ .sensor ;
: .sensors ( -- ) ['] .sensor-node for-each-sensor-node ;
: free-sensor ( addr -- )
    dup node-data @ free throw
    free throw ;
: free-sensors ( -- )
    ['] free-sensor for-each-sensor-node
    0 sensors ! ;
: sensors-append ( data -- )
    sensors @ 0= if list-new-node sensors !
    else sensors @ list-append endif ;
: load-sensors ( -- )
    free-sensors open
    begin line-buffer line-buffer# fd-in read-line throw while
            line-buffer swap parse-line sensors-append repeat drop close ;

: coord- ( x1 y1 x2 y2 -- x1-x2 y1-y2 ) rot swap - -rot - swap ;
: man-dist ( x1 y1 x2 y2 -- u ) coord- abs swap abs + ;
: sensor-man-dist ( addr -- u )
    dup beacon-coords 2@ rot sensor-coords 2@ man-dist ;

: sample-line s" Sensor at x=2, y=18: closest beacon is at x=-2, y=15" ;