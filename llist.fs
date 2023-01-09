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