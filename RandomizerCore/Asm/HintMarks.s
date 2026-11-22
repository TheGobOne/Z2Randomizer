.include "z2r.inc"

; Archipelago: a white "!" over the head of each town NPC whose text is a helpful hint, until the player has
; read that hint. Assembled only in Archipelago mode with helpful hints on (Hyrule.AddHintMarks).
;
; A helpful hint is a text slot of ARCHIPELAGO_HINT_SIZE bytes that ends in an $FF end byte and $80 + the hint
; number (CustomTexts.ArchipelagoHintPlaceholder). The Archipelago client rewrites the text but keeps that shape.
;
; Drawing: the game draws each object through bank7_Display ($ef11), which jumps to the object's own routine at
; $ef42 with Y = the object's OAM offset. Each object owns 6 sprites there and town NPCs draw 4, so the mark goes
; in the 5th, after the object's routine has run (it hides the sprites it doesn't use). The NPC text tables and the texts are in bank 3, which is mapped while a town runs.

HINT_SIZE = 52
HINT_MARK_TILE = $e5            ; 8x16 sprite from the fixed $1C00 page: tile $e4 (the "!") over blank $e5 (ArchipelagoHintMark.cs)
HINT_MARK_PALETTE = 1           ; sprite palette: the glyph is in color 3, white in towns
HINT_MARK_OAM = $10             ; the 5th sprite of the object's OAM block
HINT_MARK_DX = 4                ; the "!" is in the middle of the 8 pixel tile, so this centers it over a 16 pixel NPC
HINT_MARK_DY = 10               ; the "!" fills the top 7 rows of the sprite, so this leaves a 3 pixel gap above the NPC
MAX_HINTS = 8                   ; bits in HintsReadMask

FIRST_NPC_TYPE = $0a
LAST_NPC_TYPE = $22
RANDOM_TEXT_FIRST = $19         ; Z2R gives these NPC types a random text, so they never hold a hint
RANDOM_TEXT_LAST = $20
STATE1_ROWS = 16                ; the dialog state 1 tables have fewer rows than the state 0 tables
SIDE_VIEW_MODE = $0b

; bank 3
TextPointerTables = $b421       ; a word for each WorldNumber: that world's text pointer table
NpcTextWestState0 = $a238       ; text number for each NPC type (row) and town (TownNumber & 3)
NpcTextWestState1 = $a29c
NpcTextEastState0 = $a2dc
NpcTextEastState1 = $a340
DialogStartWithPointerSet = $b614

ObjectDisplayDispatch = $ef42   ; ldy $91,x / jmp ($000e)
ObjectOamOffset = $91
ScreenY = $00                   ; set by bank7_Display for the object being drawn
ScreenX = $01
TextPointer = $0569

.segment "PRG7"

.org ObjectDisplayDispatch
    jmp HintMarkDisplay

.reloc
HintMarkDisplay:
    ; Draw the object first: its routine hides the sprites it doesn't use, which would hide the mark
    ldy ObjectOamOffset,x
    jsr CallObjectDisplay
    php
    pha
    txa
    pha
    tya
    pha
    lda WorldNumber
    cmp #1
    beq @Town
    cmp #2
    bne @Exit
@Town:
    ldx EnemyIndex
    cpx #6
    bcs @Exit
    lda GameMode
    cmp #SIDE_VIEW_MODE
    bne @Exit
    lda $0c
    pha
    lda $0d
    pha
    lda $0e
    pha
    lda $0f
    pha
    jsr DrawHintMark
    pla
    sta $0f
    pla
    sta $0e
    pla
    sta $0d
    pla
    sta $0c
@Exit:
    pla
    tay
    pla
    tax
    pla
    plp
    rts

CallObjectDisplay:
    jmp ($000e)

; X = object slot (also in EnemyIndex). Uses $0c-$0f.
DrawHintMark:
    lda Enemy0Status,x
    cmp #1
    bne @Done
    lda EnemyType,x
    sec
    sbc #FIRST_NPC_TYPE
    bcc @Done
    cmp #LAST_NPC_TYPE - FIRST_NPC_TYPE + 1
    bcs @Done
    cmp #RANDOM_TEXT_FIRST - FIRST_NPC_TYPE
    bcc @Known
    cmp #RANDOM_TEXT_LAST - FIRST_NPC_TYPE + 1
    bcc @Done
