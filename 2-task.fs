marker (reload)

: reload (reload) "2-task.fs" included ;

(
This one works really well with forth, since the input is space delimited lines.
I was able to just treat 'a/b/c' and 'x/y/z' as forth words to add their
  moves to the stack, and have a 'score' function that does the scoring
)

\ Set up move data storage
: score ;
: beats 1 cells + ;
: loses-to 2 cells + ;

\ Create moves
variable rock 2 cells allot
variable paper 2 cells allot
variable scissors 2 cells allot

\ Set  move data
1 rock score !
scissors rock beats !
paper rock loses-to !

2 paper score !
rock paper beats !
scissors paper loses-to !

3 scissors score !
paper scissors beats !
rock scissors loses-to !

\ Scoring - win/lose/draw
: beats-score ( a b -- score )
    over beats @ over = if \ if a beats b (loss)
        2drop 0
    else
        = if 3 else 6 endif
    endif
;

\ Scoring - move value + win/lose/draw
: score ( a b -- score )
    dup score @
    -rot \ score a b
    beats-score +
;

: a rock ;
: b paper ;
: c scissors ;

\ Part 1 - input tells move
\ : x rock ;
\ : y paper ;
\ : z scissors ;

\ Part 2 - input tells result
: x ( opp-move -- opp-move losing-move )
    dup beats @ \ get the move that opp-move beats
;

: y ( opp-move -- opp-move draw-move )
    dup
;

: z ( opp-move -- opp-move win-move )
    dup loses-to @ \ get the move that opp-move loses to
;

variable score-total

: run-game ( c-addr u -- score )
    0 score-total !
    r/o open-file throw
    begin
        dup pad 84 rot read-line throw
    while
            pad swap evaluate \ treat each line as forth code
            score
            score-total +!
    repeat
    2drop
    score-total @
;
