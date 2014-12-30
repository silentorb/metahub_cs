## MetaHub 3 ##

A declarative, data-centered, relational programming language and logic engine.
MetaHub is designed to compile to standard, object-oriented imperative languages.
Currently the only target is C++.

### The Compiler ###

Compilation is translated through 3 stages.  Each stage has it's own independent data structures.

1. MetaScript
2. Parser (tokenized data structure) ->
3. Logic (gathered requirements) ->
4. Imperative (pseudo code) ->4
5. Generated code in target language.