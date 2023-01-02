marker (reload)
: reload (reload) "7-task-v2.fs" included ;

\ Since each directory is only listed once, we can do a tree traversal using the
\ stack. So as we cd down, we push an empty size to the stack.
\
\ As we see a number string (a file listing), we add that size to the current
\ stack value.
\
\ When we go up a directory, we can just add the current directory's size to the
\ parent (with +)
\
\ When going up a directory, we can also check the current size, and update
\ dir-total and min-dir for the tasks

\ Part 2
\ Had to run through part 1 to get the total disk used
\   Storing here saves having to run through twice in one go
40208860 value disk-used
70000000 value disk-capacity
30000000 value space-needed
disk-used disk-capacity space-needed - - value delete-required

variable dir-total
0 dir-total !
variable min-dir
disk-capacity min-dir !
\ these don't need to do anything
: $ ;
: ls ;
: dir parse-name 2drop ;

: ..
    \ part 1 -- add smaller directories to total
    dup 100000 <= if dup dir-total +! endif
    \ part 2 -- find smallest file to delete to get required capacity
    dup delete-required >= if dup min-dir @ min min-dir ! endif
    + ; \ add to parent directory, also going back up a directory
: cd parse-name
    ".." compare 0= if ..
    else 0 endif ; \ move down a level, start a new size count

0 value fd-in
: open "7-input" r/o open-file throw to fd-in ;
: close fd-in close-file throw 0 to fd-in ;
: read ( -- c-a u more? ) pad 80 fd-in read-line throw pad -rot ;
: -d over c@ dup [char] 0 < swap [char] 9 > or ;
: number 0. 2swap >number 2drop d>s ;

: (init) 0 dir-total ! ;
: go
    (init) open
    begin read
    while -d if evaluate else number + endif repeat
    2drop close
    begin depth 1 > while .. repeat
    ." Total used space: " . cr
    ." Part 1: " dir-total @ . cr
    ." Part 2: " min-dir @ . cr ;
