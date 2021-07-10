# Andl Extended Relational Algebra

Andl is A New Database Language. See <http://andl.org>.

Andl ERA is an extended implementation of the Relational Algebra for manipulating
relational data in any .NET language. While Andl itself is a full programming 
language, Andl ERA provides the same set of queries as a C# library.

Andl ERA can perform relational queries at or beyond the capabilities 
of any SQL dialect. It can do all the ordinary things like select, where 
and join but it can also do generative queries, self-joins, complex 
aggregations, subtotals and running totals (a bit like SQL recursive 
common table expressions and windowing). 

[Note: running totals not yet implemented.]

Andl ERA has its own in-memory database so it can provide a complete 
application backend for any kind of user interface on any platform. It can 
easily be used to program a data model as a set of tables just like SQL, but 
including all the access routines, without the need for an Object Relational 
Mapper.

Andl ERA can retrieve data from Csv, Txt, Sql, Odbc and Oledb but does not
provide any persistence mechanism. (Left as an exercise for the caller!)

[Note: Sql etc not yet implemented.]

The core feature of Andl ERA is an implementation based on headings as a lit of 
attribute names. 

The main differences from SQL are:

* all joins are natural (by attribute name)
* relations (tables) have no duplicate tuples (rows)
* data values cannot be null.


* provides sources, stores and updates as well as queries
* many additional relational operations

Sample programs are included to demonstrate these capabilities. 

A future release of Andl ERA will generate SQL so that queries can be
executed on a relational database backend.

## BUILDING ANDL ERA

The source code can be downloaded from <https://github.com/david-pfx/AndlEra>.

The project should build 'out of the box' in Visual Studio 2019 with the .NET 
Framework 4.7, and possibly earlier versions. It builds an executable program 
that runs the samples, a class file generator, class library and unit tests.

## LICENCE

The source code files contain the following notice.

/// Andl is A New Data Language. See http://andl.org.
///
/// Copyright � David M. Bennett 2015-20 as an unpublished work. All rights reserved.
/// 
/// This work is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. 
/// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc/4.0/.
/// 
/// In summary, you are free to share and adapt this software freely for any non-commercial purpose provided 
/// you give due attribution and do not impose additional restrictions.
/// 
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.

Please contact me with any questions or suggestions at david@andl.org.
