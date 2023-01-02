marker (reload)

: reload (reload) "7-task.fs" included ;

struct
    cell% field node-next
    cell% field node-data
end-struct node%

struct
    cell% field fnode-parent
    cell% field fnode-size
    cell% field fnode-dir?
    cell% field fnode-name
    cell% field fnode-children
end-struct fnode%

: callocate-struct ( align size -- addr )
    2dup %allocate throw >r
    %size r@ swap erase
    r>
;

: cons ( car cdr -- node )
    node% callocate-struct
    dup >r node-next !
    r@ node-data !
    r>
;

: foreach ( xt list -- )
    begin
        dup
    while
            2dup node-data @ swap execute
            node-next @
    repeat
    2drop
;

: inc-pad
    1 pad +!
    drop
;

: list-len
    0 pad !
    ['] inc-pad swap foreach
    pad @
;

: p-node
    ." Node: " . cr
;

: p-list
    ['] p-node swap foreach
;

variable current-dir
0 current-dir !
variable root-dir
0 root-dir !

: indent
    spaces
;

: print-size
    dup fnode-size @ 10 .r space
;

: print-name
    dup fnode-name $@ type
;

: print-dir?
    dup fnode-dir? @ if
        [char] / emit
    endif
;

: dump-fnode ( fnode indent -- )
    dup 2 + >r
    swap print-size swap indent print-name print-dir? cr
    fnode-children @
    begin
        dup
    while
            dup node-data @ r@ recurse
            node-next @
    repeat
    rdrop
    drop
;

: dcd current-dir @ 0 dump-fnode ;
: drd root-dir @ 0 dump-fnode ;

: ..
    current-dir @ fnode-parent @ dup 0= if
        ." Already at top directory!" cr
        drop
    else
        current-dir !
    endif
;

variable fnodes
0 fnodes !

: save-fnode
    dup fnodes @ cons fnodes !
;

: create-fnode ( c-addr u parent -- addr )
    \ cr ." create-fnode: " .s cr
    fnode% callocate-struct >r
    r@ fnode-parent !
    r@ fnode-name $!
    r> save-fnode
;

: add-child ( fnode -- )
    >r
    current-dir @ fnode-children dup @ 0= if
        \ ." first child" cr
        ( cd-children )
        0 r> swap cons ( cd-children new-list )
    else
        \ ." Not first child" cr
        \ next child
        r> over @ cons \ ( cd-children new-node )
    endif ( cd-children new-list-node )
    swap !
;

: mkdir ( c-addr u -- addr )
    current-dir @ create-fnode >r
    true r@ fnode-dir? !
    current-dir @ 0= if
        r>
    else
        r@ add-child
        r>
    endif
;

: switch-to-mkdir ( c-addr u -- ) \ add child to CURRENT-DIR and switch to new child
    mkdir current-dir !
;

create <child-name> 256 chars allot
<child-name> 256 chars erase
variable found?
: check-name ( fnode-node -- fnode-node f ) \ returns true if name matches
    dup node-data @ fnode-name $@ <child-name> $@ compare 0=
;

: current-exists?
    current-dir @ 0= if
        \ ." No current dir" cr
        <child-name> $free
        rdrop \ exit parent function
        exit
    endif
;

: current-has-children? ( -- children | exits )
    current-dir @ fnode-children @ dup 0= if
        \ ." Current dir has no children" cr
        <child-name> $free
        rdrop \ exit parent function
        exit
    endif
;

: child-dir ( c-addr u -- addr | 0 )
    false found? !
    <child-name> $!
    current-exists?
    current-has-children?
    begin
        dup
        found? @ invert and
    while
            check-name if
                true found? !
            else
                node-next @
            endif
    repeat
    \ ." Found child? " found? ? cr
    found? @ if
        node-data @
    else
        drop 0
    endif
;

: sub-dir
    2dup child-dir dup if
        \ c-addr u child-dir
        -rot 2drop \ child-dir
        current-dir !
    else
        drop
        switch-to-mkdir
    endif
;

: (cd)
    2dup ".." compare 0= if
        2drop ..
    else
        current-dir @ 0= if
            switch-to-mkdir
            current-dir @ root-dir !
        else
            sub-dir
        endif
    endif
;

: cd
    parse-name (cd)
;

: update-parent-sizes ( size fnode -- )
    fnode-parent @
    begin
        dup
    while
            2dup fnode-size +!
            fnode-parent @
    repeat
    2drop
;

: new-file ( size c-addr u -- )
    current-dir @ create-fnode ( size fnode )
    dup add-child
    2dup fnode-size ! tuck ( fnode size fnode )
    update-parent-sizes ( fnode )
    drop
;

: w? ( c -- f ) 32 <= ;
: advance-n >r r@ - swap r> + swap ; \ advance a string by N characters
: advance 1 advance-n ; \ advance a string by one character
: skip ( c-addr u -- c-addr u )
    begin
        over c@ w?
        over 0<> and
    while
            advance
    repeat
;

: handle-file
    0. 2swap >number 2swap d>s
    -rot new-file
;

: ls ; \ don't really need to do anything for ls

: cmd
    2 advance-n \ drop "$ "
    evaluate
;

: handle
    over c@
    case
        dup [char] $ = ?of drop cmd endof
        dup [char] 0 >= over [char] 9 <= and ?of
            drop handle-file
        endof
        drop 2dup "dir " string-prefix? ?of
            4 advance-n mkdir drop
        endof
    endcase
;

0 value fd-in
: open "7-input" r/o open-file throw to fd-in ;

: read
    open
    begin
        pad 84 fd-in read-line throw
    while
            pad swap
            \ 2dup ." Handling: " type cr
            handle
    repeat
    drop fd-in close-file throw
    0 to fd-in
;

\ TODO use foreach on fnodes @ to get total needed for task

: test-setup
    "/" (cd)
    "foo" (cd)
    1024 "7-task.fs" new-file
    42 "shera-rocks" new-file
    "bar" (cd)
    32 "another" new-file
    ".." (cd)
    ".." (cd)
    512 ".test.txt" new-file
    "baz" (cd)
    78 "haha.el" new-file
    ".." (cd)
    cr dcd
;