@Known:
    asl a
    asl a
    sta $0c                     ; row * 4: each row has a text for 4 towns
    lda #0
    sta $0d                     ; dialog state
@StateLoop:
    lda $0d
    beq @RowOk
    lda $0c
    cmp #STATE1_ROWS * 4
    bcs @Done
@RowOk:
    ; ($0e) = the text table for this world and state
    lda WorldNumber
    and #2                      ; 0 west, 2 east
    ora $0d
    asl a
    tay
    lda NpcTextTables,y
    sta $0e
    lda NpcTextTables+1,y
    sta $0f
    lda TownNumber
    and #3
    ora $0c
    tay
    lda ($0e),y
    jsr IsUnreadHintText
    bcs @Draw
    inc $0d
    lda $0d
    cmp #2
    bcc @StateLoop
@Done:
    rts
@Draw:
    ; The NPC is already drawn: put the mark above the top-left of its first 4 sprites
    ldx EnemyIndex              ; IsUnreadHintText used X; the draw routines keep the slot here
    lda ObjectOamOffset,x
    tax
    lda #$ff
    sta $0c                     ; top
    sta $0d                     ; left
    ldy #4
@Scan:
    lda SpriteYPosition,x
    cmp #$ef
    bcs @Next                   ; hidden
    cmp $0c
    bcs @Left
    sta $0c
@Left:
    lda SpriteXPosition,x
    cmp $0d
    bcs @Next
    sta $0d
@Next:
    inx
    inx
    inx
    inx
    dey
    bne @Scan
    txa
    sec
    sbc #4 * 4
    tax
    lda $0c
    cmp #$ef
    bcs @Done                   ; nothing drawn
    sbc #HINT_MARK_DY - 1       ; carry is clear here
    bcc @Done
    tay
    lda $0d
    clc
    adc #HINT_MARK_DX
    bcs @Done
    sta SpriteXPosition + HINT_MARK_OAM,x
    tya
    sta SpriteYPosition + HINT_MARK_OAM,x
    lda #HINT_MARK_TILE
    sta SpriteTilenumber + HINT_MARK_OAM,x
    lda #HINT_MARK_PALETTE
    sta SpriteAttributes + HINT_MARK_OAM,x
    rts

NpcTextTables:
    .word NpcTextWestState0, NpcTextWestState1, NpcTextEastState0, NpcTextEastState1

; A = text number in the current world's table. C = 1 if it is a helpful hint the player has not read.
; Uses $0e/$0f.
IsUnreadHintText:
    asl a
    tay
    lda WorldNumber
    asl a
    tax
    lda TextPointerTables,x
    sta $0e
    lda TextPointerTables+1,x
    sta $0f
    lda ($0e),y
    pha
    iny
    lda ($0e),y
    sta $0f
    pla
    sta $0e
    jsr HintNumber
    bcc @No
    cmp #MAX_HINTS
    bcs @Yes                    ; no bit to remember it by, so it always shows
    tax
    lda HintBits,x
    and HintsReadMask
    bne @No
@Yes:
    sec
    rts
@No:
    clc
    rts

; ($0e) = a text. C = 1 and A = the hint number if the text is a helpful hint. Uses Y.
HintNumber:
    ldy #HINT_SIZE - 2
    lda ($0e),y
    cmp #$ff
    bne @No
    iny
    lda ($0e),y
    cmp #$80
    bcc @No
    cmp #$a0
    bcs @No
    and #$1f
    sec
    rts
@No:
    clc
    rts

HintBits:
    .byte $01, $02, $04, $08, $10, $20, $40, $80

; When a dialog starts, remember a helpful hint as read
.segment "PRG3"

.org DialogStartWithPointerSet
    jsr MarkHintRead
    nop
    nop

.segment "PRG7"

.reloc
MarkHintRead:
    txa
    pha
    tya
    pha
    lda $0e
    pha
    lda $0f
    pha
    lda TextPointer
    sta $0e
    lda TextPointer+1
    sta $0f
    jsr HintNumber
    bcc @Exit
    cmp #MAX_HINTS
    bcs @Exit
    tax
    lda HintBits,x
    ora HintsReadMask
    sta HintsReadMask
@Exit:
    pla
    sta $0f
    pla
    sta $0e
    pla
    tay
    pla
    tax
    ; what the hook replaced (vanilla $b614: lda #$2a / sta $0566)
    lda #$2a
    sta $0566
    rts
