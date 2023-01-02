marker (reload)

: reload (reload) "6-task.fs" included ;

14 constant buffer-size
create buffer buffer-size chars allot
buffer buffer-size chars + constant buffer-end

variable dup?
: packet?
    false dup? !
    buffer-end buffer ?do
        buffer-end i 1 + ?do
            \ i i c@ j j c@ .s 2drop 2drop cr
            i c@ j c@ = if
                true dup? !
            endif
        loop
    loop
    dup? @ invert
;

hex
: packet?-test
    buffer buffer-size erase packet? assert( 0= )
    buffer-size 0 ?do
        i buffer i + c!
    loop
    packet? assert( 0<> )
;
decimal

0 value fd-in
: open
    fd-in if
        fd-in close-file
    endif
    "6-input" r/o open-file throw to fd-in
;

variable position
: find-packet
    buffer buffer-size erase
    open
    0 position !
    begin
        fd-in key?-file
        position @ buffer-size < packet? invert or
        and
    while
            fd-in key-file position @ buffer-size mod chars buffer + c!
            1 position +!
            \ buffer buffer-size type cr
    repeat
    position @
;