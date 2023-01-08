marker (reload)
: reload (reload) clearstack "13-task.fs" included ;

( ==================== Current attempt ==================== )
\ Currently planning to try and parse the whole list at once, and
\ compare the parsed values. Will have 'item' type that contains either
\ a number or a list, where the list is just a single-linked list.

( ==================== Item data and linked list ==================== )
0 constant type-list
1 constant type-num
struct
    cell% field item-type
    cell% field item-data \ either the number or a pointer to the list cells
end-struct item%

struct
    cell% field node-next
    cell% field node-data
end-struct node%

: %callocate ( align size -- addr ) dup >r %allocate throw dup r> erase ;

: list-tail ( node -- addr )
    begin dup node-next @ 0<> while node-next @ repeat ;

: list-new-node ( data -- addr ) node% %callocate dup -rot node-data ! ;
: list-append ( data list-head -- )
    \ ." Appending item: " over .item
    list-tail swap ( tail data )
    list-new-node ( tail new-node )
    swap node-next ! ;

( ==================== Parsing items ==================== )

: advance ( c-addr u -- c-addr' u' ) 1- swap 1+ swap ;
: skip-comma ( c-addr u -- c-addr' u' )
    over c@ [char] , = if advance endif ;

: number-item ( n -- item )
    item% %callocate type-num over item-type !
    dup -rot item-data ! ;

: list-item-append ( new-item list-item -- )
    \ ." list-item-append: " over .item ." item data: " .s cr cr \ dup item-data ? cr cr
    dup item-data @ 0= if swap list-new-node swap item-data !
    else item-data @ list-append endif ;

defer parse-item
\ : add-new-item postpone r@ postpone postpone list-item-append ; immediate
: parse-list ( c-addr u -- c-addr' u' list )
    \ ." parse-list: " 2dup type cr
    item% %callocate >r
    type-list r@ item-type !
    advance over c@ [char] ] <> if
        begin parse-item 0= while r@ list-item-append repeat
        r@ list-item-append
    endif advance skip-comma r> ;

: parse-number ( c-addr u -- c-addr' u' number-item )
    #0. 2swap >number 2swap d>s number-item -rot skip-comma rot ;

: end-of-list? ( c-addr u -- c-addr u f )
    over c@ [char] ] = over 0= or ;
