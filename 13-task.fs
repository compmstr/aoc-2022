marker (reload)
: reload (reload) clearstack "13-task.fs" included ;

( ==================== Current attempt ==================== )
\ Currently planning to try and parse the whole list at once, and
\ compare the parsed values. Will have 'item' type that contains either
\ a number or a list, where the list is just a single-linked list.

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

: %callocate ( align size -- addr ) dup >r %allocate throw dup r> erase ;

: sample s" [10,2,[7,8],3,[[]],4]" pad swap dup >r move pad r> ;

: list-tail ( node -- addr )
    begin dup node-next @ 0<> while node-next @ repeat ;

: list-new-node ( data -- addr ) node% %callocate dup -rot node-data ! ;
: list-append ( data list-head -- )
    \ ." Appending item: " over .item
    list-tail swap ( tail data )
    list-new-node ( tail new-node )
    swap node-next ! ;

: advance ( c-addr u -- c-addr' u' ) 1- swap 1+ swap ;
: skip-comma ( c-addr u -- c-addr' u' )
    over c@ [char] , = if advance endif ;

: number-item ( n -- item )
    item% %callocate type-num over item-type !
    dup -rot item-data ! ;

: list-item-append ( new-item list-item -- )
    ." list-item-append: " over .item ." item data: " .s cr cr \ dup item-data ? cr cr
    dup item-data @ 0= if swap list-new-node swap item-data !
    else item-data @ list-append endif ;

defer parse-item
\ : add-new-item postpone r@ postpone postpone list-item-append ; immediate
: parse-list ( c-addr u -- c-addr' u' list )
    ." parse-list: " 2dup type cr
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
: (parse-item) ( c-addr u -- c-addr2 u2 item end-of-list )
    ." Parse item: " 2dup type cr .s cr
    over c@ [char] [ = if parse-list
    else parse-number endif
    >r end-of-list? r> swap ;
' (parse-item) is parse-item

\ Status -- parsing of the list appears to be done
\    Now just need to load from file and compare

( ==================== Initial attempt ==================== )

0 constant list 1 constant atom
1024 constant buffer#
struct
    cell% field packet-length
    cell% field packet-offset
    cell% field packet-flags
    char% buffer# * field packet-contents
end-struct packet%

create left-packet packet% %allot packet% %size erase
create right-packet packet% %allot packet% %size erase
left-packet value packet

: left left-packet to packet ;
: right right-packet to packet ;
: length packet packet-length ;
: offset packet packet-offset ;
: contents packet packet-contents ;
: contents-str contents offset @ + length @ offset @ - ;
: single-list? packet packet-flags 1 and 0<> ;
: single-list! packet packet-flags @ 1 1 lshift and + packet packet-flags ! ;

0 value fd-in
false value file-more?
defer input-file
: (example) "13-example" ; : example ['] (example) is input-file ;
: (real) "13-input" ; : real ['] (real) is input-file ;
example

variable pair#
: reset 0 pair# ! ;

: open input-file r/o open-file throw to fd-in true to file-more? ;
: close fd-in close-file throw 0 to fd-in false to file-more? ;

: line-more? offset @ length < ;
: next-char contents offset @ + c@ ;
: advance-char ( n -- ) offset dup >r @ + length @ min r> ! ;
: line contents buffer# fd-in read-line throw to file-more? ;
: item line length ! 0 offset ! ;
: pair ( -- ) left item right item 1 pair# +! line assert( 0= ) ;

: list? ( -- f ) next-char [char] [ = ;
: digit? ( -- f ) next-char dup [char] 0 >= swap [char] 0 <= and ;
: list-end? ( -- f ) next-char single-list? if [char] , else [char] ] endif = ;
: consume-list ( -- ) begin list-end? 0= while 1 advance repeat 1 advance ;

: parse-number #0. contents-str >number length @ swap - offset ! drop d>s ;

: init reset open ;

: compare-numbers  ;
: compare-mixed ( TODO ) ;
: compare-lists ( TODO ) ;
: categorize left list? 1 and right list? 2 and + ;
: compare categorize case
        0 of compare-numbers endof
        1 of true left single-list! compare-lists endof
        2 of true right single-list! compare-lists endof
        3 of compare-lists endof
        0 endcase ;

\ TODO I may want to try to parse a whole list at a time
\      I could compare element counts