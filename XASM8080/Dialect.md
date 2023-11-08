# XASM8080 Dialect

## Projects

Assembler jobs consist of one or more assembler source files, containing the instructions
which make up a program.  There is no project file, the project contents are specified
on the assembler command line.

## Files

Assembler source files are ordinary text files, containing instructions in word form which
the assembler will convert into binary machine code.  These will typically have a filename
suffix of '.asm'.

Assembler source files will consist of zero or more assembly lines.  A line ending with a
backslash will be joined with the next line. ('\' is the line continuation character)

An assembler source file may contain an END directive.  END is a pseudo-op to mark the end
of source code.  And lines following an end directive will be ignored, through the end of
the file.

## Code Format
An assembler source line consists of four optional parts: a LABEL, an INSTRUCTION, the
INSTRUCTION ARGUMENTS, and a COMMENT.  These parts must be in this order.

```
   MAIN:  LXI  SP, 0F000H   ;init stack pointer

          CALL CLEARSCREEN  ;the label is optional
          MVI  A, '?'
          CALL WRITETTY     ;comments are optional
                            ;and so are instructions and arguments
 ```
### Labels

A Label is a symbolic name for a numeric value, often representing a memory address.  Labels
may be of three different scope types:

 - global
 - static (filewide)
 - local (relative to nearest previous static label)

Labels may be essentially unlimited in length.

A global label must begin with a '$', followed by a letter and then any number of letters,
digits, or underscores, and an optional colon (not a part of the label).  Global files may
be referenced by all files in a project.  A global label name must be unique within its
containing file (internally its name is decorated as a unique name
'$globalname@filename').

A static label must begin with a letter, followed by any number of letters,
digits, or underscores, and an optional colon (again, not a part of the label).  A static
label may be referenced anywhere in the file in which it is declared.  A static label name
must be unique within its containing file (internally its name is decorated as a unique
name 'staticname@filename').

A local label must begin with a '.', followed by a letter and then any number of letters,
digits, or underscores, and an optional colon (again, not a part of the label).  A local
label name need not be unique within its containing file.  It must be unique under its
nearest prior static label (internally its name is decorated as a unique name
'staticname.localname@filename').

### Instructions (Mnemonics, Pseudo-Ops, and Directives)

XASM8080 uses normal Intel(TM) 8080A instruction mnemonics.  Capitalization is optional,
but consistently all caps or all lowercase is more readable than mixed case.

### Instruction Arguments (Operands)

Instruction arguments take a variety of forms, for 8 bit registers, 16-bit registers,
immediate values, and strings (for the 'DB' pseudo-op).  Immediate values may be in the
form of complex expressions, i.e. ```('Z'-'A'+1)```, or ```0FC00H AND 08000H```.
Values may be decimal numbers, hexadecimal numbers, labels, or the '$' symbol (which
represents the program counter at the start of the instruction).


### Comments

Comments may contain any text.  Comments begin with a semicolon, and end at the end of
the line.