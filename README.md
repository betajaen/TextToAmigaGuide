Text To AmigaGuide
==================

Version 0.1

Written by Robin Southern https://github.com/betajaen

About
-----

Converts all text files (.txt) in a directory to an AmigaGuide.

Text files may contain some very minor markdown-like formatting:

Titles:

The first line of any text file is considered the title of the Node.

Headers:

~~~
# Header 1
## Header 2
### Header 3
~~~
Code Indention:

~~~
  Code blocks is indented by at least two spaces, and must
  start with an empty line.
~~~

Bold and Underline:

~~~
 This is some *bold* text and this is some _underline_ text.
 var calc^_result = 2 ^* 2. // For escaping
~~~

Links:

~~~
 Please see the [Table of Contents](TOC) these can be also ^[Escaped^].
~~~

Usage
-----

Amiga2Markdown requires .NET Core 3.1 runtimes, so runs on Windows, Linux and MacOS.

It is launched through the console

~~~
    TextToAmigaGuide.exe --input inputpath --output Documentation.guide
~~~