: (parse-item) ( c-addr u -- c-addr2 u2 item end-of-list? )
    \ ." Parse item: " 2dup type cr .s cr
    over c@ [char] [ = if parse-list
    else parse-number endif
    >r end-of-list? r> swap ;
' (parse-item) is parse-item

0 value (packet)
variable left-packet
variable right-packet

: left left-packet to (packet) ;
: right right-packet to (packet) ;
: packet (packet) @ ;

0 value fd-in
false value file-more?
defer input-file
: (example) "13-example" ; : example ['] (example) is input-file ;
: (real) "13-input" ; : real ['] (real) is input-file ;
example

\ Keeps track of index for scoring
variable pair#

: open input-file r/o open-file throw to fd-in true to file-more? ;
: close fd-in close-file throw 0 to fd-in false to file-more? ;

1024 constant #buffer
create buffer #buffer allot buffer #buffer erase
: line buffer #buffer erase buffer #buffer fd-in read-line throw to file-more? ;
: item ( -- ) line buffer swap parse-item 0= throw -rot 2drop (packet) ! ;
: pair ( -- ) left item right item 1 pair# +! line assert( 0= ) ;

: list? item-type @ type-list = ;

-1 constant done-wrong
0 constant continue
1 constant done-right

\ Compare fns are all ( left right -- f )
\   where f is -1 for wrong order, 1 for correct order, or 0 for continue
defer compare-items
: dbg-compare ( l r -- l r ) 2dup swap .item ."  & " .item ;
: list-length ( item -- u ) dup item-type @ type-num = if drop 1 else
        0 swap item-data @ begin dup 0<>
        while swap 1+ swap node-next @ repeat drop endif ;
: compare-numbers ( l r -- f ) \ ." compare-numbers " dbg-compare cr
    item-data @ swap item-data @ swap
    2dup = if 2drop 0 else \ If equal, continue
        < if done-right \ if left is smaller, we're done (correct order)
        else done-wrong endif endif ; \ if right is smaller, we're done (incorrect order)
: nth-item ( item n -- addr )
    over item-type @ type-num = if drop
    else swap item-data @ swap 0 +do node-next @ loop node-data @ endif ;
: lengths ( l r -- u1 u2 ) list-length swap list-length swap ;
: compare-lists ( l r -- f )
    \ ." compare-lists " dbg-compare cr
    2dup lengths min 0 +do
        2dup i nth-item swap i nth-item swap compare-items
        dup 0<> if -rot unloop 2drop exit else drop endif loop
    lengths 2dup < if 2drop done-right else = if continue else done-wrong endif endif ;

0 constant both-nums
1 constant left-list
2 constant right-list
3 constant both-lists
\ Was originally going to handle each mismatch combo separately
\ Turns out it was easier to have nth-item just return the number for any index
\   and loop over the lists by length
: categorize ( left right -- f ) list? right-list and swap list? left-list and + ;
: (compare-items) \ ." compare-items " dbg-compare cr
    2dup categorize both-nums = if compare-numbers else compare-lists endif ;
' (compare-items) is compare-items

: compare left packet right packet compare-items ;

variable score
: reset 0 pair# ! 0 score ! ;
: init reset open ;

: go cr init begin file-more? while
            pair compare 0>= if pair# @ score +! endif repeat
    ." Score: " score ? cr ;

( ---------------------------------------- )
\ For part 2, I just put all of the packets into a list, sorting as I insert.
\ Then I insert the markers, and run through the sorted list until I find
\ them, and multiply their index by the score

variable sorted-items 0 sorted-items !

0 value previous
: add-sorted ( item -- )
    >r sorted-items @ 0= if
        \ First item in the list, jus add it
        r> list-new-node sorted-items !
    else
        0 to previous
        sorted-items @ begin ( head )
            dup 0<>  ( head head0<> )
            dup if over node-data @ r@ compare-items 0> else false endif and
        while
                dup to previous node-next @
        repeat
        r> list-new-node >r r@ node-next !
        previous 0= if r> sorted-items ! else r> previous node-next ! endif
    endif ;

2variable markers
variable index
: is-marker? ( node-addr -- f )
    dup node-data @ markers @ over = swap
    markers cell + @ = or ;
: find-markers ( -- ) 1 index ! sorted-items @ begin dup 0<> while
            is-marker? if index @ score @ * score ! endif
            1 index +! node-next @ repeat drop ;
: parse-marker ( c-addr u -- item-addr ) parse-item drop -rot 2drop ;
: insert-markers
    s" [[2]]" parse-marker s" [[6]]" parse-marker
    2dup markers 2!
    add-sorted add-sorted ;
: init init 1 score ! ;
: part2 cr init begin file-more? while
            pair left packet add-sorted right packet add-sorted repeat
    insert-markers ;

( -------------------- Debug stuff -------------------- )

defer .item
: .number-item ( addr -- ) item-data @ . ;
: (.list-item) ( addr -- ) begin dup while dup node-data @ .item
            node-next @ dup 0<> if [char] , emit endif repeat drop ;
: .list-item ( addr -- ) item-data @ (.list-item) ;

: (.item) ( addr -- ) >r r@ item-type @
    case type-list of ." [" r> .list-item ." ]" endof
        type-num of r> .number-item endof
    drop endcase ;
' (.item) is .item

: .compare ." Pair: " pair# ? cr
    ." Left: " left packet .item cr
    ." Right: " right packet .item cr
    ." Correct: " dup 0>= if ." true" else ." false" endif cr ;

: .sorted-items
    sorted-items @ begin
        dup 0<> while
            dup node-data @ .item cr
            node-next @
    repeat drop ;

\ Memory cleanup, never ended up using
: free-list-item ( list-item -- )
    dup item-data @
    begin dup 0<>
    while dup node-data @ dup item-type @ case
                type-num of drop endof
                type-list of recurse endof
                0 endcase
            dup node-next @
            swap free throw repeat 2drop ;

