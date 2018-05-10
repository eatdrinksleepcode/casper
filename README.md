# Casper
A cross-platform .NET build system using the Boo language.
## Build Status
[![Build status](https://ci.appveyor.com/api/projects/status/wtuojfl6gqp9fuxx/branch/master?svg=true)](https://ci.appveyor.com/project/eatdrinksleepcode/casper/branch/master)
[![Build Status](https://travis-ci.org/eatdrinksleepcode/casper.svg?branch=master)](https://travis-ci.org/eatdrinksleepcode/casper)
## Getting Started

Casper uses previously published versions of itself to build itself. The best way to learn how to use Casper is to learn from Casper's build. 

1. Obtain Casper. Casper uses a [bootstrap script](./bootstrap) to install Casper locally from MyGet.
2. Create a "build.casper" script file and configure it to [build your solution file](./build.casper#L4).
3. To run NUnit tests, configure a "build.casper" script file in your test project to [execute a NUnit task](./Test/build.casper#L3).
4. To execute an arbitrary command, add a [Exec task](./build.casper#L6). 
