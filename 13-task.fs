marker (reload)
: reload (reload) clearstack "13-task.fs" included ;

1024 constant buffer#
create left-buf buffer# allot variable (left-offset) variable (left#)
create right-buf buffer# allot variable (right-offset) variable (right#)

0 value buffer variable (offset) variable (string#)
: offset (offset) @ ;
: string# (string#) @ ;
: left left-buf to buffer (left-offset) (offset) ! (left#) (string#) ! ;
: right right-buf to buffer (right-offset) (offset) ! (right#) (string#) ! ;
: buffer-string ( -- c-addr u ) offset @ dup buffer + string# @ rot - ;
left

0 value fd-in
defer input-file
: (example) "13-example" ; : example ['] (example) is input-file ;
: (real) "13-input" ; : real ['] (real) is input-file ;
example

: open input-file r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;

false value file-more?
: line-more? offset @ string# < ;
: next-char buffer offset @ + c@ ;
: advance ( n -- ) offset dup >r @ + string# @ min r> ! ;
: line buffer buffer# fd-in read-line throw to file-more? ;
: item line string# ! ;
: pair ( -- ) left item right item ;

: list? ( -- f ) next-char [char] [ = ;
: digit? ( -- f ) next-char dup [char] 0 >= swap [char] 0 <= and ;