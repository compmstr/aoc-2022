marker (reload)
: reload (reload) "3-task.fs" included ;

: split ( c-addr u -- c-addr2 u2 c-addr3 u3 )
    2 / 2dup + over
;

: find-dup ( c-addr u c-addr2 u2 -- c )
    over + swap ?do \ c-addr u
        \ i c@ emit 3 spaces .s cr
        2dup over + swap ?do
            i c@ j c@
            \ 6 spaces 2dup emit emit cr
            = if
                \ 4 spaces .s cr
                2drop i c@
                unloop unloop exit
            endif
        loop
    loop
;

: priority ( c -- priority )
    dup 'Z' > if
        \ Lowercase
        'a' - 1+
    else
        'A' - 27 +
    endif
;

1024 constant common-buffer#
variable common common-buffer# allot
variable #common
0 #common !

variable current-line
variable current

variable found

: common-string common #common @ ;
: .common common-string type cr ;

: common-char ( -- c )
    [char] !
    common-string + common ?do
        i c@ dup 0<> if
            swap drop
            leave
        else
            drop
        endif
    loop
;

: set-common ( u -- )
    \ ." common-char " common-char emit cr
    \ ." set-common " .s cr
    pad over common swap cmove
    #common !
    \ ." Set common to: " .common
;

: update-common ( u -- )
    \ ." update-common " .s cr
    common-string + common ?do
        false found !
        i c@ current c!
        \ ." Checking against common: " i c@ emit cr
        pad over + pad ?do
            \ ."   update-common 2 " .s cr
            i c@ current c@ = if
                true found !
                leave
            endif
        loop
        found @ invert if
            0 i c! \ clear the letter in common if match not found
        endif
    loop
    drop
    \ ."     updated common to: " .common
;

\ Keep track of the single common character in thruples of lines
variable common-priority
0 common-priority !

: common-priority+!
    common-char dup [char] ! <> if
        priority common-priority +!
    else
        drop
    endif
;

: handle-common ( u -- )
    current-line @ 3 mod 0= if
        common-priority+!
        set-common
    else
        update-common
    then
;

: run ( c-addr u -- priority )
    0 current-line !
    0 #common !
    common common-buffer# erase
    0 common-priority !
    r/o open-file throw 0
    begin
        \ ." run loop " .s cr
        over pad 84 rot read-line throw
        \ ." run loop 2 " .s cr
    while
            dup handle-common
            \ ." run loop 3 " .s cr
            pad swap split find-dup priority +
            1 current-line +!
    repeat
    common-priority+!
    drop swap close-file throw
;